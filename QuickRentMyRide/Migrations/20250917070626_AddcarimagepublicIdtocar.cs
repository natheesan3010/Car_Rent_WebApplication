using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuickRentMyRide.Migrations
{
    /// <inheritdoc />
    public partial class AddcarimagepublicIdtocar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarImagePublicId",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarImagePublicId",
                table: "Cars");
        }
    }
}
