using Microsoft.OpenApi.Models;
using System.Reflection;


var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSwaggerGen(swg =>
                  {
                    swg.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Microservice", Version = "v1" });
                    // To Enable authorization using Swagger (JWT)    
                    swg.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                    {
                      Name = "Authorization",
                      Type = SecuritySchemeType.ApiKey,
                      Scheme = "Bearer",
                      BearerFormat = "JWT",
                      In = ParameterLocation.Header,
                      Description = "Hi Chris Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                    });
                    swg.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    swg.IncludeXmlComments(xmlPath);
                  });

AddShowAllServicesSupport();

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment.IsDevelopment());
builder.Services.AddUseCasesServices();
builder.Services.AddCoreServices();
builder.Services.AddOpenTelemetryTracing(builder.Configuration);
builder.Services.AddOpenTelemetryMetrics(builder.Configuration);
//AddAuthenticationJwtBearer()
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
                                {
                                  options.Authority = builder.Configuration["IdentityServerURL"];
                                  options.RequireHttpsMetadata = false;
                                  options.Audience = "resource_order";
                                });

builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();

app.UseOpenTelemetryPrometheusScrapingEndpoint();


app.UseFastEndpoints().UseSwaggerGen(); // FastEndpoints middleware
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
