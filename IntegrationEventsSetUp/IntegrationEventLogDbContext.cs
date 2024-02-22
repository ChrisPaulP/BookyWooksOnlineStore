
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutBoxPattern;

public class IntegrationEventLogDbContext : DbContext
{
    public IntegrationEventLogDbContext(DbContextOptions<IntegrationEventLogDbContext> options) : base(options)
    {
    }
    public DbSet<IntegrationEventLog> IntegrationEventLogs { get; set; }
           protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<IntegrationEventLog>(ConfigureIntegrationEventLogEntry);
    }

    private void ConfigureIntegrationEventLogEntry(EntityTypeBuilder<IntegrationEventLog> builder)
        {
            builder.ToTable("IntegrationEventLog");

            builder.HasKey(e => e.EventId);

            builder.Property(e => e.EventId)
                .IsRequired();

            builder.Property(e => e.EventData)
                .IsRequired();

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.IntegrationEventLogStatus)
                .IsRequired();
        }
    }


