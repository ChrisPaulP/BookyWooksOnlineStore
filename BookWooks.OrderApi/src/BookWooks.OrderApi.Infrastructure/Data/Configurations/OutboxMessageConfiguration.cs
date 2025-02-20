namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
  public void Configure(EntityTypeBuilder<OutboxMessage> builder)
  {
    builder.ToTable("OutboxMessage", "order");

    builder.HasKey(im => im.Id);
    builder.Property(im => im.Id).ValueGeneratedNever();

    // Configure properties
    builder.Property(im => im.MessageType)
          .IsRequired()
          .HasMaxLength(255);

    builder.Property(im => im.Message)
        .IsRequired();

    builder.Property(im => im.OccurredOn)
        .IsRequired()
        .HasColumnName("OccurredOn");

    builder.Property(im => im.ProcessedDate)
        .HasColumnName("ProcessedDate");
  }
}
