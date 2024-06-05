
using System.Reflection;
using Autofac.Core;
using BookWooks.OrderApi.Infrastructure.Data.Extensions;
using BookWooks.OrderApi.UseCases;

using Logging;
using Tracing;




var builder = WebApplication.CreateBuilder(args);

//It appears that the class you provided is indeed using Autofac as the dependency injection container. This can be inferred from the following lines of code:
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.UseSerilog(SeriLogger.Configure);

builder.Services.Configure<CookiePolicyOptions>(options =>
{
  options.CheckConsentNeeded = context => true;
  options.MinimumSameSitePolicy = SameSiteMode.None;
});

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument(o =>
{
  o.ShortSchemaNames = true;
});
#pragma warning restore S125 // Sections of code should not be commented out

// add list services for diagnostic purposes - see https://github.com/ardalis/AspNetCoreStartupServices
builder.Services.Configure<ServiceConfig>(config =>
{
  config.Services = new List<ServiceDescriptor>(builder.Services);

  // optional - default path to view services is /listallservices - recommended to choose your own path
  config.Path = "/listservices";
});

builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
  containerBuilder.RegisterModule(new AutofacCoreModule());
  containerBuilder.RegisterModule(new AutofacUseCasesModule(builder.Environment.IsDevelopment()));
  containerBuilder.RegisterModule(new AutofacInfrastructureModule(builder.Environment.IsDevelopment(), builder.Configuration));
});

builder.Services.AddInfrastructureMessagingServices(builder.Configuration);
builder.Services.AddOpenTelemetryTracing(builder.Configuration);
builder.Services.AddOpenTelemetryMetrics(builder.Configuration);
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
  await app.InitialiseDatabaseAsync();
}
else
{
  app.UseDefaultExceptionHandler(); // from FastEndpoints
  app.UseHsts();
}
app.UseMiddleware<LogContextMiddleware>();
app.UseFastEndpoints();
app.UseSwaggerGen(); // FastEndpoints middleware
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseHttpsRedirection();
app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
  protected Program()
  {
      
  }
}
