global using System.Data;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.DependencyInjection;

// Core
global using BookWooks.OrderApi.Core.OrderAggregate.Entities;
global using BookWooks.OrderApi.Core.OrderAggregate.Specifications;
global using BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;
global using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;

// Use Cases
global using BookWooks.OrderApi.UseCases.Cancel;
global using BookWooks.OrderApi.UseCases.Create;
global using BookWooks.OrderApi.UseCases.Orders;
global using BookWooks.OrderApi.UseCases.Errors;

// Shared Kernel
global using BookyWooks.SharedKernel.Commands;
global using BookyWooks.SharedKernel.Queries;
global using BookyWooks.SharedKernel.Repositories;
global using BookyWooks.SharedKernel.Validation;

// LanguageExt
global using LanguageExt;
global using static LanguageExt.Prelude;

// Type Aliases
//global using OrderItem = BookWooks.OrderApi.UseCases.Create.OrderItem;

// Functional Extensions
global using BookWooks.OrderApi.UseCases.FunctionalExtensions;

// AI Services
global using BookWooks.OrderApi.UseCases.Orders.AiServices;


