using BookWooks.OrderApi.Web.Configuration;
using BookyWooks.Messaging.Messages.InitialMessage;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SeriLogger.Configure);
builder.Services.AddSingleton<IDiagnosticsActivityLogger, DiagnosticsActivityLogger>();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.None;
});

// Configure services
builder.Services
    .AddShowAllServicesSupport()
    .AddValidatorsFromAssemblyContaining<CreateOrderRequestValidator>();

Console.WriteLine($"[DEBUG] ENVIRONMENT NAME: {builder.Environment.EnvironmentName}");

// Configure core services
var services = builder.Services;

// Add core infrastructure
services.AddInfrastructureServices(
    builder.Configuration,
    builder.Environment.IsDevelopment(),
    DomainEventConfiguration.InitializeDomainEventsMap(),
    DomainEventConfiguration.InitializeInternalCommandMap());

// Add use cases and core services separately
services.AddUseCasesServices();
services.AddCoreServices();

// Configure OpenTelemetry
var tracingEnabled = builder.Configuration.GetValue<bool>("OpenTelemetry:TracingEnabled");
if (tracingEnabled)
{
    var telemetryServices = builder.Services;
    telemetryServices.AddOpenTelemetryTracing(builder.Configuration);
    telemetryServices.AddOpenTelemetryMetrics(builder.Configuration);
}

// Configure API and Security
services.RegisterEndpoints<IEndpoint>();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen(swg =>
{
    swg.SwaggerDoc("v1", new OpenApiInfo { Title = "Order Microservice", Version = "v1" });
    swg.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
    });
    swg.AddSecurityRequirement(new OpenApiSecurityRequirement 
    { 
        { 
            new OpenApiSecurityScheme 
            { 
                Reference = new OpenApiReference 
                { 
                    Type = ReferenceType.SecurityScheme, 
                    Id = "Bearer" 
                } 
            }, 
            Array.Empty<string>() 
        } 
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    swg.IncludeXmlComments(xmlPath);
});

// Configure Authentication
var authBuilder = services.AddAuthentication("Bearer");
authBuilder.AddJwtBearer("Bearer", options =>
{
    options.Authority = builder.Configuration["IdentityServerURL"];
    options.RequireHttpsMetadata = false;
    options.Audience = "resource_order";
});

services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage()
       .UseShowAllServicesMiddleware();
    await app.InitialiseDatabaseAsync();
}
else
{
    app.UseMiddleware<ExceptionHandlingMiddleware>()
       .UseHsts();
}

app.UseMiddleware<LogContextMiddleware>()
   .UseAuthentication()
   .UseAuthorization();

// Configure OpenTelemetry endpoint
if (builder.Configuration.GetValue<bool>("OpenTelemetry:MetricsEnabled"))
{
    app.UseOpenTelemetryPrometheusScrapingEndpoint();
}

// Configure Swagger
app.UseSwagger()
   .UseSwaggerUI();

// Map endpoints
app.MapEndpoints();

app.Run();

// Make the implicit Program class public for integration tests
public partial class Program
{
    protected Program() { }
}
