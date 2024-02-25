
using BookWooks.OrderApi.Core.OrderAggregate;
using BookWooks.OrderApi.Infrastructure.Data;
using BookWooks.OrderApi.Infrastructure.Data.Repositories;
using BookyWooks.SharedKernel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace BookWooks.OrderApi.IntegrationTests.Data;
//public abstract class BaseEfRepoTestFixture
//{
//  protected BookyWooksOrderDbContext _dbContext;

//  protected BaseEfRepoTestFixture()
//  {
//    var options = CreateNewContextOptions();
//    //var _fakeEventDispatcher = Substitute.For<IDomainEventDispatcher<Guid>>();
//    var _fakeEventDispatcher = Substitute.For<IDomainEventDispatcher<Guid>>();

//    _dbContext = new BookyWooksOrderDbContext(options, _fakeEventDispatcher);
//  }

//  protected static DbContextOptions<BookyWooksOrderDbContext> CreateNewContextOptions()
//  {
//    // Create a fresh service provider, and therefore a fresh
//    // InMemory database instance.
//    var serviceProvider = new ServiceCollection()
//        .AddEntityFrameworkInMemoryDatabase()
//        .BuildServiceProvider();

//    // Create a new options instance telling the context to use an
//    // InMemory database and the new service provider.
//    var builder = new DbContextOptionsBuilder<BookyWooksOrderDbContext>();
//    builder.UseInMemoryDatabase("bookywooks")
//           .UseInternalServiceProvider(serviceProvider);

//    return builder.Options;
//  }

//  protected Repository<Order> GetRepository()
//  {
//    return new Repository<Order>(_dbContext);
//  }
//}
