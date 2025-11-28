using Backend.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class coursedetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "aim_and_objectives",
                table: "course_versions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<CILOs[]>(
                name: "cilos",
                table: "course_versions",
                type: "jsonb",
                nullable: false,
                defaultValue: new CILOs[0]);

            migrationBuilder.AddColumn<string>(
                name: "course_content",
                table: "course_versions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TLAs[]>(
                name: "tlas",
                table: "course_versions",
                type: "jsonb",
                nullable: false,
                defaultValue: new TLAs[0]);

            migrationBuilder.CreateTable(
                name: "course_assessments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_version_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    weighting = table.Column<decimal>(type: "numeric", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_assessments", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_assessments_course_versions_course_version_id",
                        column: x => x.course_version_id,
                        principalTable: "course_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "medium_of_instructions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medium_of_instructions", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "course_version_mediums",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_version_id = table.Column<int>(type: "integer", nullable: false),
                    medium_of_instruction_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_version_mediums", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_version_mediums_course_versions_course_version_id",
                        column: x => x.course_version_id,
                        principalTable: "course_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_version_mediums_medium_of_instructions_medium_of_ins~",
                        column: x => x.medium_of_instruction_id,
                        principalTable: "medium_of_instructions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_course_assessments_course_version_id",
                table: "course_assessments",
                column: "course_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_version_mediums_course_version_id",
                table: "course_version_mediums",
                column: "course_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_version_mediums_medium_of_instruction_id",
                table: "course_version_mediums",
                column: "medium_of_instruction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "course_assessments");

            migrationBuilder.DropTable(
                name: "course_version_mediums");

            migrationBuilder.DropTable(
                name: "medium_of_instructions");

            migrationBuilder.DropColumn(
                name: "aim_and_objectives",
                table: "course_versions");

            migrationBuilder.DropColumn(
                name: "cilos",
                table: "course_versions");

            migrationBuilder.DropColumn(
                name: "course_content",
                table: "course_versions");

            migrationBuilder.DropColumn(
                name: "tlas",
                table: "course_versions");
        }
    }
}
