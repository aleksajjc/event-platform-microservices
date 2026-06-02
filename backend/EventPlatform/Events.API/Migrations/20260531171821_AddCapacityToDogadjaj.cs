using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCapacityToDogadjaj : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaksimalanKapacitet",
                table: "StrucniDogadjaji",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SlobodnaMesta",
                table: "StrucniDogadjaji",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaksimalanKapacitet",
                table: "StrucniDogadjaji");

            migrationBuilder.DropColumn(
                name: "SlobodnaMesta",
                table: "StrucniDogadjaji");
        }
    }
}
