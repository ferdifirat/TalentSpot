using Microsoft.EntityFrameworkCore;
using Nest;
using TalentSpot.Application.Services.Concrete;
using TalentSpot.Application.Services;
using TalentSpot.Domain.Interfaces;
using TalentSpot.Infrastructure.Data;
using TalentSpot.Infrastructure.Repositories;

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
    .DefaultIndex("your_default_index") // Varsayýlan indeks adýný buraya ekleyin
    .BasicAuthentication(username, password);

var client = new ElasticClient(settings);

// Dependency Injection
builder.Services.AddSingleton<IElasticClient>(client);
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<IJobRepository, JobRepository>();
builder.Services.AddScoped<IForbiddenWordsService, ForbiddenWordsService>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
