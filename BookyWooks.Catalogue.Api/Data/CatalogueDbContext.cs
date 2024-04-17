using BookyWooks.Catalogue.Api.Entities;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using System;

namespace BookyWooks.Catalogue.Api.Data;

public class CatalogueDbContext : DbContext
{
    public CatalogueDbContext(DbContextOptions<CatalogueDbContext> options)
        : base(options)
    {
        
    }
    public DbSet<Product> Products { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();
    }
}
