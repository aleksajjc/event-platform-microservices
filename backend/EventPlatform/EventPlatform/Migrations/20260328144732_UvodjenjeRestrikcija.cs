using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UvodjenjeRestrikcija : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StrucniDogadjaji_Lokacije_LokacijaID",
                table: "StrucniDogadjaji");

            migrationBuilder.DropForeignKey(
                name: "FK_StrucniDogadjaji_TipoviDogadjaja_TipDogadjajaID",
                table: "StrucniDogadjaji");

            migrationBuilder.DropForeignKey(
                name: "FK_StrucniDogadjajiPredavaci_Predavaci_PredavacID",
                table: "StrucniDogadjajiPredavaci");

            migrationBuilder.DropForeignKey(
                name: "FK_StrucniDogadjajiPredavaci_StrucniDogadjaji_StrucniDogadjajID",
                table: "StrucniDogadjajiPredavaci");

            migrationBuilder.AddForeignKey(
                name: "FK_StrucniDogadjaji_Lokacije_LokacijaID",
                table: "StrucniDogadjaji",
                column: "LokacijaID",
                principalTable: "Lokacije",
                principalColumn: "LokacijaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StrucniDogadjaji_TipoviDogadjaja_TipDogadjajaID",
                table: "StrucniDogadjaji",
                column: "TipDogadjajaID",
                principalTable: "TipoviDogadjaja",
                principalColumn: "TipDogadjajaID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StrucniDogadjajiPredavaci_Predavaci_PredavacID",
                table: "StrucniDogadjajiPredavaci",
                column: "PredavacID",
                principalTable: "Predavaci",
                principalColumn: "PredavacID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StrucniDogadjajiPredavaci_StrucniDogadjaji_StrucniDogadjajID",
                table: "StrucniDogadjajiPredavaci",
                column: "StrucniDogadjajID",
                principalTable: "StrucniDogadjaji",
                principalColumn: "StrucniDogadjajID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StrucniDogadjaji_Lokacije_LokacijaID",
                table: "StrucniDogadjaji");

            migrationBuilder.DropForeignKey(
                name: "FK_StrucniDogadjaji_TipoviDogadjaja_TipDogadjajaID",
                table: "StrucniDogadjaji");

            migrationBuilder.DropForeignKey(
                name: "FK_StrucniDogadjajiPredavaci_Predavaci_PredavacID",
                table: "StrucniDogadjajiPredavaci");

            migrationBuilder.DropForeignKey(
                name: "FK_StrucniDogadjajiPredavaci_StrucniDogadjaji_StrucniDogadjajID",
                table: "StrucniDogadjajiPredavaci");

            migrationBuilder.AddForeignKey(
                name: "FK_StrucniDogadjaji_Lokacije_LokacijaID",
                table: "StrucniDogadjaji",
                column: "LokacijaID",
                principalTable: "Lokacije",
                principalColumn: "LokacijaID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StrucniDogadjaji_TipoviDogadjaja_TipDogadjajaID",
                table: "StrucniDogadjaji",
                column: "TipDogadjajaID",
                principalTable: "TipoviDogadjaja",
                principalColumn: "TipDogadjajaID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StrucniDogadjajiPredavaci_Predavaci_PredavacID",
                table: "StrucniDogadjajiPredavaci",
                column: "PredavacID",
                principalTable: "Predavaci",
                principalColumn: "PredavacID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StrucniDogadjajiPredavaci_StrucniDogadjaji_StrucniDogadjajID",
                table: "StrucniDogadjajiPredavaci",
                column: "StrucniDogadjajID",
                principalTable: "StrucniDogadjaji",
                principalColumn: "StrucniDogadjajID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
