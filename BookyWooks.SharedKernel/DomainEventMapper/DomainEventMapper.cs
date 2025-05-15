using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookyWooks.SharedKernel.DomainEventMapper;

public class DomainEventMapper : IDomainEventMapper
{
    private readonly BiDirectionalDictionary<string, Type> _domainEventsMap;

    public DomainEventMapper(BiDirectionalDictionary<string, Type> domainNotificationsMap)
    {
        _domainEventsMap = domainNotificationsMap;
    }

    public string GetName(Type type)
    {
        return _domainEventsMap.TryGetBySecond(type, out var name) ? name : null;
    }

    public Type GetType(string name)
    {
        return _domainEventsMap.TryGetByFirst(name, out var type) ? type : null;
    }
}

//How to register: 
//    public void ConfigureServices(IServiceCollection services)
//{
//    // Assuming _domainNotificationsMap is already defined and initialized
//    var domainNotificationsMap = new BiDictionary<string, Type>();

//    // Register DomainNotificationsMapper with the specific constructor parameter
//    services.AddSingleton<IDomainNotificationsMapper>(provider =>
//    {
//        return new DomainNotificationsMapper(domainNotificationsMap);
//    });

//    // Other service registrations
//    services.AddMediatR(typeof(Startup)); // Example for MediatR
//    services.AddScoped<IOutbox, OutboxImplementation>(); // Example for IOutbox
//    // Add other services as needed
//}