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