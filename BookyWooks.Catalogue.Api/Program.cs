using BookyWooks.Catalogue.Api.Data;
using Marten;

using System.Reflection;
using BookyWooks.Messaging.MassTransit;
using MassTransit;
using System;
using Microsoft.EntityFrameworkCore;
using BookyWooks.Catalogue.Api.MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    //config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    //config.AddOpenBehavior(typeof(LoggingBehavior<,>));
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
        opt.EnableRetryOnFailure(5);
    });
});
builder.Services.AddScoped<IMassTransitService, CatalogueMassTransitService>();
//if (builder.Environment.IsDevelopment())
//builder.Services.InitializeMartenWith<CatalogueInitialData>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddMessageBroker<CatalogueDbContext>(builder.Configuration, Assembly.GetExecutingAssembly(), false);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await app.InitialiseDatabaseAsync();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
