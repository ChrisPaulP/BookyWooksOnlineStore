namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
      builder.ToTable("InboxMessage", "order");

      // Configure the primary key
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
