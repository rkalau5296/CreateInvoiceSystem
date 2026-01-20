using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CreateInvoiceSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ClientNipUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Clients_Nip_UserId",
                table: "Clients");

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

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Nip_UserId",
                table: "Clients",
                columns: new[] { "Nip", "UserId" },
                unique: true,
                filter: "[Nip] IS NOT NULL");
        }
    }
}
