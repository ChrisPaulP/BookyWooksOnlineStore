using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.MCPServer.Settings;

public class OpenAIOptions
{
    public const string Key = "OpenAIOptions";
    public const string QdrantUrl = "http://localhost:6333";

    [Required(ErrorMessage = "OpenAI:ApiKey is required.")]
    public string OpenAiApiKey { get; init; } = string.Empty;

    [Required(ErrorMessage = "OpenAI:ChatModelId is required.")]
    public string ChatModelId { get; init; } = string.Empty;

    [Required(ErrorMessage = "OpenAI:EmbeddingModelId is required.")]
    public string EmbeddingModelId { get; init; } = string.Empty;
}
