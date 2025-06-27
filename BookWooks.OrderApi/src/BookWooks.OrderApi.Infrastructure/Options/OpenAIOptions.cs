using System.ComponentModel.DataAnnotations;

namespace BookWooks.OrderApi.Infrastructure.Options;
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
