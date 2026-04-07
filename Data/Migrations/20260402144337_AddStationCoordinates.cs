using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentalVehicleService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStationCoordinates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Stations_EndStationId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Stations_StartStationId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Vehicles_VehicleId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Stations_CurrentStationId",
                table: "Vehicles");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Stations",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Stations",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Stations_EndStationId",
                table: "Rentals",
                column: "EndStationId",
                principalTable: "Stations",
                principalColumn: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Stations_StartStationId",
                table: "Rentals",
                column: "StartStationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Vehicles_VehicleId",
                table: "Rentals",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Stations_CurrentStationId",
                table: "Vehicles",
                column: "CurrentStationId",
                principalTable: "Stations",
                principalColumn: "StationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Stations_EndStationId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Stations_StartStationId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Rentals_Vehicles_VehicleId",
                table: "Rentals");

            migrationBuilder.DropForeignKey(
                name: "FK_Vehicles_Stations_CurrentStationId",
                table: "Vehicles");

            migrationBuilder.AlterColumn<double>(
                name: "Longitude",
                table: "Stations",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AlterColumn<double>(
                name: "Latitude",
                table: "Stations",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Stations_EndStationId",
                table: "Rentals",
                column: "EndStationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Stations_StartStationId",
                table: "Rentals",
                column: "StartStationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Rentals_Vehicles_VehicleId",
                table: "Rentals",
                column: "VehicleId",
                principalTable: "Vehicles",
                principalColumn: "VehicleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Vehicles_Stations_CurrentStationId",
                table: "Vehicles",
                column: "CurrentStationId",
                principalTable: "Stations",
                principalColumn: "StationId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
