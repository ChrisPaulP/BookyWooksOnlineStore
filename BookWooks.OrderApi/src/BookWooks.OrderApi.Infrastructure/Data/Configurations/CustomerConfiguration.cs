namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
  public void Configure(EntityTypeBuilder<Customer> builder)
  {
    // Configure the primary key
    builder.HasKey(c => c.CustomerId);

    // Configure properties
    builder.Property(c => c.CustomerId)
        .HasConversion(new CustomerId.EfCoreValueConverter())
        .IsRequired();

    builder.Property(c => c.Name)
        .HasConversion(new CustomerName.EfCoreValueConverter())
        .IsRequired()
        .HasMaxLength(100);

    builder.Property(c => c.Email)
         .HasConversion(new EmailAddress.EfCoreValueConverter())
        .IsRequired()
        .HasMaxLength(255);

    // Configure unique index on Email
    builder.HasIndex(c => c.Email).IsUnique();

    // Configure table name
    builder.ToTable("Customers");
  }
}
