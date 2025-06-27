using BookWooks.MCPServer.Prompts;
using BookWooks.MCPServer.Resources.Templates;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
namespace BookWooks.MCPServer.Extensions;
#pragma warning disable SKEXP0001
public static class McpServerBuilderExtensions
{
    public static IMcpServerBuilder WithTools(this IMcpServerBuilder builder, Func<IServiceProvider, Kernel> kernelFactory)
    {
        builder.WithTools(kernelFactory(builder.Services.BuildServiceProvider()));
        return builder;
    }
    public static IMcpServerBuilder WithTools(this IMcpServerBuilder builder, Kernel? kernel = null)
    {
        if (kernel is not null)
        {
            foreach (var plugin in kernel.Plugins)
            {
                foreach (var function in plugin)
                {
                    builder.Services.AddSingleton(McpServerTool.Create(function.AsAIFunction(kernel)));
                }
            }
            return builder;
        }
        return builder;
    }

    public static IMcpServerBuilder WithPrompt(this IMcpServerBuilder builder, PromptDefinition promptDefinition)
    {
        builder.Services.AddSingleton(promptDefinition);

        builder.WithPromptHandlers();

        return builder;
    }
    /// Adds handlers for listing and reading prompts.
    public static IMcpServerBuilder WithPromptHandlers(this IMcpServerBuilder builder)
    {
        builder.WithListPromptsHandler(HandleListPromptRequestsAsync);
        builder.WithGetPromptHandler(HandleGetPromptRequestsAsync);

        return builder;
    }
    private static ValueTask<ListPromptsResult> HandleListPromptRequestsAsync(RequestContext<ListPromptsRequestParams> context, CancellationToken cancellationToken)
    {
        IEnumerable<PromptDefinition> promptDefinitions = context.Server.Services!.GetServices<PromptDefinition>();

        return ValueTask.FromResult(new ListPromptsResult
        {
            Prompts = [.. promptDefinitions.Select(d => d.Prompt)]
        });
    }

    private static async ValueTask<GetPromptResult> HandleGetPromptRequestsAsync(RequestContext<GetPromptRequestParams> context, CancellationToken cancellationToken)
    {
        if (context.Params?.Name is not string { } promptName || string.IsNullOrEmpty(promptName))
        {
            throw new ArgumentException("Prompt name is required.");
        }

        IEnumerable<PromptDefinition> promptDefinitions = context.Server.Services!.GetServices<PromptDefinition>();

        PromptDefinition? definition = promptDefinitions.FirstOrDefault(d => d.Prompt.Name == promptName);
        if (definition is null)
        {
            throw new ArgumentException($"No handler found for the prompt '{promptName}'.");
        }
        return await definition.Handler(context, cancellationToken);
    }

    public static IMcpServerBuilder WithResourceTemplate(this IMcpServerBuilder builder, ResourceTemplateDefinition templateDefinition)
    {
        // Register the resource template definition in the DI container
        builder.Services.AddSingleton(templateDefinition);

        builder.WithResourceTemplateHandlers();

        return builder;
    }
    /// Adds handlers for listing and reading resource templates.
    public static IMcpServerBuilder WithResourceTemplateHandlers(this IMcpServerBuilder builder)
    {
        builder.WithListResourceTemplatesHandler(HandleListResourceTemplatesRequestAsync);
        builder.WithReadResourceHandler(HandleReadResourceRequestAsync);

        return builder;
    }
    private static ValueTask<ListResourceTemplatesResult> HandleListResourceTemplatesRequestAsync(RequestContext<ListResourceTemplatesRequestParams> context, CancellationToken cancellationToken)
    {
        // Get and return all resource template definitions registered in the DI container
        IEnumerable<ResourceTemplateDefinition> definitions = context.Server.Services!.GetServices<ResourceTemplateDefinition>();

        return ValueTask.FromResult(new ListResourceTemplatesResult
        {
            ResourceTemplates = [.. definitions.Select(d => d.ResourceTemplate)]
        });
    }

    private static ValueTask<ReadResourceResult> HandleReadResourceRequestAsync(RequestContext<ReadResourceRequestParams> context, CancellationToken cancellationToken)
    {
        // Make sure the uri of the resource or resource template is provided
        if (context.Params?.Uri is not string { } resourceUri || string.IsNullOrEmpty(resourceUri))
        {
            throw new ArgumentException("Resource uri is required.");
        }

        // Look up in registered resource templates
        IEnumerable<ResourceTemplateDefinition> resourceTemplateDefinitions = context.Server.Services!.GetServices<ResourceTemplateDefinition>();

        foreach (var resourceTemplateDefinition in resourceTemplateDefinitions)
        {
            if (resourceTemplateDefinition.IsMatch(resourceUri))
            {
                return resourceTemplateDefinition.InvokeHandlerAsync(context, cancellationToken);
            }
        }
        throw new ArgumentException($"No handler found for the resource uri '{resourceUri}'.");
    }
}
