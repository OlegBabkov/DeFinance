using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateReportCategoryIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Reports");

            migrationBuilder.AddColumn<string>(
                name: "category_ids",
                table: "Reports",
                type: "text",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.Sql("UPDATE \"Reports\" SET category_ids = '[]' WHERE category_ids = '' OR category_ids IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category_ids",
                table: "Reports");

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "Reports",
                type: "uuid",
                nullable: true);
        }
    }
}
