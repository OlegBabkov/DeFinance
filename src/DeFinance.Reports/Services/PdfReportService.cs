using DeFinance.Domain.Entities;
using DeFinance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DeFinance.Reports.Services;

public class PdfReportService(DeFinanceDbContext db) : IPdfReportService
{
    // ──────────────────────────────────────────────────────────────
    // 1. Cash Flow Statement
    // ──────────────────────────────────────────────────────────────
    public async Task<byte[]> GenerateCashFlowAsync(Guid userId, DateTime from, DateTime to, Guid? accountId)
    {
        var query = db.Transactions
            .Include(t => t.Category)
            .Include(t => t.Account).ThenInclude(a => a!.Currency)
            .Where(t => t.UserId == userId && t.DateTime >= from && t.DateTime <= to);

        if (accountId.HasValue)
            query = query.Where(t => t.AccountId == accountId.Value);

        var transactions = await query.OrderBy(t => t.DateTime).ToListAsync();

        var rows = transactions
            .GroupBy(t => t.DateTime.Date)
            .Select(g => new CashFlowRow(
                g.Key,
                g.Where(t => t.Category?.Type is CategoryType.Income or CategoryType.TransferIn)
                  .Sum(t => t.AmountInCurrency),
                g.Where(t => t.Category?.Type is CategoryType.Expense or CategoryType.TransferOut)
                  .Sum(t => t.AmountInCurrency)))
            .OrderBy(r => r.Date)
            .ToList();

        var totalIncome  = rows.Sum(r => r.Income);
        var totalExpense = rows.Sum(r => r.Expense);
        var netFlow      = totalIncome - totalExpense;

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                SetupPage(page);

                page.Header().Element(c => BuildHeader(c,
                    "Cash Flow Statement",
                    $"{from:MMM dd, yyyy} – {to:MMM dd, yyyy}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Element(c => BuildSummaryRow(c, new[]
                    {
                        ("Total Income",   $"{totalIncome:N2}",  Colors.Green.Darken2),
                        ("Total Expenses", $"{totalExpense:N2}", Colors.Red.Darken2),
                        ("Net Cash Flow",  $"{netFlow:N2}",      netFlow >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2),
                    }));

                    col.Item().PaddingTop(16).Element(c => BuildCashFlowTable(c, rows));
                });

                page.Footer().Element(BuildFooter);
            });
        }).GeneratePdf();
    }

    // ──────────────────────────────────────────────────────────────
    // 2. Expense Category Breakdown
    // ──────────────────────────────────────────────────────────────
    public async Task<byte[]> GenerateExpenseCategoryBreakdownAsync(Guid userId, DateTime from, DateTime to, Guid[] categoryIds)
    {
        var query = db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId
                     && t.DateTime >= from && t.DateTime <= to
                     && (t.Category!.Type == CategoryType.Expense || t.Category.Type == CategoryType.TransferOut));

        if (categoryIds.Length > 0)
            query = query.Where(t => categoryIds.Contains(t.CategoryId) ||
                                     (t.Category!.ParentId.HasValue && categoryIds.Contains(t.Category.ParentId.Value)));

        var transactions = await query.ToListAsync();
        var total = transactions.Sum(t => t.AmountInCurrency);

        var rows = transactions
            .GroupBy(t => t.Category?.Name ?? "Unknown")
            .Select(g => new CategoryRow(
                g.Key,
                g.Count(),
                g.Sum(t => t.AmountInCurrency),
                total > 0 ? (double)(g.Sum(t => t.AmountInCurrency) / total) * 100.0 : 0.0))
            .OrderByDescending(r => r.Amount)
            .ToList();

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                SetupPage(page);

                page.Header().Element(c => BuildHeader(c,
                    "Expense Category Breakdown",
                    $"{from:MMM dd, yyyy} – {to:MMM dd, yyyy}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Element(c => BuildSummaryRow(c, new[]
                    {
                        ("Total Expenses",  $"{total:N2}",           Colors.Red.Darken2),
                        ("Categories",      $"{rows.Count}",          Colors.Blue.Darken2),
                        ("Transactions",    $"{transactions.Count}",  Colors.Grey.Darken2),
                    }));

                    col.Item().PaddingTop(16).Element(c => BuildCategoryTable(c, rows));
                });

                page.Footer().Element(BuildFooter);
            });
        }).GeneratePdf();
    }

    // ──────────────────────────────────────────────────────────────
    // 3. Account Balance Summary
    // ──────────────────────────────────────────────────────────────
    public async Task<byte[]> GenerateAccountBalanceSummaryAsync(Guid userId, DateTime from, DateTime to)
    {
        var accounts = await db.Accounts
            .Include(a => a.Currency)
            .Where(a => a.UserId == userId && a.IsActive)
            .OrderBy(a => a.SortOrder)
            .ToListAsync();

        var transactions = await db.Transactions
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.DateTime >= from && t.DateTime <= to)
            .ToListAsync();

        var rows = accounts.Select(acc =>
        {
            var txForAccount = transactions.Where(t => t.AccountId == acc.Id).ToList();
            var change = txForAccount.Sum(t =>
                t.Category?.Type is CategoryType.Income or CategoryType.TransferIn  ?  t.Sum :
                t.Category?.Type is CategoryType.Expense or CategoryType.TransferOut ? -t.Sum : 0m);
            var closingBalance = acc.Balance;
            var openingBalance = closingBalance - change;
            return new AccountBalanceRow(acc.Name, acc.Currency?.Code ?? "?", openingBalance, change, closingBalance);
        }).ToList();

        var totalOpen   = rows.Sum(r => r.OpeningBalance);
        var totalClose  = rows.Sum(r => r.ClosingBalance);
        var totalChange = totalClose - totalOpen;

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                SetupPage(page);

                page.Header().Element(c => BuildHeader(c,
                    "Account Balance Summary",
                    $"{from:MMM dd, yyyy} – {to:MMM dd, yyyy}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Element(c => BuildSummaryRow(c, new[]
                    {
                        ("Opening Total", $"{totalOpen:N2}",   Colors.Blue.Darken2),
                        ("Closing Total", $"{totalClose:N2}",  Colors.Blue.Darken3),
                        ("Net Change",    $"{totalChange:N2}", totalChange >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2),
                    }));

                    col.Item().PaddingTop(16).Element(c => BuildAccountBalanceTable(c, rows));
                });

                page.Footer().Element(BuildFooter);
            });
        }).GeneratePdf();
    }

    // ──────────────────────────────────────────────────────────────
    // Shared layout helpers
    // ──────────────────────────────────────────────────────────────

    private static void SetupPage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.MarginHorizontal(1.5f, Unit.Centimetre);
        page.MarginVertical(1.5f, Unit.Centimetre);
        page.DefaultTextStyle(t => t.FontSize(9).FontFamily(Fonts.Calibri));
    }

    private static void BuildHeader(IContainer c, string title, string subtitle)
    {
        c.Column(col =>
        {
            col.Item()
               .BorderBottom(2).BorderColor(Colors.Indigo.Darken2)
               .PaddingBottom(8)
               .Row(row =>
               {
                   row.RelativeItem().Column(inner =>
                   {
                       inner.Item().Text("DeFinance")
                            .FontSize(20).Bold().FontColor(Colors.Indigo.Darken2);
                       inner.Item().Text("Personal Finance Report")
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                   });
                   row.ConstantItem(200).Column(inner =>
                   {
                       inner.Item().AlignRight().Text(title)
                            .FontSize(13).Bold().FontColor(Colors.Grey.Darken3);
                       inner.Item().AlignRight().Text(subtitle)
                            .FontSize(9).FontColor(Colors.Grey.Darken1);
                       inner.Item().AlignRight().Text($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC")
                            .FontSize(8).FontColor(Colors.Grey.Medium);
                   });
               });
        });
    }

    private static void BuildSummaryRow(IContainer c, (string Label, string Value, Color Color)[] items)
    {
        c.Row(row =>
        {
            foreach (var item in items)
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2)
                   .Background(Colors.Grey.Lighten5)
                   .Padding(10)
                   .Column(inner =>
                   {
                       inner.Item().Text(item.Label).FontSize(8).FontColor(Colors.Grey.Darken1);
                       inner.Item().Text(item.Value).FontSize(15).Bold().FontColor(item.Color);
                   });
            }
        });
    }

    private static void BuildFooter(IContainer c)
    {
        c.Row(row =>
        {
            row.RelativeItem().Text("DeFinance — Personal Finance Manager")
               .FontSize(8).FontColor(Colors.Grey.Medium);
            row.ConstantItem(80).AlignRight().Text(text =>
            {
                text.Span("Page ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.CurrentPageNumber().FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span(" / ").FontSize(8).FontColor(Colors.Grey.Medium);
                text.TotalPages().FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    // ── Cash Flow table ──────────────────────────────────────────
    private static void BuildCashFlowTable(IContainer c, List<CashFlowRow> rows)
    {
        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(90);
                cols.RelativeColumn();
                cols.RelativeColumn();
                cols.RelativeColumn();
            });

            BuildTableHeader(table, "Date", "Income", "Expenses", "Net Flow");

            var alt = false;
            foreach (var row in rows)
            {
                var bg  = alt ? Colors.Grey.Lighten5 : Colors.White;
                alt = !alt;
                var net = row.Income - row.Expense;

                table.Cell().Background(bg).Padding(5).Text(row.Date.ToString("MMM dd, yyyy")).FontSize(8);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.Income:N2}").FontSize(8).FontColor(Colors.Green.Darken2);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.Expense:N2}").FontSize(8).FontColor(Colors.Red.Darken2);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{net:N2}").FontSize(8)
                     .FontColor(net >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
            }

            if (rows.Count == 0)
            {
                table.Cell().ColumnSpan(4).Padding(12).AlignCenter()
                     .Text("No transactions found for the selected period.").FontSize(9).FontColor(Colors.Grey.Medium);
            }
        });
    }

    // ── Category Breakdown table ─────────────────────────────────
    private static void BuildCategoryTable(IContainer c, List<CategoryRow> rows)
    {
        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(24);
                cols.RelativeColumn(3);
                cols.ConstantColumn(60);
                cols.RelativeColumn(2);
                cols.RelativeColumn(2);
                cols.RelativeColumn(3);
            });

            BuildTableHeader(table, "#", "Category", "Txns", "Amount", "%", "Proportion");

            var i   = 1;
            var alt = false;
            foreach (var row in rows)
            {
                var bg  = alt ? Colors.Grey.Lighten5 : Colors.White;
                alt = !alt;
                var pct = (float)Math.Min(row.Percent / 100.0, 1.0);

                table.Cell().Background(bg).Padding(5).AlignCenter().Text($"{i++}").FontSize(8);
                table.Cell().Background(bg).Padding(5).Text(row.Category).FontSize(8);
                table.Cell().Background(bg).Padding(5).AlignCenter().Text($"{row.Count}").FontSize(8);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.Amount:N2}").FontSize(8).FontColor(Colors.Red.Darken2);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.Percent:F1}%").FontSize(8);
                table.Cell().Background(bg).Padding(4).Element(barCell =>
                {
                    barCell.Row(r =>
                    {
                        r.RelativeItem(Math.Max(pct, 0.01f)).Height(10).Background(Colors.Indigo.Lighten2);
                        r.RelativeItem(Math.Max(1f - pct, 0f)).Height(10);
                    });
                });
            }

            if (rows.Count == 0)
            {
                table.Cell().ColumnSpan(6).Padding(12).AlignCenter()
                     .Text("No expense transactions found for the selected period.").FontSize(9).FontColor(Colors.Grey.Medium);
            }
        });
    }

    // ── Account Balance table ────────────────────────────────────
    private static void BuildAccountBalanceTable(IContainer c, List<AccountBalanceRow> rows)
    {
        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn(3);
                cols.ConstantColumn(45);
                cols.RelativeColumn(2);
                cols.RelativeColumn(2);
                cols.RelativeColumn(2);
            });

            BuildTableHeader(table, "Account", "Currency", "Opening Balance", "Change", "Closing Balance");

            var alt = false;
            foreach (var row in rows)
            {
                var bg = alt ? Colors.Grey.Lighten5 : Colors.White;
                alt = !alt;

                table.Cell().Background(bg).Padding(5).Text(row.Account).FontSize(8);
                table.Cell().Background(bg).Padding(5).AlignCenter().Text(row.Currency).FontSize(8).FontColor(Colors.Grey.Darken1);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.OpeningBalance:N2}").FontSize(8);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.Change:+#,##0.00;-#,##0.00;0.00}").FontSize(8)
                     .FontColor(row.Change >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.ClosingBalance:N2}").FontSize(8).Bold();
            }

            if (rows.Count == 0)
            {
                table.Cell().ColumnSpan(5).Padding(12).AlignCenter()
                     .Text("No active accounts found.").FontSize(9).FontColor(Colors.Grey.Medium);
            }
        });
    }

    private static void BuildTableHeader(TableDescriptor table, params string[] headers)
    {
        table.Header(h =>
        {
            foreach (var hdr in headers)
            {
                h.Cell().Background(Colors.Indigo.Darken2)
                 .PaddingHorizontal(5).PaddingVertical(6)
                 .Text(hdr).FontSize(8).Bold().FontColor(Colors.White);
            }
        });
    }

    // ──────────────────────────────────────────────────────────────
    // 4. Counterparty Spending
    // ──────────────────────────────────────────────────────────────
    public async Task<byte[]> GenerateCounterpartySpendingAsync(Guid userId, DateTime from, DateTime to, Guid[] counterpartyIds)
    {
        var query = db.Transactions
            .Include(t => t.Counterparty)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId
                     && t.DateTime >= from && t.DateTime <= to
                     && t.CounterpartyId != null);

        if (counterpartyIds.Length > 0)
            query = query.Where(t => t.CounterpartyId.HasValue && counterpartyIds.Contains(t.CounterpartyId.Value));

        var transactions = await query.ToListAsync();

        var expenseTxns = transactions.Where(t => t.Category?.Type is CategoryType.Expense or CategoryType.TransferOut).ToList();
        var incomeTxns  = transactions.Where(t => t.Category?.Type is CategoryType.Income  or CategoryType.TransferIn).ToList();

        var totalExpense = expenseTxns.Sum(t => t.AmountInCurrency);
        var totalIncome  = incomeTxns.Sum(t => t.AmountInCurrency);
        var net          = totalIncome - totalExpense;

        CounterpartyRow[] BuildRows(List<Transaction> txns, decimal sectionTotal) =>
            txns.GroupBy(t => new { t.CounterpartyId, Name = t.Counterparty?.Name ?? "Unknown", Type = t.Counterparty?.Type.ToString() ?? "Other" })
                .Select(g => new CounterpartyRow(
                    g.Key.Name,
                    g.Key.Type,
                    g.Count(),
                    g.Sum(t => t.AmountInCurrency),
                    sectionTotal > 0 ? (double)(g.Sum(t => t.AmountInCurrency) / sectionTotal) * 100.0 : 0.0))
                .OrderByDescending(r => r.Amount)
                .ToArray();

        var expenseRows = BuildRows(expenseTxns, totalExpense);
        var incomeRows  = BuildRows(incomeTxns, totalIncome);

        return Document.Create(doc =>
        {
            doc.Page(page =>
            {
                SetupPage(page);

                page.Header().Element(c => BuildHeader(c,
                    "Counterparty Spending",
                    $"{from:MMM dd, yyyy} – {to:MMM dd, yyyy}"));

                page.Content().PaddingVertical(10).Column(col =>
                {
                    col.Item().Element(c => BuildSummaryRow(c, new[]
                    {
                        ("Total Income",   $"{totalIncome:N2}",  Colors.Green.Darken2),
                        ("Total Expenses", $"{totalExpense:N2}", Colors.Red.Darken2),
                        ("Net",            $"{net:N2}",          net >= 0 ? Colors.Green.Darken2 : Colors.Red.Darken2),
                    }));

                    if (expenseRows.Length > 0)
                    {
                        col.Item().PaddingTop(16).PaddingBottom(4)
                           .Text("Spending").FontSize(11).Bold().FontColor(Colors.Red.Darken2);
                        col.Item().Element(c => BuildCounterpartyTable(c, expenseRows, isIncome: false));
                    }

                    if (incomeRows.Length > 0)
                    {
                        col.Item().PaddingTop(16).PaddingBottom(4)
                           .Text("Income").FontSize(11).Bold().FontColor(Colors.Green.Darken2);
                        col.Item().Element(c => BuildCounterpartyTable(c, incomeRows, isIncome: true));
                    }

                    if (expenseRows.Length == 0 && incomeRows.Length == 0)
                    {
                        col.Item().PaddingTop(16).AlignCenter()
                           .Text("No transactions with counterparties found for the selected period.")
                           .FontSize(9).FontColor(Colors.Grey.Medium);
                    }
                });

                page.Footer().Element(BuildFooter);
            });
        }).GeneratePdf();
    }

    // ── Counterparty Spending table ──────────────────────────────
    private static void BuildCounterpartyTable(IContainer c, CounterpartyRow[] rows, bool isIncome)
    {
        var amountColor = isIncome ? Colors.Green.Darken2 : Colors.Red.Darken2;
        var barColor    = isIncome ? Colors.Green.Lighten2 : Colors.Indigo.Lighten2;

        c.Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.ConstantColumn(24);
                cols.RelativeColumn(3);
                cols.ConstantColumn(60);
                cols.ConstantColumn(50);
                cols.RelativeColumn(2);
                cols.RelativeColumn(2);
                cols.RelativeColumn(3);
            });

            BuildTableHeader(table, "#", "Counterparty", "Type", "Txns", "Amount", "%", "Proportion");

            var i   = 1;
            var alt = false;
            foreach (var row in rows)
            {
                var bg  = alt ? Colors.Grey.Lighten5 : Colors.White;
                alt = !alt;
                var pct = (float)Math.Min(row.Percent / 100.0, 1.0);

                table.Cell().Background(bg).Padding(5).AlignCenter().Text($"{i++}").FontSize(8);
                table.Cell().Background(bg).Padding(5).Text(row.Name).FontSize(8);
                table.Cell().Background(bg).Padding(5).AlignCenter().Text(row.Type).FontSize(8).FontColor(Colors.Grey.Darken1);
                table.Cell().Background(bg).Padding(5).AlignCenter().Text($"{row.Count}").FontSize(8);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.Amount:N2}").FontSize(8).FontColor(amountColor);
                table.Cell().Background(bg).Padding(5).AlignRight().Text($"{row.Percent:F1}%").FontSize(8);
                table.Cell().Background(bg).Padding(4).Element(barCell =>
                {
                    barCell.Row(r =>
                    {
                        r.RelativeItem(Math.Max(pct, 0.01f)).Height(10).Background(barColor);
                        r.RelativeItem(Math.Max(1f - pct, 0f)).Height(10);
                    });
                });
            }
        });
    }

    // ──────────────────────────────────────────────────────────────
    // Data records
    // ──────────────────────────────────────────────────────────────
    private record CashFlowRow(DateTime Date, decimal Income, decimal Expense);
    private record CategoryRow(string Category, int Count, decimal Amount, double Percent);
    private record CounterpartyRow(string Name, string Type, int Count, decimal Amount, double Percent);
    private record AccountBalanceRow(string Account, string Currency, decimal OpeningBalance, decimal Change, decimal ClosingBalance);
}
