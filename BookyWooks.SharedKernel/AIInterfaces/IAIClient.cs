namespace BookyWooks.SharedKernel.AIInterfaces;

public interface IAIClient
{
    Task<string> RunAsync(string genre);
}
