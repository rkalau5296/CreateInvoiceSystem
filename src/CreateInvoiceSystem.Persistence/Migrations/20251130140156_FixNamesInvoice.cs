using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class FixNamesInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ClientStreet",
                table: "Invoices",
                newName: "Street");

            migrationBuilder.RenameColumn(
                name: "ClientPostalCode",
                table: "Invoices",
                newName: "PostalCode");

            migrationBuilder.RenameColumn(
                name: "ClientNumber",
                table: "Invoices",
                newName: "Number");

            migrationBuilder.RenameColumn(
                name: "ClientNip",
                table: "Invoices",
                newName: "Nip");

            migrationBuilder.RenameColumn(
                name: "ClientName",
                table: "Invoices",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "ClientCountry",
                table: "Invoices",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "ClientCity",
                table: "Invoices",
                newName: "City");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Street",
                table: "Invoices",
                newName: "ClientStreet");

            migrationBuilder.RenameColumn(
                name: "PostalCode",
                table: "Invoices",
                newName: "ClientPostalCode");

            migrationBuilder.RenameColumn(
                name: "Number",
                table: "Invoices",
                newName: "ClientNumber");

            migrationBuilder.RenameColumn(
                name: "Nip",
                table: "Invoices",
                newName: "ClientNip");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Invoices",
                newName: "ClientName");

            migrationBuilder.RenameColumn(
                name: "Country",
                table: "Invoices",
                newName: "ClientCountry");

            migrationBuilder.RenameColumn(
                name: "City",
                table: "Invoices",
                newName: "ClientCity");
        }
    }
}
