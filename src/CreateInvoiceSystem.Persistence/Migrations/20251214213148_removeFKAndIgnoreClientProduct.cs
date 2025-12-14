using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class removeFKAndIgnoreClientProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePositions_Products_ProductId",
                table: "InvoicePositions");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Clients_ClientId",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "Invoices",
                newName: "ClientId1");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_ClientId",
                table: "Invoices",
                newName: "IX_Invoices_ClientId1");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "InvoicePositions",
                newName: "ProductId1");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePositions_ProductId",
                table: "InvoicePositions",
                newName: "IX_InvoicePositions_ProductId1");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePositions_Products_ProductId1",
                table: "InvoicePositions");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Clients_ClientId1",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "ClientId1",
                table: "Invoices",
                newName: "ClientId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_ClientId1",
                table: "Invoices",
                newName: "IX_Invoices_ClientId");

            migrationBuilder.RenameColumn(
                name: "ProductId1",
                table: "InvoicePositions",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePositions_ProductId1",
                table: "InvoicePositions",
                newName: "IX_InvoicePositions_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePositions_Products_ProductId",
                table: "InvoicePositions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Clients_ClientId",
                table: "Invoices",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "ClientId");
        }
    }
}
