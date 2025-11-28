using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class roles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "08fb9bd6-eeda-4063-9be8-ffadd4a09a39", "4e544a59-030f-4446-86ef-8ded2fa0aebf", "Admin", "ADMIN" },
                    { "f54b9699-348d-45b3-8fbb-83d5591b2aef", "5bf897a4-3ddd-41f3-8f8a-5526f3b008dd", "Student", "STUDENT" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "08fb9bd6-eeda-4063-9be8-ffadd4a09a39");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f54b9699-348d-45b3-8fbb-83d5591b2aef");
        }
    }
}
