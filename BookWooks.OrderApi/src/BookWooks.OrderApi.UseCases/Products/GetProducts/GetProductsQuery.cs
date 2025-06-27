using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.UseCases.Products.GetProducts;
public record GetProductsQuery(string Prompt) : IQuery<ProductSearchResult>;
