global using CancelOrderResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderErrors, BookWooks.OrderApi.UseCases.Orders.OrderCancelledDTO>;
global using SetOrderStatusResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderErrors, BookWooks.OrderApi.UseCases.Orders.OrderDTOs>;
global using OrdersResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderErrors, System.Collections.Generic.IEnumerable<BookWooks.OrderApi.UseCases.Orders.OrderDTOs>>;
global using OrdersByStatusResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderErrors, System.Collections.Generic.IEnumerable<BookWooks.OrderApi.UseCases.Orders.OrderWithItemsDTO>>;
global using OrderDetailsResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderErrors, BookWooks.OrderApi.UseCases.Orders.OrderDTOs>;
global using CreateOrderResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.CreateOrderErrors, BookWooks.OrderApi.Core.OrderAggregate.ValueObjects.OrderId>;
global using BookRecommendationResult = LanguageExt.Either<string, string>;

global using ProductSearchResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.ProductNotFound, System.Collections.Generic.IEnumerable<BookWooks.OrderApi.UseCases.Products.ProductDto>>;
