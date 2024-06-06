using BookyWooks.Catalogue.Api.Entities;

namespace BookyWooks.Catalogue.Api.Data;

public class InitialData
{
    public static IEnumerable<Product> Products =>
   new List<Product>
   {
         new Product()
             {
                 Id = new Guid("1e9c1a7e-1d9b-4c0e-8a15-5e12b5f5ad34"),
                 Name = "To Kill a Mockingbird",
                 Description = "A novel about the serious issues of rape and racial inequality, told through the eyes of a young girl.",
                 ImageFile = "to-kill-a-mockingbird.png",
                 Price = 10.99M,
                 Category = new List<string> { "Fiction" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("2d65ff2a-c57a-44c8-8e49-51af4e276f68"),
                 Name = "1984",
                 Description = "A dystopian social science fiction novel and cautionary tale about the dangers of totalitarianism.",
                 ImageFile = "1984.png",
                 Price = 8.99M,
                 Category = new List<string> { "Dystopian" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("3c4e6b45-738f-4a9a-85f5-68e26b3a58f9"),
                 Name = "Pride and Prejudice",
                 Description = "A romantic novel that charts the emotional development of the protagonist, Elizabeth Bennet.",
                 ImageFile = "pride-and-prejudice.png",
                 Price = 9.99M,
                 Category = new List<string> { "Romance" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("4a2ebc70-4e79-4d35-ae39-0c8adfa7e9b6"),
                 Name = "The Great Gatsby",
                 Description = "A story about the young and mysterious millionaire Jay Gatsby and his quixotic passion for the beautiful Daisy Buchanan.",
                 ImageFile = "the-great-gatsby.png",
                 Price = 10.99M,
                 Category = new List<string> { "Tragedy" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("5c34768f-fc3f-4c9a-8054-dfe19874b2c1"),
                 Name = "Harry Potter and the Philosopher's Stone",
                 Description = "The first novel in the Harry Potter series and J.K. Rowling's debut novel.",
                 ImageFile = "harry-potter.png",
                 Price = 12.99M,
                 Category = new List<string> { "Fantasy" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("6a2d3e3c-7b64-4785-a41c-0e4382a3a72c"),
                 Name = "The Hobbit",
                 Description = "A fantasy novel and children's Product by J.R.R. Tolkien.",
                 ImageFile = "the-hobbit.png",
                 Price = 11.99M,
                 Category = new List<string> { "Fantasy" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("7c348f0b-846a-4eb6-9a87-44cbe28f5672"),
                 Name = "Moby Dick",
                 Description = "A novel by Herman Melville in which Captain Ahab seeks vengeance on a giant white whale named Moby Dick.",
                 ImageFile = "moby-dick.png",
                 Price = 14.99M,
                 Category = new List<string> { "Adventure" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("8b567e9c-07e8-45c3-a0f1-6c52685a03b8"),
                 Name = "War and Peace",
                 Description = "A novel by Leo Tolstoy that chronicles the French invasion of Russia and its impact on Tsarist society.",
                 ImageFile = "war-and-peace.png",
                 Price = 15.99M,
                 Category = new List<string> { "Historical Fiction" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("9a5e87c3-5f49-4f0e-8b82-d84f8d93c85f"),
                 Name = "The Catcher in the Rye",
                 Description = "A novel by J.D. Salinger about the events and circumstances that occur around 16-year-old Holden Caulfield.",
                 ImageFile = "the-catcher-in-the-rye.png",
                 Price = 9.99M,
                 Category = new List<string> { "Fiction" },
                 Quantity = 10
             },
             new Product()
             {
                 Id = new Guid("0b85e87c-3a5f-4f9e-8c72-d74e7a03c85e"),
                 Name = "The Lord of the Rings",
                 Description = "An epic high-fantasy novel written by English author and scholar J.R.R. Tolkien.",
                 ImageFile = "the-lord-of-the-rings.png",
                 Price = 19.99M,
                 Category = new List<string> { "Fantasy" },
                 Quantity = 10
             }
   };
}
