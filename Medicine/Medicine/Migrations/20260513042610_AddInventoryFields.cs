using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Medicine.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BatchNumber",
                table: "InventoryTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "InventoryTransactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ImportPrice",
                table: "InventoryTransactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "InventoryTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StorageLocation",
                table: "InventoryTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Supplier",
                table: "InventoryTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Unit",
                table: "InventoryTransactions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinStockLevel",
                table: "Drugs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BatchNumber",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "ImportPrice",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "StorageLocation",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Supplier",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "Unit",
                table: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "MinStockLevel",
                table: "Drugs");
        }
    }
}
