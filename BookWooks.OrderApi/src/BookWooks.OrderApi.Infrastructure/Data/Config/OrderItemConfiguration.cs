using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
  public void Configure(EntityTypeBuilder<OrderItem> builder)
  {
    builder.Property(oi => oi.BookId).IsRequired(true);

    builder.Property(oi => oi.BookPrice).HasColumnType("decimal(18,2)")
           .IsRequired(true);    

    builder.Property(oi => oi.BookTitle).HasMaxLength(90)
           .IsRequired();

    builder.Property(oi => oi.BookPrice)
           .IsRequired(true)
           .HasColumnType("decimal(18,2)");
  }
}
