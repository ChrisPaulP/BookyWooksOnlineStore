namespace BookWooks.OrderApi.TestContainersIntegrationTests;

[CollectionDefinition("Order Test Collection")]
public class OrderTestCollection : ICollectionFixture<CustomOrderTestFactory<Program>>
{
}