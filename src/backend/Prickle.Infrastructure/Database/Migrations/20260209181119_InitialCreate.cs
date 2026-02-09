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
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                name: "plants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    name_latin = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    image_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    image_isometric_url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
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
                name: "ix_plants_humidity_level",
                table: "plants",
                column: "humidity_level");

            migrationBuilder.CreateIndex(
                name: "ix_plants_light_level",
                table: "plants",
                column: "light_level");

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
                name: "ix_plants_water_need",
                table: "plants",
                column: "water_need");

            migrationBuilder.CreateIndex(
                name: "ix_soil_type_soil_formulas_soil_type_id",
                table: "soil_type_soil_formulas",
                column: "soil_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "containers");

            migrationBuilder.DropTable(
                name: "decorations");

            migrationBuilder.DropTable(
                name: "plants");

            migrationBuilder.DropTable(
                name: "soil_type_soil_formulas");

            migrationBuilder.DropTable(
                name: "soil_formulas");

            migrationBuilder.DropTable(
                name: "soil_types");
        }
    }
}
