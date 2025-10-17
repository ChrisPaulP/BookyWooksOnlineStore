using Microsoft.Extensions.DependencyInjection;

namespace BookWooks.OrderApi.Web.Configuration;

public static class EndpointConfiguration
{
    public static IServiceCollection RegisterEndpoints<T>(this IServiceCollection services) where T : IEndpoint
    {
        var assembly = typeof(T).Assembly;

        var endpointTypes = assembly.GetTypes()
            .Where(t => t.IsAssignableTo(typeof(IEndpoint)) && 
                       t is { IsClass: true, IsAbstract: false, IsInterface: false });

        var serviceDescriptors = endpointTypes
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type));

        services.TryAddEnumerable(serviceDescriptors);
        
        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpoints = app.Services.GetRequiredService<IEnumerable<IEndpoint>>();
        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }
        return app;
    }
}
