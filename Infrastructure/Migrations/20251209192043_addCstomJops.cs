using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addCstomJops : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomizationJobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceGarmentPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneratedGarmentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeApiRequestId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TryOnJobId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TryOnStatusUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TryOnResultUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    LastError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomizationJobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomizationJobs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomizationJobs_UserId",
                table: "CustomizationJobs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomizationJobs");
        }
    }
}
