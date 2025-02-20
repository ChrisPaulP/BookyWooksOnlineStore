public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.HasKey(o => o.Id);
    builder.Property(o => o.Id).ValueGeneratedNever();

    // Map immutable properties using constructor binding
    builder.Property(o => o.Message).HasConversion(new Message.EfCoreValueConverter()).IsRequired();
    builder.Property(o => o.IsCancelled).HasConversion(new IsCancelled.EfCoreValueConverter());
    builder.Property(o => o.OrderPlaced).HasConversion(new OrderPlaced.EfCoreValueConverter());

    builder.Property(o => o.DeliveryAddress.Street)
        .HasConversion(new Street.EfCoreValueConverter())
        .IsRequired()
        .HasMaxLength(100)
        .HasColumnName("Street");

    builder.Property(o => o.DeliveryAddress.City)
        .HasConversion(new City.EfCoreValueConverter())
        .IsRequired()
        .HasMaxLength(50)
        .HasColumnName("City");

    builder.Property(o => o.DeliveryAddress.Country)
        .HasConversion(new Country.EfCoreValueConverter())
        .IsRequired()
        .HasMaxLength(50)
        .HasColumnName("Country");

    builder.Property(o => o.DeliveryAddress.Postcode)
        .HasConversion(new PostCode.EfCoreValueConverter())
        .IsRequired()
        .HasMaxLength(10)
        .HasColumnName("Postcode");

    builder.HasOne<Customer>()
        .WithMany()
        .HasForeignKey(o => o.CustomerId)
        .IsRequired();

    builder.Property(o => o.Status)
        .HasConversion(
            s => s.Label,
            s => OrderStatus.FromLabel(s))
        .IsRequired();

    builder.HasMany(o => o.OrderItems)
        .WithOne()
        .HasForeignKey(oi => oi.OrderId);

    builder.Property(o => o.Payment.CardName)
        .HasConversion(new CardName.EfCoreValueConverter())
        .HasMaxLength(50);

    builder.Property(o => o.Payment.CardNumber)
        .HasConversion(new CardNumber.EfCoreValueConverter())
        .HasMaxLength(24)
        .IsRequired();

    builder.Property(o => o.Payment.Expiration)
        .HasConversion(new Expiration.EfCoreValueConverter())
        .HasMaxLength(10);

    builder.Property(o => o.Payment.PaymentMethod)
        .HasConversion(new PaymentMethod.EfCoreValueConverter())
        .IsRequired();
  }
}
