using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class TypoInvoicePositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePosition_Invoices_InvoiceId",
                table: "InvoicePosition");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePosition_Products_ProductId",
                table: "InvoicePosition");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InvoicePosition",
                table: "InvoicePosition");

            migrationBuilder.RenameTable(
                name: "InvoicePosition",
                newName: "InvoicePositions");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePosition_ProductId",
                table: "InvoicePositions",
                newName: "IX_InvoicePositions_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePosition_InvoiceId",
                table: "InvoicePositions",
                newName: "IX_InvoicePositions_InvoiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvoicePositions",
                table: "InvoicePositions",
                column: "InvoicePositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePositions_Invoices_InvoiceId",
                table: "InvoicePositions",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePositions_Products_ProductId",
                table: "InvoicePositions",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePositions_Invoices_InvoiceId",
                table: "InvoicePositions");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoicePositions_Products_ProductId",
                table: "InvoicePositions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_InvoicePositions",
                table: "InvoicePositions");

            migrationBuilder.RenameTable(
                name: "InvoicePositions",
                newName: "InvoicePosition");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePositions_ProductId",
                table: "InvoicePosition",
                newName: "IX_InvoicePosition_ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_InvoicePositions_InvoiceId",
                table: "InvoicePosition",
                newName: "IX_InvoicePosition_InvoiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_InvoicePosition",
                table: "InvoicePosition",
                column: "InvoicePositionId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePosition_Invoices_InvoiceId",
                table: "InvoicePosition",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "InvoiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoicePosition_Products_ProductId",
                table: "InvoicePosition",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "ProductId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
