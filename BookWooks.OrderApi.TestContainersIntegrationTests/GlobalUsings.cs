global using Xunit;
global using BookWooks.OrderApi.Infrastructure.Data;
global using BookWooks.OrderApi.UseCases.Create;
global using BookWooks.OrderApi.Core.OrderAggregate.Entities;
global using BookyWooks.Messaging.Contracts.Commands;
global using BookyWooks.Messaging.Messages.InitialMessage;
global using FluentAssertions;
global using MassTransit.Testing;
global using BookWooks.OrderApi.TestContainersIntegrationTests.Consumers;
global using MassTransit;
global using BookyWooks.Messaging.Contracts.Events;
global using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
global using CustomerResult = LanguageExt.Validation<BookWooks.OrderApi.Core.OrderAggregate.DomainValidation.CustomerValidationErrors, BookWooks.OrderApi.Core.OrderAggregate.Entities.Customer>;
global using ProductResult = LanguageExt.Validation<BookWooks.OrderApi.Core.OrderAggregate.DomainValidation.ProductValidationErrors, BookWooks.OrderApi.Core.OrderAggregate.Entities.Product>;
global using LanguageExt;
global using CreateOrderResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.ValidationErrors, BookWooks.OrderApi.Core.OrderAggregate.ValueObjects.OrderId>;
global using BookWooks.OrderApi.Infrastructure.AiServices;

global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Mvc.Testing;
global using Microsoft.AspNetCore.TestHost;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;


global using MediatR;
global using Microsoft.EntityFrameworkCore;
global using System.Linq.Expressions;

global using Docker.DotNet;
global using DotNet.Testcontainers.Builders;
global  using DotNet.Testcontainers.Containers;     
global using DotNet.Testcontainers.Networks;
global using Testcontainers.MsSql;
global using Testcontainers.Qdrant;
global using Testcontainers.RabbitMq;
global using Testcontainers.Redis;
global using BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;