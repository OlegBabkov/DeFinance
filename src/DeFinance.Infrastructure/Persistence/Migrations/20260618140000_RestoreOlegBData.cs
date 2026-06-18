using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RestoreOlegBData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The previous corrective migration incorrectly moved OlegB's historical data
            // to the seeded 'admin' account. OlegB is a separately registered user (username='OlegB'),
            // not the seeded admin. This migration moves everything back to OlegB.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE
                    admin_id UUID;
                    olegb_id UUID;
                BEGIN
                    SELECT ""Id"" INTO admin_id FROM ""Users"" WHERE ""Username"" = 'admin' LIMIT 1;
                    SELECT ""Id"" INTO olegb_id FROM ""Users"" WHERE ""Username"" = 'OlegB' LIMIT 1;

                    IF admin_id IS NOT NULL AND olegb_id IS NOT NULL THEN
                        UPDATE ""Transactions""            SET ""UserId"" = olegb_id WHERE ""UserId"" = admin_id;
                        UPDATE ""Accounts""                SET ""UserId"" = olegb_id WHERE ""UserId"" = admin_id;
                        UPDATE ""Categories""              SET ""UserId"" = olegb_id WHERE ""UserId"" = admin_id;
                        UPDATE ""Counterparties""          SET ""UserId"" = olegb_id WHERE ""UserId"" = admin_id;
                        UPDATE ""MandatoryPayments""       SET ""UserId"" = olegb_id WHERE ""UserId"" = admin_id;
                        UPDATE ""BudgetEntries""           SET ""UserId"" = olegb_id WHERE ""UserId"" = admin_id;
                        UPDATE ""OpeningBalanceOverrides"" SET ""UserId"" = olegb_id WHERE ""UserId"" = admin_id;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible — do not re-run CorrectUserScopedEntityOwnership.
        }
    }
}
