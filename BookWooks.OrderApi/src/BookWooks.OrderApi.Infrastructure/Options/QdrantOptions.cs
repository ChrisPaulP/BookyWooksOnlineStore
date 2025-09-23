using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.Infrastructure.Options;
public class QdrantOptions
{
  public const string Key = "QdrantOptions";
  public string QdrantHost { get; set; } = string.Empty;
  public int QdrantPort { get; set; }
  public string ApiKey { get; set; } = string.Empty; //"your-secret-api-key-here";
}

