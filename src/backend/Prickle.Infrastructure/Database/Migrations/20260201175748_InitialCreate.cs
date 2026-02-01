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
                name: "ix_soil_type_soil_formulas_soil_type_id",
                table: "soil_type_soil_formulas",
                column: "soil_type_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "soil_type_soil_formulas");

            migrationBuilder.DropTable(
                name: "soil_formulas");

            migrationBuilder.DropTable(
                name: "soil_types");
        }
    }
}
