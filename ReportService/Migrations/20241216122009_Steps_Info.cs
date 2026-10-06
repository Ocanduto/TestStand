using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportService.Migrations
{
    /// <inheritdoc />
    public partial class Steps_Info : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Info",
                table: "StepsCollection",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Info",
                table: "StepsCollection");
        }
    }
}
