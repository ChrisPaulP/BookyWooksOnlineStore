using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using ModelContextProtocol.Protocol;

#pragma warning disable SKEXP0001

namespace BookWooks.OrderApi.Infrastructure.AiServices;

public static class ReadResourceResultExtensions
{
  public static ChatMessageContentItemCollection ToChatMessageContentItemCollection(this ReadResourceResult readResourceResult)
  {
    if (readResourceResult.Contents.Count == 0)
      throw new InvalidOperationException("The resource does not contain any contents.");

    ChatMessageContentItemCollection result = [];

    foreach (var content in readResourceResult.Contents)
    {
      var metadata = new Dictionary<string, object?> { ["uri"] = content.Uri };

      switch (content)
      {
        case TextResourceContents text:
          result.Add(new TextContent
          {
            Text = text.Text,
            MimeType = text.MimeType,
            Metadata = metadata
          });
          break;

        case BlobResourceContents blob:
          var data = Convert.FromBase64String(blob.Blob);
          if (blob.MimeType?.StartsWith("image", StringComparison.InvariantCulture) == true)
          {
            result.Add(new ImageContent
            {
              Data = data,
              MimeType = blob.MimeType,
              Metadata = metadata
            });
          }
          else if (blob.MimeType?.StartsWith("audio", StringComparison.InvariantCulture) == true)
          {
            result.Add(new AudioContent
            {
              Data = data,
              MimeType = blob.MimeType,
              Metadata = metadata
            });
          }
          else
          {
            result.Add(new BinaryContent
            {
              Data = data,
              MimeType = blob.MimeType,
              Metadata = metadata
            });
          }
          break;
      }
    }
    return result;
  }
}
