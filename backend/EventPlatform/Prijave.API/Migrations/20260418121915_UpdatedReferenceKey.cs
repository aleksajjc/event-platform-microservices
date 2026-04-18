using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prijave.API.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedReferenceKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StrucniDogadjajID",
                table: "DogadjajReferences",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StrucniDogadjajID",
                table: "DogadjajReferences");
        }
    }
}
