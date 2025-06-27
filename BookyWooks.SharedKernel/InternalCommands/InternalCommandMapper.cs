namespace BookyWooks.SharedKernel.InternalCommands;

public class InternalCommandMapper : IInternalCommandMapper
{
    private readonly BiDirectionalDictionary<string, Type> _internalCommandMap;

    public InternalCommandMapper(BiDirectionalDictionary<string, Type> internalCommandsMap)
    {
        _internalCommandMap = internalCommandsMap;
    }

    public string GetName(Type type)
    {
        return _internalCommandMap.TryGetBySecond(type, out var name) ? name : null;
    }

    public Type GetType(string name)
    {
        return _internalCommandMap.TryGetByFirst(name, out var type) ? type : null;
    }
}
