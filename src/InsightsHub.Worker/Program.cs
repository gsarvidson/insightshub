using InsightsHub.Api.Data;
using InsightsHub.Api.Repositories;
using InsightsHub.Worker;
using Microsoft.EntityFrameworkCore;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = Host.CreateApplicationBuilder(args);

var cs = builder.Configuration.GetConnectionString("InsightsHubConnection") ?? "Port=5432;";
cs += $"Server={builder.Configuration["db-host"]};" +
      $"Database={builder.Configuration["db-name"]};" +
      $"Password={builder.Configuration["db-password"]};" +
      $"User ID={builder.Configuration["db-username"]};";

builder.Services.AddDbContext<InsightsHubDbContext>(opt =>
    opt.UseNpgsql(cs, o => o.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)));

builder.Services.AddHttpClient();
builder.Services.AddInsightsHubRepositories();
builder.Services.AddHostedService<FeedbackScannerWorker>();

var host = builder.Build();
host.Run();
