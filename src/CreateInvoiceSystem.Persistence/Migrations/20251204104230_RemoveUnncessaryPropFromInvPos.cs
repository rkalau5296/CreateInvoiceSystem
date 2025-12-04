using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnncessaryPropFromInvPos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "InvoicePositions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "InvoicePositions");

            migrationBuilder.DropColumn(
                name: "Value",
                table: "InvoicePositions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "InvoicePositions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "InvoicePositions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "InvoicePositions",
                type: "decimal(18,2)",
                nullable: true);
        }
    }
}
