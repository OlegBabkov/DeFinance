using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToUserScopedEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OpeningBalanceOverrides_Year_Month",
                table: "OpeningBalanceOverrides");

            migrationBuilder.DropIndex(
                name: "IX_BudgetEntries_CategoryId_Year_Month",
                table: "BudgetEntries");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "OpeningBalanceOverrides",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "MandatoryPayments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Counterparties",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Categories",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "BudgetEntries",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Accounts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Assign all existing rows to the first user (OlegB's data)
            migrationBuilder.Sql(@"
                UPDATE ""Transactions""         SET ""UserId"" = u.""Id"" FROM (SELECT ""Id"" FROM ""Users"" LIMIT 1) u WHERE ""Transactions"".""UserId"" = '00000000-0000-0000-0000-000000000000';
                UPDATE ""OpeningBalanceOverrides"" SET ""UserId"" = u.""Id"" FROM (SELECT ""Id"" FROM ""Users"" LIMIT 1) u WHERE ""OpeningBalanceOverrides"".""UserId"" = '00000000-0000-0000-0000-000000000000';
                UPDATE ""MandatoryPayments""     SET ""UserId"" = u.""Id"" FROM (SELECT ""Id"" FROM ""Users"" LIMIT 1) u WHERE ""MandatoryPayments"".""UserId"" = '00000000-0000-0000-0000-000000000000';
                UPDATE ""Counterparties""        SET ""UserId"" = u.""Id"" FROM (SELECT ""Id"" FROM ""Users"" LIMIT 1) u WHERE ""Counterparties"".""UserId"" = '00000000-0000-0000-0000-000000000000';
                UPDATE ""Categories""            SET ""UserId"" = u.""Id"" FROM (SELECT ""Id"" FROM ""Users"" LIMIT 1) u WHERE ""Categories"".""UserId"" = '00000000-0000-0000-0000-000000000000';
                UPDATE ""BudgetEntries""         SET ""UserId"" = u.""Id"" FROM (SELECT ""Id"" FROM ""Users"" LIMIT 1) u WHERE ""BudgetEntries"".""UserId"" = '00000000-0000-0000-0000-000000000000';
                UPDATE ""Accounts""              SET ""UserId"" = u.""Id"" FROM (SELECT ""Id"" FROM ""Users"" LIMIT 1) u WHERE ""Accounts"".""UserId"" = '00000000-0000-0000-0000-000000000000';
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningBalanceOverrides_UserId_Year_Month",
                table: "OpeningBalanceOverrides",
                columns: new[] { "UserId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MandatoryPayments_UserId",
                table: "MandatoryPayments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Counterparties_UserId",
                table: "Counterparties",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_UserId",
                table: "Categories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetEntries_CategoryId",
                table: "BudgetEntries",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetEntries_UserId_CategoryId_Year_Month",
                table: "BudgetEntries",
                columns: new[] { "UserId", "CategoryId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Accounts_Users_UserId",
                table: "Accounts",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BudgetEntries_Users_UserId",
                table: "BudgetEntries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UserId",
                table: "Categories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Counterparties_Users_UserId",
                table: "Counterparties",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MandatoryPayments_Users_UserId",
                table: "MandatoryPayments",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OpeningBalanceOverrides_Users_UserId",
                table: "OpeningBalanceOverrides",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Accounts_Users_UserId",
                table: "Accounts");

            migrationBuilder.DropForeignKey(
                name: "FK_BudgetEntries_Users_UserId",
                table: "BudgetEntries");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_Counterparties_Users_UserId",
                table: "Counterparties");

            migrationBuilder.DropForeignKey(
                name: "FK_MandatoryPayments_Users_UserId",
                table: "MandatoryPayments");

            migrationBuilder.DropForeignKey(
                name: "FK_OpeningBalanceOverrides_Users_UserId",
                table: "OpeningBalanceOverrides");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Users_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_UserId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_OpeningBalanceOverrides_UserId_Year_Month",
                table: "OpeningBalanceOverrides");

            migrationBuilder.DropIndex(
                name: "IX_MandatoryPayments_UserId",
                table: "MandatoryPayments");

            migrationBuilder.DropIndex(
                name: "IX_Counterparties_UserId",
                table: "Counterparties");

            migrationBuilder.DropIndex(
                name: "IX_Categories_UserId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_BudgetEntries_CategoryId",
                table: "BudgetEntries");

            migrationBuilder.DropIndex(
                name: "IX_BudgetEntries_UserId_CategoryId_Year_Month",
                table: "BudgetEntries");

            migrationBuilder.DropIndex(
                name: "IX_Accounts_UserId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "OpeningBalanceOverrides");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "MandatoryPayments");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Counterparties");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BudgetEntries");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Accounts");

            migrationBuilder.CreateIndex(
                name: "IX_OpeningBalanceOverrides_Year_Month",
                table: "OpeningBalanceOverrides",
                columns: new[] { "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BudgetEntries_CategoryId_Year_Month",
                table: "BudgetEntries",
                columns: new[] { "CategoryId", "Year", "Month" },
                unique: true);
        }
    }
}
