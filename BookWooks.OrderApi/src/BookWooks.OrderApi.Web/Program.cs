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
  containerBuilder.RegisterModule(new AutofacRabbitMQModule( builder.Configuration));
});

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

    var integrationEventLogDbContext = services.GetRequiredService<IntegrationEventLogDbContext>();
    integrationEventLogDbContext.Database.Migrate();

    // To find difference between Migrate and EnsureCreated, see ProgramNotes.txt file


    SeedData.Initialize(services);
  }
  catch (Exception ex)
  {
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred seeding the DB. {exceptionMessage}", ex.Message);
  }
}
var serviceProvider = app.Services;
try
{
  var eventBus = serviceProvider.GetRequiredService<EventBus.IEventBus>();
}
catch (Exception ex)
{
  var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
  logger.LogError(ex, "An error occurred creating the event bus. {exceptionMessage}", ex.Message);
}
//await eventBus.Subscribe<CheckBookStockIntegrationEvent, BookStockCheckedEventHandler>(); Example of how to subscribe to events
app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program
{
}
