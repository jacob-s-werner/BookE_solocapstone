using Microsoft.EntityFrameworkCore.Migrations;

namespace BookEWebsite.Migrations
{
    public partial class ChangesRecurringToBooked : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recurring",
                table: "BusinessAvailabilities");

            migrationBuilder.DropColumn(
                name: "Recurring",
                table: "ArtistAvailabilities");

            migrationBuilder.AddColumn<bool>(
                name: "Booked",
                table: "BusinessAvailabilities",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Booked",
                table: "ArtistAvailabilities",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Booked",
                table: "BusinessAvailabilities");

            migrationBuilder.DropColumn(
                name: "Booked",
                table: "ArtistAvailabilities");

            migrationBuilder.AddColumn<bool>(
                name: "Recurring",
                table: "BusinessAvailabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Recurring",
                table: "ArtistAvailabilities",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
