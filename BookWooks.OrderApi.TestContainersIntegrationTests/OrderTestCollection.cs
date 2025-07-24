namespace BookWooks.OrderApi.TestContainersIntegrationTests;

[CollectionDefinition("Order Test Collection", DisableParallelization = true)]
public class OrderTestCollection : ICollectionFixture<CustomOrderTestFactory<Program>>
{
}