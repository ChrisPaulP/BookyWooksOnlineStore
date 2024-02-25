
using BookWooks.OrderApi.Core.OrderAggregate;
using Xunit;

namespace BookWooks.OrderApi.IntegrationTests.Data;
//public class EfRepositoryDelete : BaseEfRepoTestFixture
//{
//  [Fact]
//  public async Task DeletesItemAfterAddingIt()
//  {
//    // add a Contributor
//    var orderStatus = OrderStatus.Pending.Name;
//    var deliveryAddress = new DeliveryAddress("teststreetdelete", "testcitydelete", "testcountrydelete", "testpostcodedelete");
//    var repository = GetRepository();
//    var orderToDelete = new Order("testuseriddelete", "testusernamedelete", deliveryAddress, "testcardnumberdelete", "testsecuritynumberdelete", "testcardholdernamedelete");

//    await repository.AddAsync(orderToDelete);
//    await repository.UnitOfWork.SaveEntitiesAsync();

//    // delete the item
//     repository.Remove(orderToDelete);
//    await repository.UnitOfWork.SaveEntitiesAsync();

//    // verify it's no longer there
//    Assert.DoesNotContain(await repository.ListAllAsync(),
//        Order => orderToDelete.DeliveryAddress.Street == deliveryAddress.Street);
//  }
//}
