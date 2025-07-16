namespace BookWooks.OrderApi.UseCases;
public static class UseCasesDependencyInjection
{
  public static IServiceCollection AddUseCasesServices(this IServiceCollection services)
  {
    //var assembly = typeof(CreateOrderCommand).Assembly;
    //services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

    return services;
  }
}

// The AddMediatR method registers MediatR-related services in the dependency injection container, allowing the usage of MediatR patterns and components throughout the application.
//  Examples:
//•	services.AddMediatR<CreateUserCommand>(): Registers the CreateUserCommand and its corresponding handler in the dependency injection container, enabling the execution of the command.
//•	services.AddMediatR(typeof(GetUserQuery)): Registers the GetUserQuery and its corresponding handler in the dependency injection container, allowing the execution of the query.
//•	services.AddMediatR(typeof(UserCreatedEvent), typeof(UserUpdatedEvent)): Registers the UserCreatedEvent and UserUpdatedEvent along with their corresponding event handlers in the dependency injection container, enabling event publishing and handling.
