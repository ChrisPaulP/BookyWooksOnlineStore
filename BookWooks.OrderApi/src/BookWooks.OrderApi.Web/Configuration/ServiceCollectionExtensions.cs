using Microsoft.Extensions.Options;

namespace BookWooks.OrderApi.Web.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShowAllServicesSupport(this IServiceCollection services)
    {
        services.Configure<ServiceConfig>(config =>
        {
            config.Services = new List<ServiceDescriptor>(services);
            config.Path = "/listservices";
        });

        return services;
    }
}

public class ServiceConfig
{
    public IList<ServiceDescriptor> Services { get; set; } = new List<ServiceDescriptor>();
    public string Path { get; set; } = "/listservices";
}
