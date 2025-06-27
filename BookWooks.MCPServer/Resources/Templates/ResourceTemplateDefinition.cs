using System.Text.RegularExpressions;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace BookWooks.MCPServer.Resources.Templates;

public sealed class ResourceTemplateDefinition
{

    private Regex? _regex = null;


    private KernelFunction? _kernelFunction = null;

    public required ResourceTemplate ResourceTemplate { get; init; }

    public required Delegate Handler { get; init; }

    public Kernel? Kernel { get; set; }

    public bool IsMatch(string uri)
    {
        return GetRegex().IsMatch(uri);
    }
    public async ValueTask<ReadResourceResult> InvokeHandlerAsync(RequestContext<ReadResourceRequestParams> context, CancellationToken cancellationToken)
    {
        _kernelFunction ??= KernelFunctionFactory.CreateFromMethod(Handler);

        Kernel
            ??= context.Server.Services?.GetRequiredService<Kernel>()
            ?? throw new InvalidOperationException("Kernel is not available.");

        KernelArguments args = new(source: GetArguments(context.Params!.Uri!))
        {
            { "context", context },
        };

        FunctionResult result = await _kernelFunction.InvokeAsync(kernel: Kernel, arguments: args, cancellationToken: cancellationToken);

        return result.GetValue<ReadResourceResult>() ?? throw new InvalidOperationException("The handler did not return a valid result.");
    }

    private Regex GetRegex()
    {
        if (_regex != null)
        {
            return _regex;
        }

        var pattern = "^" +
                      Regex.Escape(ResourceTemplate.UriTemplate)
                           .Replace("\\{", "(?<")
                           .Replace("}", ">[^/]+)") +
                      "$";

        return _regex = new(pattern, RegexOptions.Compiled);
    }

    private Dictionary<string, object?> GetArguments(string uri)
    {
        var match = GetRegex().Match(uri);
        if (!match.Success)
        {
            throw new ArgumentException($"The uri '{uri}' does not match the template '{ResourceTemplate.UriTemplate}'.");
        }

        return match.Groups.Cast<Group>().Where(g => g.Name != "0").ToDictionary(g => g.Name, g => (object?)g.Value);
    }
}
