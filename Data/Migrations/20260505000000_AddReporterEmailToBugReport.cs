using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalVehicleService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReporterEmailToBugReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReporterEmail",
                table: "BugReport",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReporterEmail",
                table: "BugReport");
        }
    }
}
