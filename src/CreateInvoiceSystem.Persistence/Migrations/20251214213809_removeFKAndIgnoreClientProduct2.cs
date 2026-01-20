using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class removeFKAndIgnoreClientProduct2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePositions_Products_ProductId1",
                table: "InvoicePositions");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Clients_ClientId1",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ClientId1",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_InvoicePositions_ProductId1",
                table: "InvoicePositions");

            migrationBuilder.DropColumn(
                name: "ClientId1",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "InvoicePositions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId1",
                table: "Invoices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "InvoicePositions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ClientId1",
                table: "Invoices",
                column: "ClientId1");

            migrationBuilder.CreateIndex(
                name: "IX_InvoicePositions_ProductId1",
                table: "InvoicePositions",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePositions_Products_ProductId1",
                table: "InvoicePositions",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Clients_ClientId1",
                table: "Invoices",
                column: "ClientId1",
                principalTable: "Clients",
                principalColumn: "ClientId");
        }
    }
}
