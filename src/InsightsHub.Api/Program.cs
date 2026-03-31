using InsightsHub.Api.Data;
using InsightsHub.Api.Data.Seed;
using InsightsHub.Api.Endpoints;
using InsightsHub.Api.Repositories;
using InsightsHub.Api.Services;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development
});

var cs = builder.Configuration.GetConnectionString("InsightsHubConnection");
cs += $"Server={builder.Configuration["db-host"]};" +
      $"Database={builder.Configuration["db-name"]};" +
      $"Password={builder.Configuration["db-password"]};" +
      $"User ID={builder.Configuration["db-username"]};";

builder.Services.AddDbContext<InsightsHubDbContext>(opt =>
    opt.UseNpgsql(cs, o => o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)));

builder.Services.AddInsightsHubRepositories();
builder.Services.AddInsightsHubServices();
builder.Services.AddHttpClient();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
        p.WithOrigins("http://localhost:4200")
         .AllowAnyHeader()
         .AllowAnyMethod()));
}

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InsightsHubDbContext>();
    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseCors();
}
else
{
    app.UseHsts();
    app.UseHttpsRedirection();
    app.UseStaticFiles();
}

app.UseRouting();

// API endpoints
app.MapDashboardEndpoints();
app.MapOpportunityEndpoints();
app.MapFeedbackEndpoints();
app.MapSourcesEndpoints();
app.MapAiEndpoints();

// SPA fallback — serves index.html for all non-API routes in production
if (!app.Environment.IsDevelopment())
{
    app.MapFallbackToFile("index.html");
}

app.Run();
