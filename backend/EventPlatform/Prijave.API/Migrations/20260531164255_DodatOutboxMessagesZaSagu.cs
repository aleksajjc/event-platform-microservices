using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prijave.API.Migrations
{
    /// <inheritdoc />
    public partial class DodatOutboxMessagesZaSagu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationID",
                table: "Prijave",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "StatusPrijava",
                table: "Prijave",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PrijavaZapocetaOutboxMessages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrijavaZapocetaOutboxMessages", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PrijavaZapocetaOutboxMessages");

            migrationBuilder.DropColumn(
                name: "CorrelationID",
                table: "Prijave");

            migrationBuilder.DropColumn(
                name: "StatusPrijava",
                table: "Prijave");
        }
    }
}
