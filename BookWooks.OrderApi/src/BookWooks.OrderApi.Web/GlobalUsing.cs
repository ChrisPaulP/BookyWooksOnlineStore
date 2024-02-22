global using Ardalis.ListStartupServices;
global using Autofac;
global using Autofac.Extensions.DependencyInjection;
global using BookWooks.OrderApi.Core;
global using BookWooks.OrderApi.Infrastructure;
global using BookWooks.OrderApi.Infrastructure.Data;
global using BookWooks.OrderApi.Web;
global using FastEndpoints;
global using FastEndpoints.Swagger;
global using Microsoft.EntityFrameworkCore;
global using RabbitMQ;
global using Serilog;
global using BookWooks.OrderApi.UseCases;
global using OutBoxPattern;
global using BookWooks.OrderApi.UseCases.Orders;


global using BookWooks.OrderApi.UseCases.Create;
global using BookyWooks.SharedKernel;

global using MediatR;
global using Order = BookWooks.OrderApi.Core.OrderAggregate.Order;
global using BookWooks.OrderApi.UseCases.Orders.List;
global using BookWooks.OrderApi.UseCases.Orders.Get;
global using BookWooks.OrderApi.Web.Orders;
global using BookWooks.OrderApi.Core.OrderAggregate;
global using BookWooks.OrderApi.UseCases.Cancel;

