using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryPaymentObligation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentObligation",
                table: "Categories",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentObligation",
                table: "Categories");
        }
    }
}
