using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVatNetGross : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalAmount",
                table: "Invoices");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalGross",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalNet",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalVat",
                table: "Invoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "VatRate",
                table: "InvoicePositions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalGross",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TotalNet",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "TotalVat",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "VatRate",
                table: "InvoicePositions");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
