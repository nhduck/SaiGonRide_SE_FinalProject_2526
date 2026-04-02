using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalVehicleService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVehicleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Vehicle",
                columns: table => new
                {
                    VehicleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VehicleModel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    BatteryPercentage = table.Column<int>(type: "int", nullable: false),
                    State = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrentStationId = table.Column<int>(type: "int", nullable: true),
                    IsPluggedIn = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vehicle", x => x.VehicleId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Vehicle");
        }
    }
}
