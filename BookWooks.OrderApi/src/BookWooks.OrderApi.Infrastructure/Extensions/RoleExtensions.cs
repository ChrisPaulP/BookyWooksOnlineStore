﻿
using Microsoft.SemanticKernel.ChatCompletion;
using ModelContextProtocol.Protocol;

namespace BookWooks.OrderApi.Infrastructure.Extensions;
internal static class RoleExtensions
{
  /// <summary>
  /// Converts a <see cref="Role"/> to a <see cref="AuthorRole"/>.
  /// </summary>
  /// <param name="role">The MCP role to convert.</param>
  /// <returns>The corresponding <see cref="AuthorRole"/>.</returns>
  public static AuthorRole ToAuthorRole(this Role role)
  {
    return role switch
    {
      Role.User => AuthorRole.User,
      Role.Assistant => AuthorRole.Assistant,
      _ => throw new InvalidOperationException($"Unexpected role '{role}'")
    };
  }
}
