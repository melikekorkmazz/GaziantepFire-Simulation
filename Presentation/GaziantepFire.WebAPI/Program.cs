using GaziantepFire.Application.Interfaces;
using GaziantepFire.Infrastructure.BackgroundServices;
using GaziantepFire.Infrastructure.Services;
using GaziantepFire.Persistence.Context;
using GaziantepFire.Persistence.Seed;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────
builder.Services.AddDbContext<GaziantepFireDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=gaziantepfire.db"));

// ── HttpClient for external Gaziantep API ─────────────────────────────────
builder.Services.AddHttpClient("GaziantepApi", client =>
{
    client.BaseAddress = new Uri("https://acikveriapi.gaziantep.bel.tr/");
    client.Timeout = TimeSpan.FromSeconds(300);
});

// ── Application Services ──────────────────────────────────────────────────
builder.Services.AddScoped<IStationOptimizationService, StationOptimizationService>();
builder.Services.AddScoped<IIncidentSyncService, IncidentSyncService>();
builder.Services.AddScoped<ISimulationService, SimulationService>();
builder.Services.AddSingleton<KmlImportService>();

// ── Background Services ───────────────────────────────────────────────────
builder.Services.AddHostedService<DailyIncidentSyncHostedService>();

// ── Controllers & CORS ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueClient", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();

// ── Seed initial data (KML → DB, with mock fallback) ─────────────────────
using (var scope = app.Services.CreateScope())
{
    var context    = scope.ServiceProvider.GetRequiredService<GaziantepFireDbContext>();
    var kmlService = scope.ServiceProvider.GetRequiredService<KmlImportService>();
    var webRoot    = app.Environment.WebRootPath
                     ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

    var kmlDir         = Path.Combine(webRoot, "data");
    var districts      = kmlService.ParseDistricts(Path.Combine(kmlDir, "districts.kml"));
    var neighborhoods  = kmlService.ParseNeighborhoods(Path.Combine(kmlDir, "neighborhoods.kml"), districts);
    var stations       = kmlService.ParseFireStations(Path.Combine(kmlDir, "stations.kml"));

    await GaziantepFireSeedData.SeedAsync(context, districts, neighborhoods, stations);
}

// ── HTTP Pipeline ─────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowVueClient");
app.UseStaticFiles();          // serve wwwroot/data KML files if needed
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
