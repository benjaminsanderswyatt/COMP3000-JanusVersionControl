using backend.Helpers;
using backend.Models;
using backend.Services;
using backend.Utils.Users;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.RateLimiting;

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
builder.Services.AddScoped<AccessTokenHelper>();
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<CLIHelper>();
builder.Services.AddScoped<UserManagement>();
builder.Services.AddScoped<ProfilePicManagement>();
builder.Services.AddScoped<RepoManagement>();
builder.Services.AddScoped<RepoService>();


// Add Db context
builder.Services.AddDbContext<JanusDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21)),
    mysqlOptions => mysqlOptions.EnableRetryOnFailure()));

// Initialize database
builder.Services.AddHostedService<DatabaseInitialiser>();


// Token blacklist cleanup service
builder.Services.AddSingleton<PATBlacklistCleanupService>();
builder.Services.AddHostedService<PATBlacklistCleanupService>();



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
                    var tokenString = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                    Console.WriteLine("Token String: " + tokenString);

                    if (string.IsNullOrEmpty(tokenString))
                    {
                        context.Fail("Invalid token format.");
                        return;
                    }

                    var isBlacklisted = await janusDbContext.AccessTokenBlacklists
                    .AnyAsync(t => t.Token == tokenString && t.Expires > DateTime.UtcNow);

                    if (isBlacklisted)
                    {
                        context.Fail("The provided token is invalid or has been revoked."); // Token has been revoked
                        return;
                    }

                    var userIdClaim = context.Principal?.FindFirst("UserId")?.Value;
                    if (int.TryParse(userIdClaim, out int userId))
                    {
                        var user = await janusDbContext.Users.FindAsync(userId);
                        if (user == null)
                        {
                            context.Fail("The provided token is invalid or has been revoked.");
                            return;
                        }
                    }
                    else
                    {
                        context.Fail("The provided token is invalid or has been revoked.");
                        return;
                    }


                }
                catch (Exception ex)
                {
                    Console.WriteLine("Token validation failed: " + ex.Message);
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



// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // For frontend users
    options.AddFixedWindowLimiter("FrontendRateLimit", options =>
    {
        options.PermitLimit = 50;
        options.Window = TimeSpan.FromSeconds(60);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 10;
    });

    // For CLI users
    options.AddFixedWindowLimiter("CLIRateLimit", options =>
    {
        options.PermitLimit = 100;
        options.Window = TimeSpan.FromSeconds(60);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 20;
    });
});


var app = builder.Build();


app.UseHttpsRedirection();

app.UseRouting();


app.UseCors("CLIPolicy");

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();