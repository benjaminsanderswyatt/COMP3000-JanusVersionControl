using backend.Models;
using backend.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/*
// Configure Kestrel server
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(9000); // HTTP for redirection
    options.ListenAnyIP(9001, listenOptions => listenOptions.UseHttps()); // HTTPS
});

// Add services and middleware
builder.Services.AddHttpsRedirection(options =>
{
    options.RedirectStatusCode = StatusCodes.Status308PermanentRedirect; // Use permanent redirects
    options.HttpsPort = 9001; // Match your HTTPS port
});
*/

// Add services to the container.
builder.Services.AddDbContext<JanusDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21)),
    mysqlOptions => mysqlOptions.EnableRetryOnFailure()));


// CORS
var allowedOrigins = builder.Configuration.GetValue<string>("ALLOWED_CORS_ORIGINS")?.Split(",") ?? new string[0];
builder.Services.AddCors(options =>
{
    options.AddPolicy("EnvCorsPolicy", builder => 
    builder.WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader());
});


// Dependancy Injection
builder.Services.AddScoped<UserService>();




builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

PrepDB.PrepPopulation(app);

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

//app.UseCors("EnvCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
