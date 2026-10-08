using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingOverlapConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            migrationBuilder.Sql(
                """
                ALTER TABLE bookings
                ADD CONSTRAINT bookings_no_overlap
                EXCLUDE USING gist (
                    seat_id WITH =,
                    tstzrange(start_time, end_time, '[)') WITH &&
                )
                WHERE (status <> 'Cancelled');
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down пишем всегда: миграция без отката — билет в одну сторону.
            migrationBuilder.Sql(
                "ALTER TABLE bookings DROP CONSTRAINT IF EXISTS bookings_no_overlap;");

            // Расширение не удаляем: им могут пользоваться другие ограничения.
        }
    }
}
