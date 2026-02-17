using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Prickle.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "containers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    volume = table.Column<float>(type: "real", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    image_isometric_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_containers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "decorations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    image_isometric_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_decorations", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "soil_formulas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_soil_formulas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "soil_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_soil_types", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "projects",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    container_id = table.Column<Guid>(type: "uuid", nullable: false),
                    preview = table.Column<byte[]>(type: "BYTEA", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    is_published = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_projects", x => x.id);
                    table.ForeignKey(
                        name: "fk_projects_containers_container_id",
                        column: x => x.container_id,
                        principalTable: "containers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "plants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name_latin = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    image_isometric_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    category = table.Column<int>(type: "integer", nullable: false),
                    light_level = table.Column<int>(type: "integer", nullable: false),
                    water_need = table.Column<int>(type: "integer", nullable: false),
                    humidity_level = table.Column<int>(type: "integer", nullable: false),
                    item_max_size = table.Column<int>(type: "integer", nullable: false),
                    soil_formula_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_plants", x => x.id);
                    table.ForeignKey(
                        name: "fk_plants_soil_formulas_soil_formula_id",
                        column: x => x.soil_formula_id,
                        principalTable: "soil_formulas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "soil_type_soil_formulas",
                columns: table => new
                {
                    soil_formula_id = table.Column<Guid>(type: "uuid", nullable: false),
                    soil_type_id = table.Column<int>(type: "integer", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    percentage = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_soil_type_soil_formulas", x => new { x.soil_formula_id, x.soil_type_id, x.order });
                    table.ForeignKey(
                        name: "fk_soil_type_soil_formulas_soil_formulas_soil_formula_id",
                        column: x => x.soil_formula_id,
                        principalTable: "soil_formulas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_soil_type_soil_formulas_soil_types_soil_type_id",
                        column: x => x.soil_type_id,
                        principalTable: "soil_types",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "project_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    project_id = table.Column<Guid>(type: "uuid", nullable: false),
                    item_type = table.Column<int>(type: "integer", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pos_x = table.Column<int>(type: "integer", nullable: false),
                    pos_y = table.Column<int>(type: "integer", nullable: false),
                    pos_z = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_project_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_project_items_projects_project_id",
                        column: x => x.project_id,
                        principalTable: "projects",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_containers_name",
                table: "containers",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_decorations_category",
                table: "decorations",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "ix_decorations_name",
                table: "decorations",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plants_name",
                table: "plants",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_plants_name_latin",
                table: "plants",
                column: "name_latin");

            migrationBuilder.CreateIndex(
                name: "ix_plants_soil_formula_id",
                table: "plants",
                column: "soil_formula_id");

            migrationBuilder.CreateIndex(
                name: "ix_project_items_item_type",
                table: "project_items",
                column: "item_type");

            migrationBuilder.CreateIndex(
                name: "ix_project_items_project_id",
                table: "project_items",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_container_id",
                table: "projects",
                column: "container_id");

            migrationBuilder.CreateIndex(
                name: "ix_projects_created_at",
                table: "projects",
                column: "created_at",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "ix_projects_user_id",
                table: "projects",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_soil_type_soil_formulas_soil_type_id",
                table: "soil_type_soil_formulas",
                column: "soil_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "decorations");

            migrationBuilder.DropTable(
                name: "plants");

            migrationBuilder.DropTable(
                name: "project_items");

            migrationBuilder.DropTable(
                name: "soil_type_soil_formulas");

            migrationBuilder.DropTable(
                name: "projects");

            migrationBuilder.DropTable(
                name: "soil_formulas");

            migrationBuilder.DropTable(
                name: "soil_types");

            migrationBuilder.DropTable(
                name: "containers");
        }
    }
}
