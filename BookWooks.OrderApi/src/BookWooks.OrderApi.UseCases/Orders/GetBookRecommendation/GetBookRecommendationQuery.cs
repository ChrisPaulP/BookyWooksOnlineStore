using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookWooks.OrderApi.UseCases.Orders.GetBookRecommendation;
public record GetBookRecommendationQuery(string Genre) : IQuery<BookRecommendationResult>;
