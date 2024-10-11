using Microsoft.EntityFrameworkCore;
using Nest;
using TalentSpot.Application.Services.Concrete;
using TalentSpot.Application.Services;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Infrastructure.Repositories;
using TalentSpot.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Redis Configuration
var redisConfiguration = builder.Configuration.GetSection("Redis:Configuration").Value;
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfiguration;
});

// Elasticsearch Configuration
var elasticsearchOptions = builder.Configuration.GetSection("Elasticsearch");
var uri = elasticsearchOptions["Uri"];
var username = elasticsearchOptions["Username"];
var password = elasticsearchOptions["Password"];

// Elasticsearch client configuration
var settings = new ConnectionSettings(new Uri(uri))
    .DefaultIndex("your_default_index")
    .BasicAuthentication(username, password);

var client = new ElasticClient(settings);

// Dependency Injection
builder.Services.AddSingleton<IElasticClient>(client);
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IForbiddenWordService, ForbiddenWordService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// JWT Authentication Ayarlarý
var jwtSecret = builder.Configuration["Jwt:Secret"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "yourapp",
        ValidAudience = "yourapp",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddScoped<IUserService, UserService>(provider =>
        new UserService(
            provider.GetRequiredService<IUserRepository>(),
            provider.GetRequiredService<IUnitOfWork>(),
            provider.GetRequiredService<ICompanyRepository>(),
            provider.GetRequiredService<IDistributedCache>(),
            jwtSecret));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
