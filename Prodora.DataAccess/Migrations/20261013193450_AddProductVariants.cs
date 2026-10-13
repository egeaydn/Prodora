using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prodora.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProductVariants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "OrderItem",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VariantName",
                table: "OrderItem",
                type: "nvarchar(60)",
                maxLength: 60,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VariantId",
                table: "BasketItem",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ProductVariants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductVariants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductVariants_ProdoraProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "ProdoraProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderItem_VariantId",
                table: "OrderItem",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_BasketItem_VariantId",
                table: "BasketItem",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductVariants_ProductId_Name",
                table: "ProductVariants",
                columns: new[] { "ProductId", "Name" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BasketItem_ProductVariants_VariantId",
                table: "BasketItem",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItem_ProductVariants_VariantId",
                table: "OrderItem",
                column: "VariantId",
                principalTable: "ProductVariants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BasketItem_ProductVariants_VariantId",
                table: "BasketItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItem_ProductVariants_VariantId",
                table: "OrderItem");

            migrationBuilder.DropTable(
                name: "ProductVariants");

            migrationBuilder.DropIndex(
                name: "IX_OrderItem_VariantId",
                table: "OrderItem");

            migrationBuilder.DropIndex(
                name: "IX_BasketItem_VariantId",
                table: "BasketItem");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "VariantName",
                table: "OrderItem");

            migrationBuilder.DropColumn(
                name: "VariantId",
                table: "BasketItem");
        }
    }
}
