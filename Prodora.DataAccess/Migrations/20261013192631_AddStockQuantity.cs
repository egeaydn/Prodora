using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prodora.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStockQuantity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StockQuantity",
                table: "ProdoraProducts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StockReserved",
                table: "OrderItem",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockQuantity",
                table: "ProdoraProducts");

            migrationBuilder.DropColumn(
                name: "StockReserved",
                table: "OrderItem");
        }
    }
}
