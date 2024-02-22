using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Module = Autofac.Module;
using BookWooks.OrderApi.Core.Interfaces;
using BookWooks.OrderApi.Core.OrderAggregate;
using BookWooks.OrderApi.UseCases.Create;
using BookWooks.OrderApi.UseCases.Orders.Get;
using BookWooks.OrderApi.UseCases.Orders.List;
using BookyWooks.SharedKernel;
using MediatR.Pipeline;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Autofac;

using EventBus.IntegrationEventInterfaceAbstraction;
using BookWooks.OrderApi.UseCases.Contributors.Create;
using BookWooks.OrderApi.UseCases.IntegrationEventHandlers;

namespace BookWooks.OrderApi.UseCases;
public class AutofacUseCasesModule : Module
{
  private readonly bool _isDevelopment = false;

  public AutofacUseCasesModule(bool isDevelopment)
  {
    _isDevelopment = isDevelopment;
  }

  protected override void Load(ContainerBuilder builder)
  {
    if (_isDevelopment)
    {
      RegisterDevelopmentOnlyDependencies(builder);
    }
    else
    {
      RegisterProductionOnlyDependencies(builder);
    }  
    RegisterMediatR(builder);
    RegisterIntegrationEventHandlers(builder);
  }
  private void RegisterMediatR(ContainerBuilder builder)
  {
    // Register the assembly containing all the Command classes (that implement IRequestHandler)
    // typeof(IRequestHandler<,>), // this takes care of registering commands
    builder
     .RegisterAssemblyTypes(typeof(CreateOrderCommand).Assembly)
     .AsClosedTypesOf(typeof(IRequestHandler<,>));
  }
  private void RegisterIntegrationEventHandlers(ContainerBuilder builder)
  {
    builder.RegisterAssemblyTypes(typeof(BookStockCheckedEventHandler)
           .GetTypeInfo().Assembly)
           .AsClosedTypesOf(typeof(IIntegrationEventHandler<>)); // Registers Integration Event Handlers
  }

  private void RegisterDevelopmentOnlyDependencies(ContainerBuilder builder)
  {
    // NOTE: Add any development only services here

  }

  private void RegisterProductionOnlyDependencies(ContainerBuilder builder)
  {
    // NOTE: Add any production only (real) services here

  }
}

