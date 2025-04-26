using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentIt.Bookings.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexToBookings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Bookings_Status_EndDate",
                table: "Bookings",
                columns: new[] { "Status", "EndDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bookings_Status_EndDate",
                table: "Bookings");
        }
    }
}
