global using System.Data;
global using BookWooks.OrderApi.UseCases.Cancel;
global using Microsoft.Extensions.Logging;
global using BookWooks.OrderApi.Core.OrderAggregate.Entities;
global using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
global using BookyWooks.SharedKernel.Validation;
global using BookWooks.OrderApi.UseCases.Create;
global using BookyWooks.SharedKernel.Commands;
global using BookyWooks.SharedKernel.Repositories;
global using LanguageExt;
global using static LanguageExt.Prelude;
global using BookyWooks.SharedKernel.Queries;
global using BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;
global using BookWooks.OrderApi.UseCases.Errors;
global using OrderItem = BookWooks.OrderApi.UseCases.Create.OrderItem;



