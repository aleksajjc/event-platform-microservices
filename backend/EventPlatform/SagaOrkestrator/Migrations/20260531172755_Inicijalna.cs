using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SagaOrkestrator.Migrations
{
    /// <inheritdoc />
    public partial class Inicijalna : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SagaCommandOutboxMessages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrelationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QueueName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaCommandOutboxMessages", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SagaStates",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CorrelationID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StrucniDogadjajID = table.Column<int>(type: "int", nullable: false),
                    UcesnikID = table.Column<int>(type: "int", nullable: false),
                    CenaKotizacije = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TrenutniKorak = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Greska = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagaStates", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SagaCommandOutboxMessages");

            migrationBuilder.DropTable(
                name: "SagaStates");
        }
    }
}
