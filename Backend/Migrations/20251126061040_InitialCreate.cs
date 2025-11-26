using System;
using Backend.Models;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: true),
                    rules = table.Column<RuleNode>(type: "jsonb", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: false),
                    min_credit = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "codes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tag = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_codes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "course_groups",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_groups", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "programmes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programmes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "terms",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_terms", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    code_id = table.Column<int>(type: "integer", nullable: false),
                    course_number = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_courses", x => x.id);
                    table.ForeignKey(
                        name: "FK_courses_codes_code_id",
                        column: x => x.code_id,
                        principalTable: "codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "category_groups",
                columns: table => new
                {
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    group_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category_groups", x => new { x.category_id, x.group_id });
                    table.ForeignKey(
                        name: "FK_category_groups_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_category_groups_course_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "course_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "programme_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    programme_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_edited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programme_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_programme_versions_programmes_programme_id",
                        column: x => x.programme_id,
                        principalTable: "programmes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_departments",
                columns: table => new
                {
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    department_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_departments", x => new { x.course_id, x.department_id });
                    table.ForeignKey(
                        name: "FK_course_departments_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_departments_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_versions",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_id = table.Column<int>(type: "integer", nullable: false),
                    credit = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_versions", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_versions_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_courses",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_id = table.Column<int>(type: "integer", nullable: false),
                    course_id = table.Column<int>(type: "integer", nullable: true),
                    code_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_courses", x => x.id);
                    table.ForeignKey(
                        name: "FK_group_courses_codes_code_id",
                        column: x => x.code_id,
                        principalTable: "codes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_courses_course_groups_group_id",
                        column: x => x.group_id,
                        principalTable: "course_groups",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_courses_courses_course_id",
                        column: x => x.course_id,
                        principalTable: "courses",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "programme_categories",
                columns: table => new
                {
                    programme_version_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_programme_categories", x => new { x.programme_version_id, x.category_id });
                    table.ForeignKey(
                        name: "FK_programme_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_programme_categories_programme_versions_programme_version_id",
                        column: x => x.programme_version_id,
                        principalTable: "programme_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_programmes",
                columns: table => new
                {
                    student_id = table.Column<int>(type: "integer", nullable: false),
                    programme_version_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_student_programmes", x => new { x.student_id, x.programme_version_id });
                    table.ForeignKey(
                        name: "FK_student_programmes_programme_versions_programme_version_id",
                        column: x => x.programme_version_id,
                        principalTable: "programme_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_student_programmes_users_student_id",
                        column: x => x.student_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_anti_reqs",
                columns: table => new
                {
                    course_version_id = table.Column<int>(type: "integer", nullable: false),
                    excluded_course_version_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_anti_reqs", x => new { x.course_version_id, x.excluded_course_version_id });
                    table.ForeignKey(
                        name: "FK_course_anti_reqs_course_versions_course_version_id",
                        column: x => x.course_version_id,
                        principalTable: "course_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_anti_reqs_course_versions_excluded_course_version_id",
                        column: x => x.excluded_course_version_id,
                        principalTable: "course_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_pre_reqs",
                columns: table => new
                {
                    course_version_id = table.Column<int>(type: "integer", nullable: false),
                    required_course_version_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_pre_reqs", x => new { x.course_version_id, x.required_course_version_id });
                    table.ForeignKey(
                        name: "FK_course_pre_reqs_course_versions_course_version_id",
                        column: x => x.course_version_id,
                        principalTable: "course_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_pre_reqs_course_versions_required_course_version_id",
                        column: x => x.required_course_version_id,
                        principalTable: "course_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_sections",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    course_version_id = table.Column<int>(type: "integer", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    term_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_sections", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_sections_course_versions_course_version_id",
                        column: x => x.course_version_id,
                        principalTable: "course_versions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_course_sections_terms_term_id",
                        column: x => x.term_id,
                        principalTable: "terms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "course_meetings",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    section_id = table.Column<int>(type: "integer", nullable: false),
                    meeting_type = table.Column<string>(type: "text", nullable: false),
                    day = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_course_meetings", x => x.id);
                    table.ForeignKey(
                        name: "FK_course_meetings_course_sections_section_id",
                        column: x => x.section_id,
                        principalTable: "course_sections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_category_groups_group_id",
                table: "category_groups",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_anti_reqs_excluded_course_version_id",
                table: "course_anti_reqs",
                column: "excluded_course_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_departments_department_id",
                table: "course_departments",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_meetings_section_id",
                table: "course_meetings",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_pre_reqs_required_course_version_id",
                table: "course_pre_reqs",
                column: "required_course_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_sections_course_version_id",
                table: "course_sections",
                column: "course_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_sections_term_id",
                table: "course_sections",
                column: "term_id");

            migrationBuilder.CreateIndex(
                name: "IX_course_versions_course_id",
                table: "course_versions",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_courses_code_id",
                table: "courses",
                column: "code_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_courses_code_id",
                table: "group_courses",
                column: "code_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_courses_course_id",
                table: "group_courses",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_group_courses_group_id",
                table: "group_courses",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_programme_categories_category_id",
                table: "programme_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_programme_versions_programme_id",
                table: "programme_versions",
                column: "programme_id");

            migrationBuilder.CreateIndex(
                name: "IX_student_programmes_programme_version_id",
                table: "student_programmes",
                column: "programme_version_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_role_id",
                table: "users",
                column: "role_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "category_groups");

            migrationBuilder.DropTable(
                name: "course_anti_reqs");

            migrationBuilder.DropTable(
                name: "course_departments");

            migrationBuilder.DropTable(
                name: "course_meetings");

            migrationBuilder.DropTable(
                name: "course_pre_reqs");

            migrationBuilder.DropTable(
                name: "group_courses");

            migrationBuilder.DropTable(
                name: "programme_categories");

            migrationBuilder.DropTable(
                name: "student_programmes");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "course_sections");

            migrationBuilder.DropTable(
                name: "course_groups");

            migrationBuilder.DropTable(
                name: "categories");

            migrationBuilder.DropTable(
                name: "programme_versions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "course_versions");

            migrationBuilder.DropTable(
                name: "terms");

            migrationBuilder.DropTable(
                name: "programmes");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "codes");
        }
    }
}
