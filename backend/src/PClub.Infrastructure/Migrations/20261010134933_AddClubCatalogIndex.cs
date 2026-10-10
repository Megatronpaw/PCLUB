using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClubCatalogIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_clubs_city_status",
                table: "clubs");

            migrationBuilder.CreateIndex(
                name: "ix_clubs_status_city_name",
                table: "clubs",
                columns: new[] { "status", "city", "name" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_clubs_status_city_name",
                table: "clubs");

            migrationBuilder.CreateIndex(
                name: "ix_clubs_city_status",
                table: "clubs",
                columns: new[] { "city", "status" });
        }
    }
}
