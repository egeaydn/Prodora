using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prodora.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class PreserveOrdersAndCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ProdoraProducts_ProductId",
                table: "OrderItem");

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "ProdoraProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RequestId",
                table: "Orders",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductImage",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "OrderItem",
                type: "nvarchar(max)",
                nullable: true);

            // Preserve today's catalog values for legacy orders before future catalog edits.
            migrationBuilder.Sql(@"UPDATE oi SET ProductName = p.Name,
                ProductImage = (SELECT TOP (1) i.ImageUrl FROM ImagesTable i WHERE i.ProductId = p.Id ORDER BY i.Id)
                FROM OrderItem oi INNER JOIN ProdoraProducts p ON p.Id = oi.ProductId;");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RequestId",
                table: "Orders",
                column: "RequestId",
                unique: true,
                filter: "[RequestId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ProdoraProducts_ProductId",
                table: "OrderItem",
                column: "ProductId",
                principalTable: "ProdoraProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ProdoraProducts_ProductId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "ProdoraProducts");

            migrationBuilder.DropColumn(
                name: "RequestId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductImage",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "OrderItem");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ProdoraProducts_ProductId",
                table: "OrderItem",
                column: "ProductId",
                principalTable: "ProdoraProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
