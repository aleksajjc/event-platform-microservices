using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Placanja.API.Migrations
{
    /// <inheritdoc />
    public partial class InicijalnaPlacanja : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RacuniUcesnika",
                columns: table => new
                {
                    UcesnikID = table.Column<int>(type: "int", nullable: false),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StanjeNaRacunu = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RacuniUcesnika", x => x.UcesnikID);
                });

            migrationBuilder.InsertData(
                table: "RacuniUcesnika",
                columns: new[] { "UcesnikID", "Email", "Ime", "Prezime", "StanjeNaRacunu" },
                values: new object[,]
                {
                    { 1, "aleksa@example.com", "Aleksa", "Jovanovic", 5000.0 },
                    { 2, "marko@example.com", "Marko", "Markovic", 50.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RacuniUcesnika");
        }
    }
}
