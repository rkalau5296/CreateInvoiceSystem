using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NipIsUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Nip",
                table: "Clients",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Nip",
                table: "Clients",
                column: "Nip",
                unique: true,
                filter: "[Nip] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Nip",
                table: "Clients");

            migrationBuilder.AlterColumn<string>(
                name: "Nip",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
