using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using moah_api.Models;
using moah_api.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("MongoDB");
MongoClient client = new(connectionString);
IMongoCollection<User> usersCollection = client.GetDatabase("moah").GetCollection<User>("users");
IMongoCollection<JournalEntry> journalEntriesCollection = client.GetDatabase("moah").GetCollection<JournalEntry>("journal_entries");

builder.Services.AddControllers();
builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddSingleton(usersCollection);
builder.Services.AddSingleton(journalEntriesCollection);
builder.Services.AddSingleton(builder.Configuration);
builder.Services.AddScoped<TokenSigner>();
builder.Services.AddScoped<TokenDecryptor>();
builder.Services.AddAuthentication().AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["TokenSecret"]!)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:3000")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseHttpLogging();

app.MapControllers();

app.Run();
