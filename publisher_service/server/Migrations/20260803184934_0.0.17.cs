using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace server.Migrations
{
    /// <inheritdoc />
    public partial class _0017 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "SmevOrder");

            migrationBuilder.DropColumn(
                name: "StatusCode",
                table: "EpguOrder");

            migrationBuilder.AddColumn<string>(
                name: "OrderStatusId",
                table: "SmevOrder",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OrderStatusId",
                table: "EpguOrder",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderStatusId",
                table: "SmevOrder");

            migrationBuilder.DropColumn(
                name: "OrderStatusId",
                table: "EpguOrder");

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "SmevOrder",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StatusCode",
                table: "EpguOrder",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
