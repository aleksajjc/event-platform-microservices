using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Events.API.Migrations
{
    /// <inheritdoc />
    public partial class Inicijalna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Lokacije",
                columns: table => new
                {
                    LokacijaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Adresa = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Kapacitet = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lokacije", x => x.LokacijaID);
                });

            migrationBuilder.CreateTable(
                name: "Predavaci",
                columns: table => new
                {
                    PredavacID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Prezime = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Titula = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OblastStrucnosti = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Predavaci", x => x.PredavacID);
                });

            migrationBuilder.CreateTable(
                name: "TipoviDogadjaja",
                columns: table => new
                {
                    TipDogadjajaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NazivTipa = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoviDogadjaja", x => x.TipDogadjajaID);
                });

            migrationBuilder.CreateTable(
                name: "StrucniDogadjaji",
                columns: table => new
                {
                    StrucniDogadjajID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agenda = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DatumVremeOdrzavanja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Trajanje = table.Column<double>(type: "float", nullable: false),
                    CenaKotizacije = table.Column<double>(type: "float", nullable: false),
                    LokacijaID = table.Column<int>(type: "int", nullable: false),
                    TipDogadjajaID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrucniDogadjaji", x => x.StrucniDogadjajID);
                    table.ForeignKey(
                        name: "FK_StrucniDogadjaji_Lokacije_LokacijaID",
                        column: x => x.LokacijaID,
                        principalTable: "Lokacije",
                        principalColumn: "LokacijaID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StrucniDogadjaji_TipoviDogadjaja_TipDogadjajaID",
                        column: x => x.TipDogadjajaID,
                        principalTable: "TipoviDogadjaja",
                        principalColumn: "TipDogadjajaID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "StrucniDogadjajiPredavaci",
                columns: table => new
                {
                    PredavacID = table.Column<int>(type: "int", nullable: false),
                    StrucniDogadjajID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StrucniDogadjajiPredavaci", x => new { x.PredavacID, x.StrucniDogadjajID });
                    table.ForeignKey(
                        name: "FK_StrucniDogadjajiPredavaci_Predavaci_PredavacID",
                        column: x => x.PredavacID,
                        principalTable: "Predavaci",
                        principalColumn: "PredavacID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StrucniDogadjajiPredavaci_StrucniDogadjaji_StrucniDogadjajID",
                        column: x => x.StrucniDogadjajID,
                        principalTable: "StrucniDogadjaji",
                        principalColumn: "StrucniDogadjajID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StrucniDogadjaji_LokacijaID",
                table: "StrucniDogadjaji",
                column: "LokacijaID");

            migrationBuilder.CreateIndex(
                name: "IX_StrucniDogadjaji_TipDogadjajaID",
                table: "StrucniDogadjaji",
                column: "TipDogadjajaID");

            migrationBuilder.CreateIndex(
                name: "IX_StrucniDogadjajiPredavaci_StrucniDogadjajID",
                table: "StrucniDogadjajiPredavaci",
                column: "StrucniDogadjajID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StrucniDogadjajiPredavaci");

            migrationBuilder.DropTable(
                name: "Predavaci");

            migrationBuilder.DropTable(
                name: "StrucniDogadjaji");

            migrationBuilder.DropTable(
                name: "Lokacije");

            migrationBuilder.DropTable(
                name: "TipoviDogadjaja");
        }
    }
}
