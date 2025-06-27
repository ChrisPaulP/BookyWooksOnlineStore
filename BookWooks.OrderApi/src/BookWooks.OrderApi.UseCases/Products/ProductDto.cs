﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.UseCases.Products;
public class ProductDto
{
  [JsonPropertyName("id")]
  public Guid Id { get; set; }
  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;
  [JsonPropertyName("description")]
  public string Description { get; set; } = string.Empty;
  [JsonPropertyName("price")]
  public decimal Price { get; set; }
}
