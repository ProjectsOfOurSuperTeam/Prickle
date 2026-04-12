using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prickle.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddDecorationItemMaxSize : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "item_max_size",
                table: "decorations",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "item_max_size",
                table: "decorations");
        }
    }
}
