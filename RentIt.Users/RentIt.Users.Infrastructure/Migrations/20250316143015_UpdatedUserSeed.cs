using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RentIt.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedUserSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "user_id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "password_hash",
                value: "$2a$11$VkCOxZ3UEmKm7q54hd069uLhehwjFJD753vEAeTwDeB9wkA3MyBNW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "user_id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"),
                column: "password_hash",
                value: "HASHED_testadmin");
        }
    }
}
