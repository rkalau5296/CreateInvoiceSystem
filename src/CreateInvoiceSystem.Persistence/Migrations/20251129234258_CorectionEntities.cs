using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorectionEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UnitPrice",
                table: "InvoicePosition");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "InvoicePosition",
                newName: "ProductName");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "InvoicePosition",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "Value",
                table: "InvoicePosition",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value",
                table: "InvoicePosition");

            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "InvoicePosition",
                newName: "Name");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "InvoicePosition",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPrice",
                table: "InvoicePosition",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
