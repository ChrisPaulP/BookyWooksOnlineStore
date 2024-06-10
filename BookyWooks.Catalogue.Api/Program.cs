using BookyWooks.Catalogue.Api.Data;
using System.Reflection;
using BookyWooks.Messaging.MassTransit;
using Microsoft.EntityFrameworkCore;
using BookyWooks.Catalogue.Api.MassTransit;
using Serilog;
using Logging;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);
// Add services to the container.
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
});
builder.Services.AddControllers();

//builder.Services.AddMarten(opts =>
//{
//    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
//}).UseLightweightSessions();
builder.Services.AddDbContext<CatalogueDbContext>(x =>
{
    x.UseNpgsql(builder.Configuration.GetConnectionString("Database")!, opt =>
    {
        var x = builder.Configuration.GetConnectionString("Database");
        opt.EnableRetryOnFailure(5);
    });
});
builder.Services.AddScoped<IMassTransitService, CatalogueMassTransitService>();
//if (builder.Environment.IsDevelopment())
//builder.Services.InitializeMartenWith<CatalogueInitialData>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMessageBroker<CatalogueDbContext>(builder.Configuration, Assembly.GetExecutingAssembly(), false);
builder.Services.AddOpenTelemetryTracing(builder.Configuration);
builder.Services.AddOpenTelemetryMetrics(builder.Configuration);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitialiseDatabaseAsync();
}

app.UseHttpsRedirection();
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.MapControllers();
app.Run();
