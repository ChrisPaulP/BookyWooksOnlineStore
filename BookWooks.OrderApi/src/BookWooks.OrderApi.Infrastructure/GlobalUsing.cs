// System namespaces
global using System.Data;
global using System.Reflection;
global using System.Text;
global using System.Linq.Expressions;
global using System.Net.Mail;
global using System.Text.Json;

// Microsoft namespaces
global using Microsoft.EntityFrameworkCore;
global using Microsoft.EntityFrameworkCore.Storage;
global using Microsoft.EntityFrameworkCore.Metadata.Builders;
global using Microsoft.Extensions.Caching.Distributed;
global using Microsoft.Extensions.Caching.StackExchangeRedis;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.EntityFrameworkCore.Design;

// Third-party libraries
global using MassTransit;
global using MediatR;
global using Newtonsoft.Json;
global using Polly;
global using Quartz;
global using Serilog.Core;
global using Serilog.Events;
global using LogContext = Serilog.Context.LogContext;
global using LanguageExt;
global using Microsoft.SemanticKernel;
global using Microsoft.SemanticKernel.ChatCompletion;
global using Microsoft.SemanticKernel.Connectors.OpenAI;

// Project: BookyWooks.SharedKernel
global using BookyWooks.SharedKernel;
global using BookyWooks.SharedKernel.Messages;
global using BookyWooks.SharedKernel.Serialization;
global using BookyWooks.SharedKernel.Commands;
global using BookyWooks.SharedKernel.InternalCommands;
global using BookyWooks.SharedKernel.DomainEventsDispatching;
global using BookyWooks.SharedKernel.DomainEventMapper;
global using BookyWooks.SharedKernel.Repositories;
global using BookyWooks.SharedKernel.UnitOfWork;
global using BookyWooks.SharedKernel.Specification;

// Project: BookyWooks.Messaging
global using BookyWooks.Messaging.MassTransit;
global using BookyWooks.Messaging.Contracts.Commands;
global using BookyWooks.Messaging.Contracts.Events;
global using BookyWooks.Messaging.Constants;
global using BookyWooks.Messaging.Contracts;

// Project: BookWooks.OrderApi.Core
global using BookWooks.OrderApi.Core.OrderAggregate.Entities;
global using BookWooks.OrderApi.Core.OrderAggregate.Enums;
global using BookWooks.OrderApi.Core.OrderAggregate.ValueObjects;
global using BookWooks.OrderApi.Core.OrderAggregate.DomainValidation;
global using BookWooks.OrderApi.Core.Interfaces;

// Project: BookWooks.OrderApi.UseCases
global using BookWooks.OrderApi.UseCases;
global using BookWooks.OrderApi.UseCases.Orders;
global using BookWooks.OrderApi.UseCases.Orders.List;
global using BookWooks.OrderApi.UseCases.Orders.AiServices;
global using BookWooks.OrderApi.UseCases.Products;
global using BookWooks.OrderApi.UseCases.InternalCommands;

// Project: BookWooks.OrderApi.Infrastructure
global using BookWooks.OrderApi.Infrastructure.Data.Repositories.Abstract;
global using BookWooks.OrderApi.Infrastructure.Data;
global using BookWooks.OrderApi.Infrastructure.Data.Queries;
global using BookWooks.OrderApi.Infrastructure.Data.Repositories;
global using BookWooks.OrderApi.Infrastructure.Email;
global using BookWooks.OrderApi.Infrastructure.MassTransit;
global using BookWooks.OrderApi.Infrastructure.Common;
global using BookWooks.OrderApi.Infrastructure.Common.Processing;
global using BookWooks.OrderApi.Infrastructure.Common.Processing.Outbox;
global using BookWooks.OrderApi.Infrastructure.Common.Processing.InternalCommands;
global using BookWooks.OrderApi.Infrastructure.AIClients;
global using BookWooks.OrderApi.Infrastructure.AiServices;
global using BookWooks.OrderApi.Infrastructure.Options;

// Other/External
global using ModelContextProtocol.Client;
global using ModelContextProtocol.Protocol;

// Aliases
global using JsonSerializer = System.Text.Json.JsonSerializer;
global using bookwooks.orderapi.infrastructure.caching;


