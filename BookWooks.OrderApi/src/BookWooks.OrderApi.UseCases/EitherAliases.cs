global using CancelOrderResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderNotFound, BookWooks.OrderApi.UseCases.Orders.OrderCancelledDTO>;
global using SetOrderStatusResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderNotFound, BookWooks.OrderApi.UseCases.Orders.OrderDTOs>;
global using OrdersResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderNotFound, System.Collections.Generic.IEnumerable<BookWooks.OrderApi.UseCases.Orders.OrderDTOs>>;
global using OrdersByStatusResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderNotFound, System.Collections.Generic.IEnumerable<BookWooks.OrderApi.UseCases.Orders.OrderWithItemsDTO>>;
global using OrderDetailsResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.OrderNotFound, BookWooks.OrderApi.UseCases.Orders.OrderDTOs>;
global using CreateOrderResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.ValidationErrors, BookWooks.OrderApi.Core.OrderAggregate.ValueObjects.OrderId>;
global using BookRecommendationResult = LanguageExt.Either<string, string>;

global using ProductSearchResult = LanguageExt.Either<BookWooks.OrderApi.UseCases.Errors.ProductNotFound, System.Collections.Generic.IEnumerable<BookWooks.OrderApi.UseCases.Products.ProductDto>>;
