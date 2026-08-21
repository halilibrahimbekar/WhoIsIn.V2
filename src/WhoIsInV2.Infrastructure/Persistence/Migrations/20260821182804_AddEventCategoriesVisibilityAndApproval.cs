using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhoIsInV2.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEventCategoriesVisibilityAndApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Events",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RequireApproval",
                table: "Events",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Visibility",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

                migrationBuilder.Sql("INSERT INTO \"Categories\" (\"Id\", \"Name\") SELECT gen_random_uuid(), defaults.name FROM (VALUES ('Conference'), ('Workshop'), ('Meetup'), ('Social'), ('Other')) AS defaults(name) WHERE NOT EXISTS (SELECT 1 FROM \"Categories\" c WHERE c.\"Name\" = defaults.name)");
                migrationBuilder.Sql("INSERT INTO \"Categories\" (\"Id\", \"Name\") SELECT gen_random_uuid(), e.\"Category\" FROM \"Events\" e WHERE e.\"Category\" IS NOT NULL AND e.\"Category\" <> '' AND NOT EXISTS (SELECT 1 FROM \"Categories\" c WHERE c.\"Name\" = e.\"Category\") GROUP BY e.\"Category\"");
            migrationBuilder.Sql("UPDATE \"Events\" AS e SET \"CategoryId\" = c.\"Id\" FROM \"Categories\" AS c WHERE e.\"Category\" = c.\"Name\"");
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Events");

            migrationBuilder.CreateIndex(
                name: "IX_Events_CategoryId",
                table: "Events",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Categories_CategoryId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Events_CategoryId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "RequireApproval",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Visibility",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Events",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }
    }
}
