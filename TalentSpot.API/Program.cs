using Microsoft.EntityFrameworkCore;
using TalentSpot.Application.Services.Concrete;
using TalentSpot.Application.Services;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Infrastructure.Repositories;
using TalentSpot.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TalentSpot.Infrastructure.ElasticSearch;
using Nest;
using TalentSpot.Infrastructure.Interfaces;
using TalentSpot.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
#region Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
#endregion

// Redis Configuration
#region Redis Configuration
var redisConfiguration = builder.Configuration.GetSection("Redis:Configuration").Value;
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = redisConfiguration;
});
#endregion

// Elasticsearch Configuration
#region Elasticsearch Configuration
var elasticsearchOptions = builder.Configuration.GetSection("Elasticsearch");
var uri = elasticsearchOptions["Uri"];
var username = elasticsearchOptions["Username"];
var password = elasticsearchOptions["Password"];

// Register the setup class
builder.Services.AddSingleton<ElasticsearchSetup>();

// Configure the ElasticClient
builder.Services.AddSingleton<IElasticClient>(provider =>
{
    var settings = new ConnectionSettings(new Uri(uri))
        .EnableDebugMode() // Optional: for detailed debugging info
        .ServerCertificateValidationCallback((o, certificate, chain, errors) => true)
        .DefaultIndex("jobs")
        .BasicAuthentication(username, password);

    return new ElasticClient(settings);
});
#endregion

// Dependency Injection
#region Dependency Injection
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IForbiddenWordService, ForbiddenWordService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IWorkTypeRepository, WorkTypeRepository>();
builder.Services.AddScoped<IBenefitRepository, BenefitRepository>();
builder.Services.AddScoped<IJobBenefitRepository, JobBenefitRepository>();
builder.Services.AddScoped<IForbiddenWordRepository, ForbiddenWordRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IWorkTypeService, WorkTypeService>();
builder.Services.AddScoped<IBenefitService, BenefitService>();
builder.Services.AddScoped<ICacheService, CacheService>();
#endregion

// JWT Authentication Configuration
#region JWT Authentication Configuration
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
            provider.GetRequiredService<ICacheService>(),
            jwtSecret));
#endregion

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Check Database Connection
#region Database Connection Check
try
{
    using (var scope = builder.Services.BuildServiceProvider().CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        dbContext.Database.OpenConnection(); // Try to open the connection
        dbContext.Database.CloseConnection(); // Close the connection
        Console.WriteLine("Database connection successful.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Database connection failed: " + ex.Message);
}
#endregion

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