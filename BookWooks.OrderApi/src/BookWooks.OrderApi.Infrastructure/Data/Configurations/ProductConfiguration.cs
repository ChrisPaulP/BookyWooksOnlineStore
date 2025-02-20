namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class ProductConfiguration : IEntityTypeConfiguration<Product> 
{ 
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.HasKey(p => p.Id);
    builder.Property(p => p.Id).ValueGeneratedNever();

    builder.Property(p => p.Title).HasMaxLength(100).IsRequired();
  }
}

