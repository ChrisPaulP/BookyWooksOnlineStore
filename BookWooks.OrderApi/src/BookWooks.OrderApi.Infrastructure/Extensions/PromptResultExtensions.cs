﻿
namespace BookWooks.OrderApi.Infrastructure.Extensions;
internal static class PromptResultExtensions
{
  /// <summary>
  /// Converts a <see cref="GetPromptResult"/> to chat message contents.
  /// </summary>
  /// <param name="result">The prompt result to convert.</param>
  /// <returns>The corresponding <see cref="ChatHistory"/>.</returns>
  public static IList<ChatMessageContent> ToChatMessageContents(this GetPromptResult result)
  {
    return [.. result.Messages.Select(ToChatMessageContent)];
  }

  /// <summary>
  /// Converts a <see cref="PromptMessage"/> to a <see cref="ChatMessageContent"/>.
  /// </summary>
  /// <param name="message">The <see cref="PromptMessage"/> to convert.</param>
  /// <returns>The corresponding <see cref="ChatMessageContent"/>.</returns>
  public static ChatMessageContent ToChatMessageContent(this PromptMessage message)
  {
    return new ChatMessageContent(role: message.Role.ToAuthorRole(), items: [new TextContent(message.Content.Text)]);
  }
}
