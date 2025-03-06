using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace RentIt.Users.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UsingHasData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "role_id", "role_name" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), "Admin" },
                    { new Guid("22222222-2222-2222-2222-222222222222"), "User" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), "Landlord" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "user_id", "created_at", "email", "first_name", "last_name", "normalized_email", "password_hash", "RefreshToken", "RefreshTokenExpiryTime", "role_id", "status", "updated_at" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc), "admin@example.com", "Admin", "Adminov", "admin@example.com", "HASHED_testadmin", "TEST_REFRESH_TOKEN_ADMIN", new DateTime(2025, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-1111-1111-1111-111111111111"), "Active", new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc), "john.doe@example.com", "John", "Doe", "john.doe@example.com", "HASHED_testuser", "TEST_REFRESH_TOKEN_USER", new DateTime(2025, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-2222-2222-2222-222222222222"), "Active", new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc), "michael.smith@example.com", "Michael", "Smith", "michael.smith@example.com", "HASHED_testlandlord", "TEST_REFRESH_TOKEN_LANDLORD", new DateTime(2025, 3, 9, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-3333-3333-3333-333333333333"), "Active", new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "user_profiles",
                columns: new[] { "user_id", "address", "city", "country", "created_at", "phone_number" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Admin Address", "Admin City", "Admin Country", new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc), "111-111-1111" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Doe Address", "Doe City", "Doe Country", new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc), "222-222-2222" },
                    { new Guid("66666666-6666-6666-6666-666666666666"), "Smith Address", "Smith City", "Smith Country", new DateTime(2025, 3, 2, 0, 0, 0, 0, DateTimeKind.Utc), "333-333-3333" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "user_profiles",
                keyColumn: "user_id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "user_profiles",
                keyColumn: "user_id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "user_profiles",
                keyColumn: "user_id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "user_id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "user_id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "user_id",
                keyValue: new Guid("66666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "role_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "role_id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "role_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));
        }
    }
}
