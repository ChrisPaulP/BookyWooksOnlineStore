using BookyWooks.Identity;
using BookyWooks.Identity.Data;
using BookyWooks.Identity.Models;
using BookyWooks.Identity.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using BookyWooks.Messaging.MassTransit;
using Tracing;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddLocalApiAuthentication();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<IdentityDbContext>()
    .AddDefaultTokenProviders();

// Configure IdentityServer with various options and resources
builder.Services.AddIdentityServer(options =>
{
    options.Events.RaiseErrorEvents = true;     // Enable raising error events
    options.Events.RaiseInformationEvents = true; // Enable raising information events  
    options.Events.RaiseFailureEvents = true;  // Enable raising failure events    
    options.Events.RaiseSuccessEvents = true; // Enable raising success events   
    options.EmitStaticAudienceClaim = true; // Emit a static audience claim   
    options.IssuerUri = "BookyWooks";  // Set the issuer URI
})

    .AddInMemoryIdentityResources(Configuration.IdentityResources)     // Add in-memory identity resources                                                                       
    .AddInMemoryApiResources(Configuration.ApiResources) // Add in-memory API resources                                                         
    .AddInMemoryApiScopes(Configuration.ApiScopes)
    .AddInMemoryClients(Configuration.Clients)  // Add in-memory clients                                                
    .AddAspNetIdentity<ApplicationUser>() // Add ASP.NET Identity support
    .AddDeveloperSigningCredential() // Add a developer signing credential                                
    .AddResourceOwnerValidator<IdentityResourceOwnerPasswordValidator>(); // Add a resource owner validator

builder.Services.AddAuthentication();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity Microservice", Version = "v1" });
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
builder.Services.AddMessageBroker<IdentityDbContext>(builder.Configuration, Assembly.GetExecutingAssembly(), true);
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
app.UseMiddleware<LogContextMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity Microservice"));
app.UseAuthentication();
app.UseAuthorization();
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.Run();
