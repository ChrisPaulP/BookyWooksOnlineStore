﻿namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
  public void Configure(EntityTypeBuilder<OrderItem> builder)
  {
    builder.HasKey(oi => oi.OrderItemId);
    builder.Property(oi => oi.OrderItemId).ValueGeneratedNever().HasConversion(new OrderItemId.EfCoreValueConverter()).IsRequired();


    builder.Property(oi => oi.OrderId).HasConversion(new OrderId.EfCoreValueConverter()).IsRequired();


    builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(oi => oi.ProductId);

    builder.Property(oi => oi.Quantity).HasConversion(new ProductQuantity.EfCoreValueConverter()).IsRequired();
    builder.Property(oi => oi.ProductName).HasConversion(new ProductName.EfCoreValueConverter()).IsRequired();
    builder.Property(oi => oi.ProductDescription).HasConversion(new ProductDescription.EfCoreValueConverter()).IsRequired();

    builder.Property(oi => oi.Price).HasConversion(new ProductPrice.EfCoreValueConverter()).IsRequired().HasColumnType("decimal(18,2)");
  }
}
