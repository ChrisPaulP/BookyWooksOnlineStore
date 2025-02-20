namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class InternalCommandConfiguration : IEntityTypeConfiguration<InternalCommand>
{
  public void Configure(EntityTypeBuilder<InternalCommand> builder)
  {
    builder.ToTable("InternalCommand", "order");

    builder.HasKey(ic => ic.Id);
    builder.Property(b => b.Id).ValueGeneratedNever();

    builder.Property(ic => ic.EnqueueDate)
    .IsRequired();

    builder.Property(ic => ic.Type)
        .IsRequired()
        .HasMaxLength(255);

    builder.Property(ic => ic.Data)
        .IsRequired();

    builder.Property(ic => ic.ProcessedDate);

    builder.Property(x => x.Error);
  }
}
