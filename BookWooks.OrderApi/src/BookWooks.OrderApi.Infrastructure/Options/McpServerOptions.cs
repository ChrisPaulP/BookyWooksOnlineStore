using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.Infrastructure.Options;
public class McpServerOptions
{
  public const string Key = "McpServerOptions";
  public string Host { get; init; } = "bookwooks.mcpserver";
  public int Port { get; init; } = 8181;
}
