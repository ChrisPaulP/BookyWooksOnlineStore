using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookWooks.OrderApi.Core.OrderAggregate.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWooks.OrderApi.Infrastructure.Data.Config;
public class ProductConfiguration : IEntityTypeConfiguration<Product> 
{ 
  public void Configure(EntityTypeBuilder<Product> builder)
  {
    builder.HasKey(p => p.Id);

    builder.Property(p => p.Title).HasMaxLength(100).IsRequired();
  }
}

