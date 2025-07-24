
namespace BookWooks.OrderApi.TestContainersIntegrationTests;

public class CustomOrderTestFactory<TEntryPoint> : TestFactoryBase<TEntryPoint> where TEntryPoint : class
{
    //protected override void ConfigureProjectSpecificServices(IServiceCollection services)
    //{
    //    // Add project-specific services here
    //    services.AddSingleton<IProject3Service, Project3Service>();
    //}

    protected override void ConfigureMassTransit(IBusRegistrationConfigurator busRegistration)
    {
        busRegistration.AddConsumer<CompletePaymentCommandConsumer>();
        busRegistration.AddConsumer<OrderCreatedConsumer>();
    }
}
