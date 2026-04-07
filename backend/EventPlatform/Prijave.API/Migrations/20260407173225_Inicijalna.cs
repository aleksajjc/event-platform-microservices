using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prijave.API.Migrations
{
    /// <inheritdoc />
    public partial class Inicijalna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ucesnici",
                columns: table => new
                {
                    UcesnikID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ucesnici", x => x.UcesnikID);
                });

            migrationBuilder.CreateTable(
                name: "Prijave",
                columns: table => new
                {
                    UcesnikID = table.Column<int>(type: "int", nullable: false),
                    StrucniDogadjajID = table.Column<int>(type: "int", nullable: false),
                    DatumPrijave = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prijave", x => new { x.UcesnikID, x.StrucniDogadjajID });
                    table.ForeignKey(
                        name: "FK_Prijave_Ucesnici_UcesnikID",
                        column: x => x.UcesnikID,
                        principalTable: "Ucesnici",
                        principalColumn: "UcesnikID",
                        onDelete: ReferentialAction.Restrict);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Prijave");

            migrationBuilder.DropTable(
                name: "Ucesnici");
        }
    }
}
