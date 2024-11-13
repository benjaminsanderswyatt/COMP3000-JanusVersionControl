using backend.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<JanusDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21)),
    mysqlOptions => mysqlOptions.EnableRetryOnFailure()));

/*
// CORS
var allowedOrigins = builder.Configuration.GetValue<string>("ALLOWED_CORS_ORIGINS")?.Split(",") ?? new string[0];
builder.Services.AddCors(options =>
{
    options.AddPolicy("EnvCorsPolicy", builder => 
    builder.WithOrigins(allowedOrigins)
        .AllowAnyMethod()
        .AllowAnyHeader());
});
*/


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

PrepDB.PrepPopulation(app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseCors("EnvCorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
