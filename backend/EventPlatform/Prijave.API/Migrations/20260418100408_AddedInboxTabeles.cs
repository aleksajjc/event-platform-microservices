using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prijave.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedInboxTabeles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DogadjajReferences",
                columns: table => new
                {
                    DogadjajReferenceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Naziv = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DogadjajReferences", x => x.DogadjajReferenceID);
                });

            migrationBuilder.CreateTable(
                name: "ProcessedMessages",
                columns: table => new
                {
                    EventId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProcessedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessedMessages", x => x.EventId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DogadjajReferences");

            migrationBuilder.DropTable(
                name: "ProcessedMessages");
        }
    }
}
