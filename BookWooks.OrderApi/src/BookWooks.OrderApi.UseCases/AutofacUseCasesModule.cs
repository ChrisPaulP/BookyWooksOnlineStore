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
  }
  private void RegisterMediatR(ContainerBuilder builder)
  {
    // Register the assembly containing all the Command classes (that implement IRequestHandler)
    // typeof(IRequestHandler<,>), // this takes care of registering commands
    builder
     .RegisterAssemblyTypes(typeof(CreateOrderCommand).Assembly)
     .AsClosedTypesOf(typeof(IRequestHandler<,>));
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

