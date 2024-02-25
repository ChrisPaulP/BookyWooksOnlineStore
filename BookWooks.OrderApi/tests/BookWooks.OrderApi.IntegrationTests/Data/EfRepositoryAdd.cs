
using BookWooks.OrderApi.Core.OrderAggregate;
using Xunit;

namespace BookWooks.OrderApi.IntegrationTests.Data;
//public class EfRepositoryAdd : BaseEfRepoTestFixture
//{
//  [Fact]
//  public async Task AddsOrderAndSetsId()
//  {
//    //var orderStatus = OrderStatus.Pending.Name;
//    var orderStatus = OrderStatus.Cancelled.Name;
//    var deliveryAddress = new DeliveryAddress("teststreet", "testcity", "testcountry", "testpostcode");
//    var repository = GetRepository();
//    var newOrder = new Order("testuserid", "testusername", deliveryAddress, "testcardnumber", "testsecuritynumber", "testcardholdername" );

//    await repository.AddAsync(newOrder);
//    await repository.UnitOfWork.SaveEntitiesAsync();

//    // Assert
//    var retrievedOrder = (await repository.ListAllAsync()).FirstOrDefault();

//    Assert.Equal(orderStatus, retrievedOrder?.Status.Name);
//    Assert.NotEqual(Guid.Empty, retrievedOrder?.Id);
//  }
//}
