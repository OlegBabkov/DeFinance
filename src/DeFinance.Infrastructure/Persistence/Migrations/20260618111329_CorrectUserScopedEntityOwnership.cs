using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DeFinance.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CorrectUserScopedEntityOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Re-assign all user-scoped data to the admin user.
            // The previous migration's backfill used LIMIT 1 without ORDER BY, which is
            // non-deterministic in PostgreSQL.  If a non-admin user happened to be the
            // first physical row returned, all historical data would have been stamped
            // with that user's Id instead of admin's Id.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE admin_id UUID;
                BEGIN
                    SELECT ""Id"" INTO admin_id FROM ""Users"" WHERE ""Username"" = 'admin' LIMIT 1;
                    IF admin_id IS NOT NULL THEN
                        -- BudgetEntries has a unique index on (UserId, CategoryId, Year, Month).
                        -- Remove any admin rows that would collide with rows being moved from other users.
                        DELETE FROM ""BudgetEntries""
                        WHERE ""UserId"" = admin_id
                          AND (""CategoryId"", ""Year"", ""Month"") IN (
                              SELECT ""CategoryId"", ""Year"", ""Month""
                              FROM ""BudgetEntries""
                              WHERE ""UserId"" != admin_id
                          );

                        -- OpeningBalanceOverrides has a unique index on (UserId, Year, Month).
                        DELETE FROM ""OpeningBalanceOverrides""
                        WHERE ""UserId"" = admin_id
                          AND (""Year"", ""Month"") IN (
                              SELECT ""Year"", ""Month""
                              FROM ""OpeningBalanceOverrides""
                              WHERE ""UserId"" != admin_id
                          );

                        UPDATE ""Transactions""            SET ""UserId"" = admin_id WHERE ""UserId"" != admin_id;
                        UPDATE ""Accounts""                SET ""UserId"" = admin_id WHERE ""UserId"" != admin_id;
                        UPDATE ""Categories""              SET ""UserId"" = admin_id WHERE ""UserId"" != admin_id;
                        UPDATE ""Counterparties""          SET ""UserId"" = admin_id WHERE ""UserId"" != admin_id;
                        UPDATE ""MandatoryPayments""       SET ""UserId"" = admin_id WHERE ""UserId"" != admin_id;
                        UPDATE ""BudgetEntries""           SET ""UserId"" = admin_id WHERE ""UserId"" != admin_id;
                        UPDATE ""OpeningBalanceOverrides"" SET ""UserId"" = admin_id WHERE ""UserId"" != admin_id;
                    END IF;
                END $$;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Data corrections are not reversible.
        }
    }
}
