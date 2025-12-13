using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class remove20251212235105_addCustomizationJobDesignAndFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DesignImageUrl",
                table: "CustomizationJobs");

            migrationBuilder.DropColumn(
                name: "FinalProductImageUrl",
                table: "CustomizationJobs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DesignImageUrl",
                table: "CustomizationJobs",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalProductImageUrl",
                table: "CustomizationJobs",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
