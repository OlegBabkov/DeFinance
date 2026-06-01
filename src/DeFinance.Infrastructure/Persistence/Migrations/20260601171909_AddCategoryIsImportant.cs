using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryIsImportant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsImportant",
                table: "Categories",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsImportant",
                table: "Categories");
        }
    }
}
