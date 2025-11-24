namespace BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;

[CollectionDefinition("Order Test Collection", DisableParallelization = true)]
public class OrderTestCollection : ICollectionFixture<OrderWebApplicationFactory<Program>>
{
}
