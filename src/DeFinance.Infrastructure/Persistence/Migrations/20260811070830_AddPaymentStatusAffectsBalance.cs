using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentStatusAffectsBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AffectsBalance",
                table: "PaymentStatuses",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            // Rejected transactions do not affect balance
            migrationBuilder.Sql(
                "UPDATE \"PaymentStatuses\" SET \"AffectsBalance\" = false WHERE \"Name\" = 'Rejected'");

            // Recompute existing account balances: subtract contributions from rejected transactions
            migrationBuilder.Sql(@"
                UPDATE ""Accounts"" a
                SET ""Balance"" = a.""Balance"" - COALESCE((
                    SELECT SUM(
                        CASE
                            WHEN c.""Type"" IN (1, 4) THEN t.""Sum""
                            WHEN c.""Type"" IN (2, 5) THEN -t.""Sum""
                            ELSE 0
                        END
                    )
                    FROM ""Transactions"" t
                    JOIN ""Categories"" c ON t.""CategoryId"" = c.""Id""
                    JOIN ""PaymentStatuses"" ps ON t.""PaymentStatusId"" = ps.""Id""
                    WHERE t.""AccountId"" = a.""Id""
                    AND ps.""Name"" = 'Rejected'
                ), 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Restore account balances: add back rejected transaction contributions
            migrationBuilder.Sql(@"
                UPDATE ""Accounts"" a
                SET ""Balance"" = a.""Balance"" + COALESCE((
                    SELECT SUM(
                        CASE
                            WHEN c.""Type"" IN (1, 4) THEN t.""Sum""
                            WHEN c.""Type"" IN (2, 5) THEN -t.""Sum""
                            ELSE 0
                        END
                    )
                    FROM ""Transactions"" t
                    JOIN ""Categories"" c ON t.""CategoryId"" = c.""Id""
                    JOIN ""PaymentStatuses"" ps ON t.""PaymentStatusId"" = ps.""Id""
                    WHERE t.""AccountId"" = a.""Id""
                    AND ps.""Name"" = 'Rejected'
                ), 0)");

            migrationBuilder.DropColumn(
                name: "AffectsBalance",
                table: "PaymentStatuses");
        }
    }
}
