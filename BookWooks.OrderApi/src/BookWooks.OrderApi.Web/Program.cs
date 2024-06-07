var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SeriLogger.Configure);
builder.Services.Configure<CookiePolicyOptions>(options =>
{
  options.CheckConsentNeeded = context => true;
  options.MinimumSameSitePolicy = SameSiteMode.None;
});


builder.Services.AddFastEndpoints()
                .SwaggerDocument(o =>
                  {
                    o.ShortSchemaNames = true;
                  });

AddShowAllServicesSupport();
builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddUseCasesServices();
builder.Services.AddCoreServices();
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
app.UseFastEndpoints().UseSwaggerGen(); // FastEndpoints middleware
app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseHttpsRedirection();
app.Run();

void AddShowAllServicesSupport()
{
  // add list services for diagnostic purposes - see https://github.com/ardalis/AspNetCoreStartupServices
  builder.Services.Configure<ServiceConfig>(config =>
  {
    config.Services = new List<ServiceDescriptor>(builder.Services);

    // optional - default path to view services is /listallservices - recommended to choose your own path
    config.Path = "/listservices";
  });
}
// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
  protected Program(){}
}
