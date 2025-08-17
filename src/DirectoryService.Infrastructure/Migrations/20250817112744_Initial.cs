using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DirectoryService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_id = table.Column<Guid>(type: "uuid", nullable: true),
                    depth = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    identifier = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    path = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_departments", x => x.id);
                    table.CheckConstraint("CK_departments_identifier_format", "\"identifier\" ~ '^[A-Za-z-]+$'");
                    table.CheckConstraint("CK_departments_identifier_length", "char_length(\"identifier\") >= 3 AND char_length(\"identifier\") <= 150");
                    table.CheckConstraint("CK_departments_name_cyrillic", "\"name\" ~ '^[А-Яа-яЁё -]+$'");
                    table.CheckConstraint("CK_departments_name_length", "char_length(\"name\") >= 3 AND char_length(\"name\") <= 150");
                    table.CheckConstraint("CK_departments_path", "\"path\" ~ '^(?=.*[A-Za-z])[A-Za-z.-]+$'");
                    table.ForeignKey(
                        name: "FK_departments_departments_parent_id",
                        column: x => x.parent_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    address_building = table.Column<string>(type: "text", nullable: false),
                    address_city = table.Column<string>(type: "text", nullable: false),
                    address_country = table.Column<string>(type: "text", nullable: false),
                    address_street = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    timezone = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locations", x => x.id);
                    table.CheckConstraint("CK_locations_address_building", "\"address_building\" ~ '^[A-Za-zА-Яа-яЁё0-9 \\-]+$'");
                    table.CheckConstraint("CK_locations_address_city", "\"address_city\" ~ '^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9 .-]+$'");
                    table.CheckConstraint("CK_locations_address_country", "\"address_country\" ~ '^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9 .-]+$'");
                    table.CheckConstraint("CK_locations_address_street", "\"address_street\" ~ '^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9 .-]+$'");
                    table.CheckConstraint("CK_locations_name_format", "\"name\" ~ '^(?=.*[A-Za-zА-Яа-яЁё0-9])[A-Za-zА-Яа-яЁё0-9 .-]+$'");
                    table.CheckConstraint("CK_locations_name_length", "char_length(\"name\") >= 3 AND char_length(\"name\") <= 120");
                });

            migrationBuilder.CreateTable(
                name: "positions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_positions", x => x.id);
                    table.CheckConstraint("CK_positions_description_length", "description IS NULL OR char_length(description) <= 1000");
                    table.CheckConstraint("CK_positions_name_format", "\"name\" ~ '^[A-Za-zА-Яа-яЁё\\s.-]+$'");
                    table.CheckConstraint("CK_positions_name_length", "char_length(\"name\") >= 3 AND char_length(\"name\") <= 100");
                });

            migrationBuilder.CreateTable(
                name: "department_locations",
                columns: table => new
                {
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    location_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department_locations", x => new { x.department_id, x.location_id });
                    table.ForeignKey(
                        name: "FK_department_locations_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_department_locations_locations_location_id",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "department_positions",
                columns: table => new
                {
                    department_id = table.Column<Guid>(type: "uuid", nullable: false),
                    position_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_department_positions", x => new { x.department_id, x.position_id });
                    table.ForeignKey(
                        name: "FK_department_positions_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_department_positions_positions_position_id",
                        column: x => x.position_id,
                        principalTable: "positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_department_locations_location_id",
                table: "department_locations",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_department_positions_position_id",
                table: "department_positions",
                column: "position_id");

            migrationBuilder.CreateIndex(
                name: "IX_departments_parent_id",
                table: "departments",
                column: "parent_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "department_locations");

            migrationBuilder.DropTable(
                name: "department_positions");

            migrationBuilder.DropTable(
                name: "locations");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "positions");
        }
    }
}
