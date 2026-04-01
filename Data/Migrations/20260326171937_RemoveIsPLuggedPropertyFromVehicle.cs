using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalVehicleService.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsPLuggedPropertyFromVehicle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPluggedIn",
                table: "Vehicle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPluggedIn",
                table: "Vehicle",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
