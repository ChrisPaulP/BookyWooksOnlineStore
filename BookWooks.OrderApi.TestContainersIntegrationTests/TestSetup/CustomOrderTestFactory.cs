namespace BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;
public class CustomOrderTestFactory<TEntryPoint> : TestFactoryBase<TEntryPoint>
    where TEntryPoint : class
{
    protected override void ConfigureMassTransit(IBusRegistrationConfigurator busRegistration)
    {
        busRegistration.AddConsumer<CompletePaymentCommandConsumer>();
        busRegistration.AddConsumer<OrderCreatedConsumer>();
    }
}
