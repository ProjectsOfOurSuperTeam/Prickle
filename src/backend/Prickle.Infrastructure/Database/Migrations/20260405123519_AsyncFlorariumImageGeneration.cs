using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prickle.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AsyncFlorariumImageGeneration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "hex_color",
                table: "soil_types",
                type: "character varying(7)",
                maxLength: 7,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "florarium_image_completed_at",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "florarium_image_generation_error",
                table: "projects",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "florarium_image_generation_status",
                table: "projects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "florarium_image_requested_at",
                table: "projects",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "generated_florarium_image",
                table: "projects",
                type: "BYTEA",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "generated_florarium_image_mime_type",
                table: "projects",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "pending_florarium_canvas_image",
                table: "projects",
                type: "BYTEA",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "pending_florarium_canvas_image_mime_type",
                table: "projects",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_projects_florarium_image_generation_status",
                table: "projects",
                column: "florarium_image_generation_status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_projects_florarium_image_generation_status",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "hex_color",
                table: "soil_types");

            migrationBuilder.DropColumn(
                name: "florarium_image_completed_at",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "florarium_image_generation_error",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "florarium_image_generation_status",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "florarium_image_requested_at",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "generated_florarium_image",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "generated_florarium_image_mime_type",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "pending_florarium_canvas_image",
                table: "projects");

            migrationBuilder.DropColumn(
                name: "pending_florarium_canvas_image_mime_type",
                table: "projects");
        }
    }
}
