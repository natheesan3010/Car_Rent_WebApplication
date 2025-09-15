using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRentMyRide.Migrations
{
    /// <inheritdoc />
    public partial class NR : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OTP",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "OTPGeneratedAt",
                table: "Bookings");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OTP",
                table: "Bookings",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "OTPGeneratedAt",
                table: "Bookings",
                type: "datetime2",
                nullable: true);
        }
    }
}
