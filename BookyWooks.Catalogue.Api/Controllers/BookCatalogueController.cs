using BookyWooks.Catalogue.Api.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookyWooks.Catalogue.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookCatalogueController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<BookCatalogueController> _logger;

        public BookCatalogueController(ILogger<BookCatalogueController> logger)
        {
            _logger = logger;
        }

        [Authorize]
        [HttpGet("products")]
        public IEnumerable<Product> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new Product
            {
                Id = new Guid("1e9c1a7e-1d9b-4c0e-8a15-5e12b5f5ad34"),
                Name = "To Kill a Mockingbird",
                Description = "A novel about the serious issues of rape and racial inequality, told through the eyes of a young girl.",
                ImageFile = "to-kill-a-mockingbird.png",
                Price = 10.99M,
                Category = new List<string> { "Fiction" },
                Quantity = 10
            }).ToArray();
        }

        [HttpGet("products/test")]
        public IEnumerable<Product> GetProductsTest()
        {
            return Enumerable.Range(1, 5).Select(index => new Product
            {
                Id = new Guid("1e9c1a7e-1d9b-4c0e-8a15-5e12b5f5ad34"),
                Name = "To Kill a Mockingbird",
                Description = "A novel about the serious issues of rape and racial inequality, told through the eyes of a young girl.",
                ImageFile = "to-kill-a-mockingbird.png",
                Price = 10.99M,
                Category = new List<string> { "Fiction" },
                Quantity = 10
            }).ToArray();
        }
    }
}
