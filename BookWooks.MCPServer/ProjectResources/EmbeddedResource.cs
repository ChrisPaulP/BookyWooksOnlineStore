
using System.Reflection;


namespace BookWooks.MCPServer.ProjectResources;

public static class EmbeddedResource
{
    private static readonly string? s_namespace = typeof(EmbeddedResource).Namespace;
    public static string ReadAsString(string resourcePath)
    {
        Stream stream = ReadAsStream(resourcePath);

        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
    public static Stream ReadAsStream(string resourcePath)
    {
        // Get the current assembly. Note: this class is in the same assembly where the embedded resources are stored.
        Assembly assembly =
            typeof(EmbeddedResource).GetTypeInfo().Assembly ??
            throw new InvalidOperationException($"[{s_namespace}] {resourcePath} assembly not found");

        //string resourceName = $"BookWooks.MCPServer.ProjectResources.{resourcePath}";
        string resourceName = $"{s_namespace}.{resourcePath}";


        return
            assembly.GetManifestResourceStream(resourceName) ??
            throw new InvalidOperationException($"{resourceName} resource not found");
    }
}
