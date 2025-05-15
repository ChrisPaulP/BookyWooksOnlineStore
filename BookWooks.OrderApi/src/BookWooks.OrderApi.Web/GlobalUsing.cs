// GlobalUsings.cs

// System
global using System.Data;
global using System.Reflection;

// Microsoft
global using Microsoft.OpenApi.Models;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;

// Third-Party Libraries
global using Ardalis.ListStartupServices;
global using Serilog;
global using MediatR;
global using FluentValidation;

// Core
global using BookWooks.OrderApi.Core;
global using BookWooks.OrderApi.Core.OrderAggregate.Events;

// Infrastructure
global using BookWooks.OrderApi.Infrastructure;
global using BookWooks.OrderApi.Infrastructure.Data.Extensions;
global using BookWooks.OrderApi.Infrastructure.Common.Processing;

// Use Cases
global using BookWooks.OrderApi.UseCases;
global using BookWooks.OrderApi.UseCases.Orders;
global using BookWooks.OrderApi.UseCases.Create;
global using BookWooks.OrderApi.UseCases.Cancel;
global using BookWooks.OrderApi.UseCases.InternalCommands;
global using BookWooks.OrderApi.UseCases.Errors;
global using BookWooks.OrderApi.UseCases.Orders.List;
global using BookWooks.OrderApi.UseCases.Orders.Get;
global using BookWooks.OrderApi.UseCases.Orders.GetOrders;
global using BookWooks.OrderApi.UseCases.Orders.ListOrdersForCustomer;
global using BookWooks.OrderApi.UseCases.Orders.OrderFulfillment;

// Web
global using BookWooks.OrderApi.Web.Orders;
global using BookWooks.OrderApi.Web.Shared;
global using IEndpoint = BookWooks.OrderApi.Web.Interfaces.IEndpoint;
global using IResult = Microsoft.AspNetCore.Http.IResult;
global using BookWooks.OrderApi.Web.Interfaces;

// Shared Kernel
global using BookyWooks.SharedKernel;
global using BookyWooks.SharedKernel.ExceptionHandling;

// Logging and Tracing
global using Logging;
global using Tracing;
