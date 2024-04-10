
using System.Reflection;
using Autofac.Core;
using BookWooks.OrderApi.Core.OrderAggregate.IntegrationEvents;
using BookWooks.OrderApi.UseCases;



var builder = WebApplication.CreateBuilder(args);

//It appears that the class you provided is indeed using Autofac as the dependency injection container. This can be inferred from the following lines of code:
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.UseSerilog((_, config) => config.ReadFrom.Configuration(builder.Configuration));
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
var app = builder.Build();


if (app.Environment.IsDevelopment())
{
  app.UseDeveloperExceptionPage();
  app.UseShowAllServicesMiddleware(); // see https://github.com/ardalis/AspNetCoreStartupServices
}
else
{
  app.UseDefaultExceptionHandler(); // from FastEndpoints
  app.UseHsts();
}
app.UseFastEndpoints();
app.UseSwaggerGen(); // FastEndpoints middleware

app.UseHttpsRedirection();

// Seed Database
using (var scope = app.Services.CreateScope())
{
  var services = scope.ServiceProvider;
  try
  {
    var bookyWooksOrderDbContext = services.GetRequiredService<BookyWooksOrderDbContext>();

    bookyWooksOrderDbContext.Database.EnsureCreated();



#pragma warning disable S125 // Sections of code should not be commented out
    //var integrationEventLogDbContext = services.GetRequiredService<IntegrationEventLogDbContext>();
    //integrationEventLogDbContext.Database.Migrate();

    // To find difference between Migrate and EnsureCreated, see ProgramNotes.txt file


    SeedData.Initialize(services);
#pragma warning restore S125 // Sections of code should not be commented out
  }
  catch (Exception ex)
  {
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred seeding the DB. {ExceptionMessage}", ex.Message);
  }
}

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
  protected Program()
  {
      
  }
}
