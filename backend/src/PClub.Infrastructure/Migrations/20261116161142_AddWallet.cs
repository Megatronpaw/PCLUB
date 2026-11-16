using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PClub.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWallet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "balance_cents",
                table: "users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "refunded_cents",
                table: "bookings",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateTable(
                name: "wallet_transactions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    amount_cents = table.Column<long>(type: "bigint", nullable: false),
                    balance_after_cents = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    booking_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_wallet_transactions", x => x.id);
                    table.ForeignKey(
                        name: "fk_wallet_transactions_bookings_booking_id",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_wallet_transactions_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddCheckConstraint(
                name: "users_balance_non_negative",
                table: "users",
                sql: "balance_cents >= 0");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_booking_id",
                table: "wallet_transactions",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "ix_wallet_transactions_user_id_created_at",
                table: "wallet_transactions",
                columns: new[] { "user_id", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "wallet_transactions");

            migrationBuilder.DropCheckConstraint(
                name: "users_balance_non_negative",
                table: "users");

            migrationBuilder.DropColumn(
                name: "balance_cents",
                table: "users");

            migrationBuilder.DropColumn(
                name: "refunded_cents",
                table: "bookings");
        }
    }
}
