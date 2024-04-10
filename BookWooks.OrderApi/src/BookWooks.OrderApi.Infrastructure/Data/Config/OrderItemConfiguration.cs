namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
  public void Configure(EntityTypeBuilder<OrderItem> builder)
  {
    builder.HasKey(oi => oi.Id);

    builder.Property(oi => oi.OrderId).IsRequired(true);


    builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(oi => oi.ProductId);

    builder.Property(oi => oi.Quantity).IsRequired();

    builder.Property(oi => oi.Price)
           .IsRequired(true)
           .HasColumnType("decimal(18,2)");
  }
}
