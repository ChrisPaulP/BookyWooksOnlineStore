using BookyWooks.Catalogue.Api.Data;
using System.Reflection;
using BookyWooks.Messaging.MassTransit;
using Microsoft.EntityFrameworkCore;
using BookyWooks.Catalogue.Api.MassTransit;
using Serilog;
using Logging;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(SeriLogger.Configure);
// Add services to the container.
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
});
builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Book Catalogue Microservice", Version = "v1" });
    // To Enable authorization using Swagger (JWT)    
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement { { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, Array.Empty<string>() } });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

builder.Services.AddAuthentication("Bearer")
           .AddJwtBearer("Bearer", options =>
           {
               options.Authority = builder.Configuration["IdentityServerURL"];
               options.Audience = "resource_bookcatalogue";
               options.RequireHttpsMetadata = false;
           });
builder.Services.AddAuthorization();
//builder.Services.AddMarten(opts =>
//{
//    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
//}).UseLightweightSessions();
builder.Services.AddDbContext<CatalogueDbContext>(x =>
{
    x.UseNpgsql(builder.Configuration.GetConnectionString("CatalogueDatabase")!, opt =>
    {
        var x = builder.Configuration.GetConnectionString("CatalogueDatabase");
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

    await app.InitialiseDatabaseAsync();
}
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Book Catalogue Microservice"));
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//app.UseOpenTelemetryPrometheusScrapingEndpoint();

await FetchData();

async Task FetchData()
{
    using var client = new HttpClient();
    client.BaseAddress = new Uri("https://www.microsoft.com/");
    var resp = await client.GetAsync(string.Empty);
    var html = await resp.Content.ReadAsStringAsync();
}

app.Run();
