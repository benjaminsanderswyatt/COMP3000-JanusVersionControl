using backend.Helpers;
using backend.Models;
using backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


// Configure Kestrel server
var certificatePath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path");
certificatePath = string.IsNullOrEmpty(certificatePath) ? "https/backend-certificate.pfx" : certificatePath; // Path cant be null or empty during configuration
var certificatePassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = new X509Certificate2(certificatePath, certificatePassword);
    });
});

// Dependancy Injection
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<AccessTokenHelper>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<CLIHelper>();


// Add Db context
builder.Services.AddDbContext<JanusDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21)),
    mysqlOptions => mysqlOptions.EnableRetryOnFailure()));


// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer("UserJWT", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "FrontendIssuer",
            ValidAudience = "FrontendAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey_Web")))
        };
    })
    .AddJwtBearer("AccessTokenJWT", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "CLIIssuer",
            ValidAudience = "CLIAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Environment.GetEnvironmentVariable("JWT__SecretKey_CLI")))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine("Authorization Header: " + authHeader);

                return Task.CompletedTask;
            },
            OnTokenValidated = async context =>
            {
                try
                {
                    var janusDbContext = context.HttpContext.RequestServices.GetRequiredService<JanusDbContext>();
                    var accessTokenHelper = context.HttpContext.RequestServices.GetRequiredService<AccessTokenHelper>();
                    var rawToken = context.SecurityToken; //  as JwtSecurityToken
                    Console.WriteLine("Raw token: " + rawToken);

                    if (rawToken == null)
                    {
                        Console.WriteLine("Invalid token format.");
                        context.Fail("Invalid token format.");
                        return;
                    }

                    var tokenString = rawToken.ToString();

                    var isBlacklisted = await janusDbContext.AccessTokenBlacklists
                    .AnyAsync(t => t.Token == tokenString && t.Expires > DateTime.UtcNow);

                    if (isBlacklisted)
                    {
                        Console.WriteLine("Invalid token.");
                        context.Fail("Invalid token."); // Token has been revoked
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Token validation failed.");
                    context.Fail("Token validation failed.");
                }

            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("Auth failed: " + context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

// Authorization
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("FrontendPolicy", policy =>
    policy.AddAuthenticationSchemes("UserJWT").RequireClaim("TokenType", "User"))
    .AddPolicy("CLIPolicy", policy =>
    policy.AddAuthenticationSchemes("AccessTokenJWT").RequireClaim("TokenType", "PAT"));




builder.Services.AddControllers();


// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", builder =>
    {
        builder.WithOrigins("https://localhost");
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
        builder.AllowCredentials();
    });

    options.AddPolicy("CLIPolicy", builder =>
    {
        builder.AllowAnyOrigin();
        builder.AllowAnyHeader();
        builder.AllowAnyMethod();
    });
});


var app = builder.Build();

PrepDB.PrepPopulation(app);

app.UseHttpsRedirection();

app.UseRouting();


app.UseCors("CLIPolicy");


app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
