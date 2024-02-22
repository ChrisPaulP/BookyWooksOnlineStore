using Autofac;
using BookWooks.OrderApi.Core.Interfaces;
using BookWooks.OrderApi.Core.OrderAggregate.Handlers;

using MediatR;

namespace BookWooks.OrderApi.Core;
/// <summary>
/// An Autofac module that is responsible for wiring up services defined in the Core project.
/// </summary>
public class AutofacCoreModule : Module
{
  protected override void Load(ContainerBuilder builder)
  {
    // Register the assembly containing all the Domain Event Handlers (that implement INotificationHandler)
    // typeof(INotificationHandler<>), // this takes care of registering domain events
    builder
     .RegisterAssemblyTypes(typeof(OrderCreatedDomainEventHandler).Assembly)
     .AsClosedTypesOf(typeof(INotificationHandler<>));

    //builder.RegisterType<DeleteContributorService>()
    //    .As<IDeleteContributorService>().InstancePerLifetimeScope();
  }
}
