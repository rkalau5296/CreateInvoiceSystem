using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ProductEntitySetNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePositions_Products_ProductId",
                table: "InvoicePositions");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePositions_Products_ProductId",
                table: "InvoicePositions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePositions_Products_ProductId",
                table: "InvoicePositions");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePositions_Products_ProductId",
                table: "InvoicePositions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId");
        }
    }
}
