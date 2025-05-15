
namespace BookWooks.OrderApi.Infrastructure.Data.Configurations;
  public class ProductConfiguration : IEntityTypeConfiguration<Product>
  {
    public void Configure(EntityTypeBuilder<Product> builder)
    {
      // Configure the primary key
      builder.HasKey(p => p.ProductId);

      // Configure properties
      builder.Property(p => p.ProductId)
          .HasConversion(new ProductId.EfCoreValueConverter())
          .IsRequired();

      builder.Property(p => p.Name)
          .HasConversion(new ProductName.EfCoreValueConverter())
          .IsRequired()
          .HasMaxLength(100);

      builder.Property(p => p.Description)
          .HasConversion(new ProductDescription.EfCoreValueConverter())
          .IsRequired()
          .HasMaxLength(500);

      builder.Property(p => p.Price)
           .HasConversion(new ProductPrice.EfCoreValueConverter())
          .IsRequired()
          .HasColumnType("decimal(18,2)");

      builder.Property(p => p.Quantity)
          .HasConversion(new ProductQuantity.EfCoreValueConverter())
          .IsRequired();

      // Configure table name
      builder.ToTable("Products");
    }
  }
