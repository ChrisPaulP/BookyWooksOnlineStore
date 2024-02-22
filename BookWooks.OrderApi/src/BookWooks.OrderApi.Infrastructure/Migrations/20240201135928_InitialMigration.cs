using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookWooks.OrderApi.Infrastructure.Migrations;

  /// <inheritdoc />
  public partial class InitialMigration : Migration
  {
      /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.CreateTable(
              name: "Contributor",
              columns: table => new
              {
                  Id = table.Column<int>(type: "int", nullable: false)
                      .Annotation("SqlServer:Identity", "1, 1"),
                  Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                  Status = table.Column<int>(type: "int", nullable: false)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_Contributor", x => x.Id);
              });

          migrationBuilder.CreateTable(
              name: "Orders",
              columns: table => new
              {
                  Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                  Status = table.Column<int>(type: "int", nullable: false),
                  OrderTotal = table.Column<int>(type: "int", nullable: false),
                  OrderPlaced = table.Column<DateTime>(type: "datetime2", nullable: false),
                  OrderPaid = table.Column<bool>(type: "bit", nullable: false),
                  IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                  Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  DeliveryAddress_Street = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                  DeliveryAddress_City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                  DeliveryAddress_Country = table.Column<string>(type: "nvarchar(90)", maxLength: 90, nullable: false),
                  DeliveryAddress_PostCode = table.Column<string>(type: "nvarchar(18)", maxLength: 18, nullable: false),
                  PaymentMethodId = table.Column<int>(type: "int", nullable: true),
                  CustomerId = table.Column<int>(type: "int", nullable: true)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_Orders", x => x.Id);
              });

          migrationBuilder.CreateTable(
              name: "OrderItems",
              columns: table => new
              {
                  Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                  BookPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                  BookTitle = table.Column<string>(type: "nvarchar(90)", maxLength: 90, nullable: false),
                  Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  BookImageUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                  Quantity = table.Column<int>(type: "int", nullable: false),
                  BookId = table.Column<int>(type: "int", nullable: false),
                  OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
              },
              constraints: table =>
              {
                  table.PrimaryKey("PK_OrderItems", x => x.Id);
                  table.ForeignKey(
                      name: "FK_OrderItems_Orders_OrderId",
                      column: x => x.OrderId,
                      principalTable: "Orders",
                      principalColumn: "Id");
              });

          migrationBuilder.CreateIndex(
              name: "IX_OrderItems_OrderId",
              table: "OrderItems",
              column: "OrderId");
      }

      /// <inheritdoc />
      protected override void Down(MigrationBuilder migrationBuilder)
      {
          migrationBuilder.DropTable(
              name: "Contributor");

          migrationBuilder.DropTable(
              name: "OrderItems");

          migrationBuilder.DropTable(
              name: "Orders");
      }
  }
