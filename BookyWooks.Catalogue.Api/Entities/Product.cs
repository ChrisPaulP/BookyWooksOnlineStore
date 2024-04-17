namespace BookyWooks.Catalogue.Api.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public List<string> Category { get; set; } = new();
    public string Description { get; set; } = default!;
    public string ImageFile { get; set; } = default!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }

    public int RemoveStock(int quantityDesired)
    {
        if (Quantity == 0)
        {
            //throw new BookCatalogueDomainException($"Empty stock, description {Description} is sold out");
        }

        if (quantityDesired <= 0)
        {
            //throw new BookCatalogueDomainException($"Products desired should be greater than zero");
        }

        int removed = Math.Min(quantityDesired, this.Quantity);

        this.Quantity -= removed;

        return removed;
    }
}
