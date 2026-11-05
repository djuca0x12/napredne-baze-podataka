using Neo4j.Driver;
using StackExchange.Redis;
using AsteroidTracker.Repositories;
using AsteroidTracker.Services;

var builder = WebApplication.CreateBuilder(args);

// NASA API servis - vrsimo registraciju, 
// da bi mogao da se obezbedi HttpClient!
builder.Services.AddHttpClient<SimpleNasaService>();

// Servis za izracunavanje slicnosti izmedju asteroida
builder.Services.AddSingleton<AsteroidSimilarityService>();

// Neo4j drajver
builder.Services.AddSingleton<IDriver>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var uri = config.GetConnectionString("Neo4j")!;
    var user = config["Neo4j:User"]!;
    var password = config["Neo4j:Password"]!;

    return GraphDatabase.Driver(
        uri, AuthTokens.Basic(user, password));
});

// Redis konekcija
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    return ConnectionMultiplexer.Connect(
        config.GetConnectionString("Redis")!);
});

// Neo4j repozitorijum - koji cuva podatke u Neo4J-u.
builder.Services.AddSingleton<Neo4jAsteroidRepository>();

// Servis koji periodicno preuzimanje podataka.
builder.Services.AddHostedService<AsteroidBackgroundService>();

// Redis:
builder.Services.AddSingleton<RedisCacheService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("CORS", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .WithOrigins(
                  "http://localhost:5500",
                  "https://localhost:5500",
                  "http://127.0.0.1:5500",
                  "https://127.0.0.1:5500");
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CORS");
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
