using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.OwnsOne(o => o.DeliveryAddress, a =>
                {
                  a.WithOwner();
                  a.Property(a => a.Street).HasMaxLength(180).IsRequired();
                  a.Property(a => a.City).HasMaxLength(100).IsRequired();
                  a.Property(a => a.Country).HasMaxLength(90).IsRequired();
                  a.Property(a => a.PostCode).HasMaxLength(18);


                });

    builder.Property(o => o.Status)
      .HasConversion(
          o => o.Value,
          o => OrderStatus.FromValue(o));

    builder.Navigation(x => x.DeliveryAddress).IsRequired(); // Every order must have a delivery address
  }
}
