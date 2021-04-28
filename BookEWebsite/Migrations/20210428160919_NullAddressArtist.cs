using Microsoft.EntityFrameworkCore.Migrations;

namespace BookEWebsite.Migrations
{
    public partial class NullAddressArtist : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artists_Addresses_AddressId",
                table: "Artists");

            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "Artists",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Artists_Addresses_AddressId",
                table: "Artists",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Artists_Addresses_AddressId",
                table: "Artists");

            migrationBuilder.AlterColumn<int>(
                name: "AddressId",
                table: "Artists",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Artists_Addresses_AddressId",
                table: "Artists",
                column: "AddressId",
                principalTable: "Addresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
