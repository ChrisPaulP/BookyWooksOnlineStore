using BookWooks.OrderApi.Infrastructure.Data;


using BookWooks.OrderApi.Web;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Testcontainers.RabbitMq;
using Xunit;

namespace BookWooks.OrderApi.FunctionalTests;
public class CustomWebApplicationFactory<TProgram> : WebApplicationFactory<TProgram>, IAsyncLifetime where TProgram : class
{
  /// <summary>
  /// Overriding CreateHost to avoid creating a separate ServiceProvider per this thread:
  /// https://github.com/dotnet-architecture/eShopOnWeb/issues/465
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  /// 
  private readonly RabbitMqContainer _rabbitMqContainer;
  private const string RabbitMqUsername = "guest";
  private const string RabbitMqPassword = "guest";
  private const string RabbitMqHostname = "rabbitmq";
  public CustomWebApplicationFactory()
  {
    _rabbitMqContainer = new RabbitMqBuilder()
                .WithImage("rabbitmq:3.11")
                 .WithPortBinding(5672, 5672)
                 .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUsername)
                 .WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
                 .WithEnvironment("RabbitMQConfiguration__Config__HostName", RabbitMqHostname)
                 .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672))
                .Build();
    //_rabbitMqContainer = new RabbitMqBuilder()
    //                   .WithImage("rabbitmq:3.11")
    //                   .WithPortBinding(5672, 5672)
    //     .WithEnvironment("RABBITMQ_DEFAULT_USER", RabbitMqUsername)
    //     .WithEnvironment("RABBITMQ_DEFAULT_PASS", RabbitMqPassword)
    //     .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(5672)) // Adding wait strategy
    //     .Build();
  }
  protected override IHost CreateHost(IHostBuilder builder)
  {
    builder.UseEnvironment("Development"); // will not send real emails
    var host = builder.Build();
    host.Start();

    // Get service provider.
    var serviceProvider = host.Services;

    // Create a scope to obtain a reference to the database
    // context (AppDbContext).
    using (var scope = serviceProvider.CreateScope())
    {
      var scopedServices = scope.ServiceProvider;
      var db = scopedServices.GetRequiredService<BookyWooksOrderDbContext>();

      var logger = scopedServices
          .GetRequiredService<ILogger<CustomWebApplicationFactory<TProgram>>>();

      // Ensure the database is created.
      db.Database.EnsureCreated();

      try
      {
        // Can also skip creating the items
        //if (!db.ToDoItems.Any())
        //{
        // Seed the database with test data.
        SeedData.PopulateTestData(db);
        //}
      }
      catch (Exception ex)
      {
        logger.LogError(ex, "An error occurred seeding the " +
                            "database with test messages. Error: {exceptionMessage}", ex.Message);
      }
    }

    return host;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    builder
        .ConfigureServices(services =>
        {
          // Remove the app's ApplicationDbContext registration.
          var descriptor = services.SingleOrDefault(
          d => d.ServiceType ==
              typeof(DbContextOptions<BookyWooksOrderDbContext>));

          if (descriptor != null)
          {
            services.Remove(descriptor);
          }

          // This should be set for each individual test run
          string inMemoryCollectionName = Guid.NewGuid().ToString();

          // Add ApplicationDbContext using an in-memory database for testing.
          services.AddDbContext<BookyWooksOrderDbContext>(options =>
          {
            options.UseInMemoryDatabase(inMemoryCollectionName);
          });

          //var descriptor2 = services.SingleOrDefault(
          //            d => d.ServiceType == typeof(ConnectionFactory));

          //if (descriptor2 != null)
          //{
          //  services.Remove(descriptor2);
          //}

          //// Use TestContainers to create an in-memory RabbitMQ container
          //services.AddSingleton<IRabbitMqContainerFactory, RabbitMqContainerFactory>();

          //services.AddSingleton<ConnectionFactory>(provider =>
          //{
          //  var containerFactory = provider.GetRequiredService<IRabbitMqContainerFactory>();
          //  var container = containerFactory.CreateContainer();
          //  container.StartAsync().GetAwaiter().GetResult(); // Start the container
          //  return container.ConnectionFactory; // Assuming ConnectionFactory is a property of RabbitMQContainer
          //});

        });


  }

  public async Task InitializeAsync()
  {
    await _rabbitMqContainer.StartAsync();
  }

  async Task IAsyncLifetime.DisposeAsync()
  {
    await _rabbitMqContainer.DisposeAsync();
  }
}
