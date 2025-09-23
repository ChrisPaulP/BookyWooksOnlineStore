namespace BookWooks.OrderApi.TestContainersIntegrationTests.TestSetup;
public record ContainerConfiguration
{
    public string DataProtectionPath { get; init; } = default!;
    public string ProjectResourcesPath { get; init; } = default!;
    public string QdrantStoragePath { get; init; } = default!;
    public const string RabbitMqUsername = "guest";
    public const string RabbitMqPassword = "guest";
    public const string SqlPassword = "Your_password123";

    public static ContainerConfiguration CreateDefault()
    {
        var dataProtectionPath = Path.Combine(Path.GetTempPath(), "dataprotection-keys");
        var qdrantStoragePath = Path.Combine(Path.GetTempPath(), "qdrant-storage");
        var currentDirectory = Directory.GetCurrentDirectory();
        var solutionDirectory = Path.GetFullPath(Path.Combine(currentDirectory, "..", "..", "..", ".."));
        var projectResourcesPath = Path.Combine(solutionDirectory, "BookWooks.MCPServer", "ProjectResources");

        Directory.CreateDirectory(dataProtectionPath);
        Directory.CreateDirectory(qdrantStoragePath);
        EnsureProjectResourcesExist(projectResourcesPath);

        return new ContainerConfiguration
        {
            DataProtectionPath = dataProtectionPath,
            ProjectResourcesPath = projectResourcesPath,
            QdrantStoragePath = qdrantStoragePath
        };
    }

    private static void EnsureProjectResourcesExist(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            File.WriteAllText(
                Path.Combine(path, "customer-support.txt"),
                "Test content for customer support"
            );
        }
    }
}
