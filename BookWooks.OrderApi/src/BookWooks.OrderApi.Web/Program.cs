var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SeriLogger.Configure);
builder.Services.AddSingleton<IDiagnosticsActivityLogger, DiagnosticsActivityLogger>();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
  options.CheckConsentNeeded = context => true;
  options.MinimumSameSitePolicy = SameSiteMode.None;
});

AddSwagger();
AddShowAllServicesSupport();

builder.Services.AddInfrastructureServices(builder.Configuration, builder.Environment.IsDevelopment(), InitializeDomainEventsMap(), InitializeInternalCommandMap());
builder.Services.AddUseCasesServices();
builder.Services.AddCoreServices();
builder.Services.AddOpenTelemetryTracing(builder.Configuration);
builder.Services.AddOpenTelemetryMetrics(builder.Configuration);


AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

OrderCompositionRoot.SetServiceProvider(app.Services);

if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseShowAllServicesMiddleware();
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


//app.UseFastEndpoints().UseSwaggerGen(); // FastEndpoints middleware
app.Run();

void AddAuthentication()
{
  builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
      options.Authority = builder.Configuration["IdentityServerURL"];
      options.RequireHttpsMetadata = false;
      options.Audience = "resource_order";
    });
}
void AddSwagger()
{
  builder.Services.SwaggerDocument(o =>
  {
    o.ShortSchemaNames = true;
  });

  builder.Services.AddSwaggerGen(swg =>
  {
    swg.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Microservice", Version = "v1" });
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
}
void AddShowAllServicesSupport()
{
  // add list services for diagnostic purposes - see https://github.com/ardalis/AspNetCoreStartupServices
  builder.Services.Configure<ServiceConfig>(config =>
  {
    config.Services = new List<ServiceDescriptor>(builder.Services);

    config.Path = "/listservices";
  });
}
BiDirectionalDictionary<string, Type> InitializeDomainEventsMap()
{
  var map = new BiDirectionalDictionary<string, Type>();
  map.Add("OrderCreatedEvent", typeof(OrderCreatedDomainEvent));
  return map;
}

BiDirectionalDictionary<string, Type> InitializeInternalCommandMap()
{
  var map = new BiDirectionalDictionary<string, Type>();
  map.Add("CompletePaymentInternalCommand", typeof(CompletePaymentInternalCommand));
  return map;
}
// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
  protected Program(){}
}
