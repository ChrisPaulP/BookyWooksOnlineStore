using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWooks.OrderApi.Infrastructure.Migrations.IntegrationEventLogDb;

  /// <inheritdoc />
  public partial class InitialMigration : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.CreateTable(
              name: "IntegrationEventLog",
              columns: table => new
              {
                  EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                  EventData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                  IntegrationEventLogStatus = table.Column<int>(type: "int", nullable: false),
                  TransactionId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  EventTypeName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                  TimesSent = table.Column<int>(type: "int", nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_IntegrationEventLog", x => x.EventId);
              });
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropTable(
              name: "IntegrationEventLog");
      }
  }
