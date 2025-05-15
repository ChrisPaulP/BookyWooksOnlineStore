using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.HasKey(o => o.OrderId);


    builder.Property(o => o.OrderId)
        .HasConversion(new OrderId.EfCoreValueConverter())  // ✅ Explicitly tell EF how to store OrderId
        .IsRequired()
        .ValueGeneratedNever();  // ✅ Ensure EF Core does NOT auto-generate OrderId

    //builder.Property(o => o.OrderId)
    //        .HasConversion(
    //            id => id.Value, // Store as Guid
    //            value => OrderId.From(value)) // Retrieve as OrderId
    //        .ValueGeneratedNever(); // Prevent EF from generating OrderId


    // Map immutable properties using constructor binding
    builder.Property(o => o.Message).HasConversion(new Message.EfCoreValueConverter()).IsRequired();
    builder.Property(o => o.IsCancelled).HasConversion(new IsCancelled.EfCoreValueConverter());
    builder.Property(o => o.OrderPlaced).HasConversion(new OrderPlaced.EfCoreValueConverter());

    builder.OwnsOne(o => o.DeliveryAddress, da =>
    {
      da.Property(d => d.Street)
          .HasConversion(new Street.EfCoreValueConverter())
          .IsRequired()
          .HasMaxLength(100)
          .HasColumnName("Street");

      da.Property(d => d.City)
          .HasConversion(new City.EfCoreValueConverter())
          .IsRequired()
          .HasMaxLength(50)
          .HasColumnName("City");

      da.Property(d => d.Country)
          .HasConversion(new Country.EfCoreValueConverter())
          .IsRequired()
          .HasMaxLength(50)
          .HasColumnName("Country");

      da.Property(d => d.Postcode)
          .HasConversion(new PostCode.EfCoreValueConverter())
          .IsRequired()
          .HasMaxLength(10)
          .HasColumnName("Postcode");
    });

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

    builder.OwnsOne(o => o.Payment, p =>
    {
      p.Property(p => p.CardName)
          .HasConversion(new CardName.EfCoreValueConverter())
          .HasMaxLength(50);

      p.Property(p => p.CardNumber)
          .HasConversion(new CardNumber.EfCoreValueConverter())
          .HasMaxLength(24)
          .IsRequired();

      p.Property(p => p.Expiration)
          .HasConversion(new Expiration.EfCoreValueConverter())
          .HasMaxLength(10);

      p.Property(p => p.PaymentMethod)
          .HasConversion(new PaymentMethod.EfCoreValueConverter())
          .IsRequired();
    });
  }
}

