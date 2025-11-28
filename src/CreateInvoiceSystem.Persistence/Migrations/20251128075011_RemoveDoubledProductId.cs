using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDoubledProductId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePosition_Products_ProductId1",
                table: "InvoicePosition");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePosition_ProductId1",
                table: "InvoicePosition");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "InvoicePosition");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "InvoicePosition",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePosition_ProductId1",
                table: "InvoicePosition",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePosition_Products_ProductId1",
                table: "InvoicePosition",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "ProductId");
        }
    }
}
