using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using BookWooks.OrderApi.Core.OrderAggregate.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
  public void Configure(EntityTypeBuilder<Order> builder)
  {
    builder.HasKey(o => o.Id);


    builder.OwnsOne(o => o.DeliveryAddress, a =>
                {
                  a.WithOwner();
                  a.Property(a => a.Street).HasMaxLength(180).IsRequired();
                  a.Property(a => a.City).HasMaxLength(100).IsRequired();
                  a.Property(a => a.Country).HasMaxLength(90).IsRequired();
                  a.Property(a => a.PostCode).HasMaxLength(18);


                });

    builder.HasOne<Customer>()
         .WithMany()
         .HasForeignKey(o => o.CustomerId)
         .IsRequired();

    //builder.HasOne<Customer>()
    //   .WithOne()
    //   .HasForeignKey<Order>(o => o.CustomerId)
    //   .IsRequired();

    builder.Property(o => o.Status)
          .HasConversion(o => o.Value,o => OrderStatus.FromValue(o));

    builder.HasMany(o => o.OrderItems)
        .WithOne()
        .HasForeignKey(oi => oi.OrderId);

    builder.Navigation(x => x.DeliveryAddress).IsRequired(); // Every order must have a delivery address

    builder.ComplexProperty(
               o => o.Payment, paymentBuilder =>
               {
                 paymentBuilder.Property(p => p.CardName)
                     .HasMaxLength(50);

                 paymentBuilder.Property(p => p.CardNumber)
                     .HasMaxLength(24)
                     .IsRequired();

                 paymentBuilder.Property(p => p.Expiration)
                     .HasMaxLength(10);

                 paymentBuilder.Property(p => p.CVV)
                     .HasMaxLength(3);

                 paymentBuilder.Property(p => p.PaymentMethod);
               });
  }
}
