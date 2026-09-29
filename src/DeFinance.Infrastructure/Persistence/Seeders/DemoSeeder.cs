using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

/// <summary>
/// Seeds a demo user with 6 months of realistic transactions and plan-fact budget data.
/// Runs only if the demo user does not yet exist; safe to call on every startup.
/// </summary>
public static class DemoSeeder
{
    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(u => u.Username == "demo", cancellationToken))
            return;

        // ── 1. User ───────────────────────────────────────────────────────────
        var user = User.Create(
            username: "demo",
            password: BCrypt.Net.BCrypt.HashPassword("demo123"),
            email: "demo@definance.app",
            phoneNumber: null);
        await context.Users.AddAsync(user, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        var uid = user.Id;

        // ── 2. Reference data (currencies + payment statuses are global) ──────
        var eurId = await context.Currencies
            .Where(c => c.Code == "EUR").Select(c => c.Id)
            .FirstOrDefaultAsync(cancellationToken);
        if (eurId == default) return;

        var statuses = await context.PaymentStatuses
            .ToDictionaryAsync(s => s.Name, s => s, cancellationToken);
        if (!statuses.TryGetValue("Paid",   out var paid))   return;
        if (!statuses.TryGetValue("Booked", out var booked)) booked = paid;

        // ── 3. Categories ─────────────────────────────────────────────────────
        static Category Cat(string name, CategoryType type, string color, string icon, Guid userId)
            => Category.Create(name, type, color, icon, null, null, userId);

        var cSalary      = Cat("Salary",             CategoryType.Income,      "#22C55E", "💰", uid);
        var cFreelance   = Cat("Freelance",          CategoryType.Income,      "#15803D", "💻", uid);
        var cRent        = Cat("Apartment Rent",     CategoryType.Expense,     "#EF4444", "🏠", uid);
        var cUtilities   = Cat("Utilities",          CategoryType.Expense,     "#F87171", "💡", uid);
        var cInternet    = Cat("Internet & Phone",   CategoryType.Expense,     "#FCA5A5", "📱", uid);
        var cGroceries   = Cat("Groceries",          CategoryType.Expense,     "#F97316", "🛒", uid);
        var cRestaurant  = Cat("Restaurants",        CategoryType.Expense,     "#FB923C", "🍽️", uid);
        var cCoffee      = Cat("Coffee & Cafes",     CategoryType.Expense,     "#FDBA74", "☕", uid);
        var cGas         = Cat("Gasoline",           CategoryType.Expense,     "#EAB308", "⛽", uid);
        var cTransport   = Cat("Public Transport",   CategoryType.Expense,     "#FACC15", "🚌", uid);
        var cGym         = Cat("Gym & Fitness",      CategoryType.Expense,     "#FBCFE8", "🏋️", uid);
        var cStreaming    = Cat("Streaming Services", CategoryType.Expense,     "#1D4ED8", "📺", uid);
        var cClothing    = Cat("Clothing",           CategoryType.Expense,     "#8B5CF6", "👗", uid);
        var cPharmacy    = Cat("Pharmacy",           CategoryType.Expense,     "#F472B6", "💊", uid);
        var cDoctor      = Cat("Doctor Visits",      CategoryType.Expense,     "#EC4899", "👨‍⚕️", uid);
        var cFlights     = Cat("Flights",            CategoryType.Expense,     "#06B6D4", "✈️", uid);
        var cHotels      = Cat("Hotels",             CategoryType.Expense,     "#0891B2", "🏨", uid);
        var cElectronics = Cat("Electronics",        CategoryType.Expense,     "#7C3AED", "📱", uid);
        var cTaxi        = Cat("Taxi & Rideshare",   CategoryType.Expense,     "#FDE047", "🚕", uid);
        var cTrIn        = Cat("Transfer In",        CategoryType.TransferIn,  "#64748B", "🔄", uid);
        var cTrOut       = Cat("Transfer Out",       CategoryType.TransferOut, "#64748B", "🔄", uid);

        cSalary.SetImportant(true);
        cRent.SetImportant(true);
        cGroceries.SetImportant(true);
        cUtilities.SetImportant(true);

        var allCats = new[]
        {
            cSalary, cFreelance, cRent, cUtilities, cInternet, cGroceries, cRestaurant,
            cCoffee, cGas, cTransport, cGym, cStreaming, cClothing, cPharmacy,
            cDoctor, cFlights, cHotels, cElectronics, cTaxi, cTrIn, cTrOut,
        };
        await context.Categories.AddRangeAsync(allCats, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // ── 4. Accounts ───────────────────────────────────────────────────────
        var aMain    = Account.Create("Main Account", AccountType.Checking, 0m,   eurId, uid);
        var aSavings = Account.Create("Savings Fund", AccountType.Savings,  0m,   eurId, uid);
        var aCard    = Account.Create("Credit Card",  AccountType.Credit,   0m,   eurId, uid);
        var aCash    = Account.Create("Cash",         AccountType.Cash,     150m, eurId, uid);
        await context.Accounts.AddRangeAsync(new[] { aMain, aSavings, aCard, aCash }, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // ── 5. Counterparties ─────────────────────────────────────────────────
        static Counterparty Cp(string name, CounterpartyType type, Guid userId)
            => Counterparty.Create(name, type, null, userId);

        var cpRewe     = Cp("Rewe",       CounterpartyType.Company, uid);
        var cpLidl     = Cp("Lidl",       CounterpartyType.Company, uid);
        var cpAldi     = Cp("Aldi",       CounterpartyType.Company, uid);
        var cpAmazon   = Cp("Amazon",     CounterpartyType.Company, uid);
        var cpAral     = Cp("Aral",       CounterpartyType.Company, uid);
        var cpNetflix  = Cp("Netflix",    CounterpartyType.Company, uid);
        var cpSpotify  = Cp("Spotify",    CounterpartyType.Company, uid);
        var cpLufth    = Cp("Lufthansa",  CounterpartyType.Company, uid);
        await context.Counterparties.AddRangeAsync(
            new[] { cpRewe, cpLidl, cpAldi, cpAmazon, cpAral, cpNetflix, cpSpotify, cpLufth },
            cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // ── 6. Transactions ───────────────────────────────────────────────────
        var rng          = new Random(2026);
        var transactions = new List<Transaction>();

        void Tx(Account acct, Category cat, decimal sum, PaymentStatus status,
                int y, int mo, int d, string? notes = null, Counterparty? cp = null)
        {
            var dt = DateTime.SpecifyKind(
                new DateTime(y, mo, d, rng.Next(8, 20), rng.Next(0, 60), 0), DateTimeKind.Utc);
            transactions.Add(Transaction.Create(dt, sum, 1m, eurId,
                acct.Id, cat.Id, cp?.Id, status.Id, uid, notes));
            if (!status.AffectsBalance) return;
            var delta = cat.Type switch
            {
                CategoryType.Income      =>  sum,
                CategoryType.Expense     => -sum,
                CategoryType.TransferIn  =>  sum,
                CategoryType.TransferOut => -sum,
                _                        =>  0m,
            };
            acct.AdjustBalance(delta);
        }

        // ─── April 2026 ───────────────────────────────────────────────────────
        Tx(aMain, cSalary,     3800m, booked, 2026, 4,  1,  "April salary");
        Tx(aMain, cRent,        950m, paid,   2026, 4,  1,  "Monthly rent");
        Tx(aMain, cUtilities,    62m, paid,   2026, 4,  4,  "Electricity + water");
        Tx(aMain, cInternet,     35m, paid,   2026, 4,  5,  "Internet + mobile");
        Tx(aMain, cGym,          35m, paid,   2026, 4,  5,  "Monthly membership");
        Tx(aMain, cStreaming,    13m, paid,   2026, 4,  6,  "Netflix",         cpNetflix);
        Tx(aMain, cStreaming,     5m, paid,   2026, 4,  6,  "Spotify",         cpSpotify);
        Tx(aMain, cGroceries,  138m, paid,   2026, 4,  8,  null,              cpRewe);
        Tx(aCard, cRestaurant,   47m, paid,   2026, 4, 10,  "Lunch with colleagues");
        Tx(aCard, cCoffee,       12m, paid,   2026, 4, 11,  null);
        Tx(aMain, cGas,          75m, paid,   2026, 4, 13,  null,              cpAral);
        Tx(aMain, cGroceries,    92m, paid,   2026, 4, 15,  null,              cpLidl);
        Tx(aCard, cCoffee,        8m, paid,   2026, 4, 17,  null);
        Tx(aCard, cRestaurant,   68m, paid,   2026, 4, 19,  "Birthday dinner");
        Tx(aMain, cGroceries,  105m, paid,   2026, 4, 22,  "Weekend shop",    cpAldi);
        Tx(aCard, cClothing,   195m, paid,   2026, 4, 23,  "Spring jacket");
        Tx(aCard, cDoctor,       55m, paid,   2026, 4, 25,  "Annual check-up");
        Tx(aCash, cCoffee,       11m, paid,   2026, 4, 26,  null);
        Tx(aMain, cTransport,    28m, paid,   2026, 4, 28,  "Monthly pass");
        Tx(aMain, cGroceries,    79m, paid,   2026, 4, 30,  null,              cpRewe);

        // ─── May 2026 ─────────────────────────────────────────────────────────
        Tx(aMain, cSalary,     3800m, booked, 2026, 5,  2,  "May salary");
        Tx(aMain, cRent,        950m, paid,   2026, 5,  1,  "Monthly rent");
        Tx(aMain, cUtilities,    58m, paid,   2026, 5,  5,  "Electricity + water");
        Tx(aMain, cInternet,     35m, paid,   2026, 5,  5,  "Internet + mobile");
        Tx(aMain, cGym,          35m, paid,   2026, 5,  6,  "Monthly membership");
        Tx(aMain, cStreaming,    13m, paid,   2026, 5,  6,  "Netflix",         cpNetflix);
        Tx(aMain, cStreaming,     5m, paid,   2026, 5,  6,  "Spotify",         cpSpotify);
        Tx(aMain, cGroceries,  121m, paid,   2026, 5,  7,  null,              cpRewe);
        Tx(aCard, cCoffee,       14m, paid,   2026, 5,  9,  null);
        Tx(aMain, cGas,          82m, paid,   2026, 5, 10,  null,              cpAral);
        Tx(aCard, cRestaurant,   39m, paid,   2026, 5, 12,  "Lunch");
        Tx(aMain, cGroceries,  147m, paid,   2026, 5, 14,  "Big shop",        cpAldi);
        Tx(aCard, cCoffee,        9m, paid,   2026, 5, 16,  null);
        Tx(aCard, cRestaurant,   54m, paid,   2026, 5, 18,  "Dinner out");
        Tx(aCard, cElectronics, 142m, paid,   2026, 5, 20,  "Noise-cancelling headphones", cpAmazon);
        Tx(aMain, cGroceries,    95m, paid,   2026, 5, 23,  null,              cpLidl);
        Tx(aCash, cTransport,    12m, paid,   2026, 5, 24,  "Bus tickets");
        Tx(aMain, cTransport,    28m, paid,   2026, 5, 28,  "Monthly pass");
        Tx(aCard, cCoffee,       11m, paid,   2026, 5, 29,  null);
        Tx(aMain, cGroceries,    88m, paid,   2026, 5, 31,  null,              cpRewe);
        Tx(aMain, cFreelance,   650m, paid,   2026, 5, 28,  "Website project");

        // ─── June 2026 (vacation month) ───────────────────────────────────────
        Tx(aMain, cSalary,     3800m, booked, 2026, 6,  1,  "June salary");
        Tx(aMain, cRent,        950m, paid,   2026, 6,  1,  "Monthly rent");
        Tx(aMain, cUtilities,    51m, paid,   2026, 6,  4,  "Electricity + water");
        Tx(aMain, cInternet,     35m, paid,   2026, 6,  5,  "Internet + mobile");
        Tx(aMain, cGym,          35m, paid,   2026, 6,  5,  "Monthly membership");
        Tx(aMain, cStreaming,    13m, paid,   2026, 6,  6,  "Netflix",         cpNetflix);
        Tx(aMain, cStreaming,     5m, paid,   2026, 6,  6,  "Spotify",         cpSpotify);
        Tx(aMain, cGroceries,  115m, paid,   2026, 6,  9,  null,              cpAldi);
        Tx(aCard, cCoffee,       16m, paid,   2026, 6, 10,  null);
        Tx(aMain, cFlights,     420m, paid,   2026, 6, 11,  "Summer vacation — round trip", cpLufth);
        Tx(aMain, cHotels,      680m, paid,   2026, 6, 11,  "Barcelona — 5 nights");
        Tx(aCard, cRestaurant,   42m, paid,   2026, 6, 13,  "Farewell dinner");
        Tx(aMain, cGroceries,  132m, paid,   2026, 6, 16,  "Weekly shop",     cpRewe);
        Tx(aCard, cCoffee,       10m, paid,   2026, 6, 18,  null);
        Tx(aMain, cGas,          70m, paid,   2026, 6, 19,  null,              cpAral);
        Tx(aCard, cRestaurant,   88m, paid,   2026, 6, 21,  "Anniversary dinner");
        Tx(aMain, cGroceries,    98m, paid,   2026, 6, 24,  null,              cpLidl);
        Tx(aMain, cTransport,    28m, paid,   2026, 6, 28,  "Monthly pass");
        Tx(aCard, cPharmacy,     25m, paid,   2026, 6, 29,  "Sunscreen & vitamins");

        // ─── July 2026 ────────────────────────────────────────────────────────
        Tx(aMain, cSalary,     3800m, booked, 2026, 7,  1,  "July salary");
        Tx(aMain, cRent,        950m, paid,   2026, 7,  1,  "Monthly rent");
        Tx(aMain, cUtilities,    45m, paid,   2026, 7,  4,  "Electricity + water");
        Tx(aMain, cInternet,     35m, paid,   2026, 7,  5,  "Internet + mobile");
        Tx(aMain, cGym,          35m, paid,   2026, 7,  5,  "Monthly membership");
        Tx(aMain, cStreaming,    13m, paid,   2026, 7,  6,  "Netflix",         cpNetflix);
        Tx(aMain, cStreaming,     5m, paid,   2026, 7,  6,  "Spotify",         cpSpotify);
        Tx(aMain, cGroceries,  108m, paid,   2026, 7,  7,  null,              cpAldi);
        Tx(aCard, cCoffee,        9m, paid,   2026, 7,  8,  null);
        Tx(aCard, cRestaurant,  120m, paid,   2026, 7, 10,  "Vacation dinner");
        Tx(aCard, cRestaurant,   95m, paid,   2026, 7, 12,  "Vacation lunch");
        Tx(aCash, cCoffee,       18m, paid,   2026, 7, 13,  "Vacation coffee");
        Tx(aMain, cGroceries,  140m, paid,   2026, 7, 15,  "Weekly shop",     cpRewe);
        Tx(aCard, cRestaurant,   55m, paid,   2026, 7, 18,  "Dinner out");
        Tx(aCard, cCoffee,       14m, paid,   2026, 7, 20,  null);
        Tx(aMain, cGas,          78m, paid,   2026, 7, 21,  null,              cpAral);
        Tx(aCard, cClothing,    240m, paid,   2026, 7, 22,  "Summer sale — shoes + jeans");
        Tx(aMain, cGroceries,  102m, paid,   2026, 7, 25,  null,              cpLidl);
        Tx(aMain, cTransport,    28m, paid,   2026, 7, 28,  "Monthly pass");
        Tx(aCard, cTaxi,         22m, paid,   2026, 7, 30,  "Late night taxi");

        // ─── August 2026 ──────────────────────────────────────────────────────
        Tx(aMain, cSalary,     3800m, booked, 2026, 8,  1,  "August salary");
        Tx(aMain, cRent,        950m, paid,   2026, 8,  1,  "Monthly rent");
        Tx(aMain, cUtilities,    67m, paid,   2026, 8,  4,  "Electricity + water");
        Tx(aMain, cInternet,     35m, paid,   2026, 8,  5,  "Internet + mobile");
        Tx(aMain, cGym,          35m, paid,   2026, 8,  5,  "Monthly membership");
        Tx(aMain, cStreaming,    13m, paid,   2026, 8,  6,  "Netflix",         cpNetflix);
        Tx(aMain, cStreaming,     5m, paid,   2026, 8,  6,  "Spotify",         cpSpotify);
        Tx(aMain, cGroceries,  125m, paid,   2026, 8,  8,  null,              cpRewe);
        Tx(aCard, cCoffee,       13m, paid,   2026, 8,  9,  null);
        Tx(aMain, cGas,          72m, paid,   2026, 8, 11,  null,              cpAral);
        Tx(aCard, cRestaurant,   48m, paid,   2026, 8, 12,  "Dinner");
        Tx(aMain, cGroceries,  118m, paid,   2026, 8, 15,  "Weekly shop",     cpAldi);
        Tx(aCard, cCoffee,        9m, paid,   2026, 8, 17,  null);
        Tx(aCard, cElectronics,  78m, paid,   2026, 8, 18,  "Mechanical keyboard", cpAmazon);
        Tx(aCard, cRestaurant,   61m, paid,   2026, 8, 20,  "Birthday lunch");
        Tx(aMain, cGroceries,  134m, paid,   2026, 8, 23,  null,              cpLidl);
        Tx(aCard, cPharmacy,     42m, paid,   2026, 8, 25,  "Vitamins & supplements");
        Tx(aMain, cTransport,    28m, paid,   2026, 8, 28,  "Monthly pass");
        Tx(aCash, cCoffee,       16m, paid,   2026, 8, 29,  null);

        // ─── September 2026 (partial — up to 29th) ────────────────────────────
        Tx(aMain, cSalary,     3800m, booked, 2026, 9,  1,  "September salary");
        Tx(aMain, cRent,        950m, paid,   2026, 9,  1,  "Monthly rent");
        Tx(aMain, cUtilities,    71m, paid,   2026, 9,  4,  "Electricity + water");
        Tx(aMain, cInternet,     35m, paid,   2026, 9,  5,  "Internet + mobile");
        Tx(aMain, cGym,          35m, paid,   2026, 9,  5,  "Monthly membership");
        Tx(aMain, cStreaming,    13m, paid,   2026, 9,  6,  "Netflix",         cpNetflix);
        Tx(aMain, cStreaming,     5m, paid,   2026, 9,  6,  "Spotify",         cpSpotify);
        Tx(aMain, cGroceries,  142m, paid,   2026, 9,  9,  null,              cpRewe);
        Tx(aCard, cCoffee,       11m, paid,   2026, 9, 10,  null);
        Tx(aMain, cGas,          68m, paid,   2026, 9, 12,  null,              cpAral);
        Tx(aCard, cRestaurant,   52m, paid,   2026, 9, 14,  "Lunch");
        Tx(aMain, cGroceries,  108m, paid,   2026, 9, 17,  null,              cpAldi);
        Tx(aCard, cPharmacy,     31m, paid,   2026, 9, 20,  "Cold medicine");
        Tx(aCard, cCoffee,       15m, paid,   2026, 9, 22,  null);
        Tx(aCard, cRestaurant,   45m, paid,   2026, 9, 25,  "Dinner");
        Tx(aMain, cTransport,    28m, paid,   2026, 9, 28,  "Monthly pass");
        Tx(aMain, cGroceries,    95m, paid,   2026, 9, 28,  null,              cpLidl);

        await context.Transactions.AddRangeAsync(transactions, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // ── 7. Monthly savings transfers ──────────────────────────────────────
        var transfers = new List<Transaction>();
        void Transfer(int y, int mo, int d, decimal amount)
        {
            var dt = DateTime.SpecifyKind(new DateTime(y, mo, d, 14, 0, 0), DateTimeKind.Utc);
            transfers.Add(Transaction.Create(dt, amount, 1m, eurId,
                aMain.Id, cTrOut.Id, null, paid.Id, uid, "Monthly savings transfer"));
            transfers.Add(Transaction.Create(dt, amount, 1m, eurId,
                aSavings.Id, cTrIn.Id, null, paid.Id, uid, "From main account"));
            aMain.AdjustBalance(-amount);
            aSavings.AdjustBalance(amount);
        }
        Transfer(2026, 4, 29, 500m);
        Transfer(2026, 5, 30, 500m);
        Transfer(2026, 6, 30, 300m); // less this month — vacation spending
        Transfer(2026, 7, 31, 400m);
        Transfer(2026, 8, 29, 500m);
        Transfer(2026, 9, 29, 500m);
        await context.Transactions.AddRangeAsync(transfers, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // ── 8. Opening balance override (pre-April 2026 balance) ─────────────
        var openingOverride = OpeningBalanceOverride.Create(2026, 4, 8_500m, uid);
        openingOverride.UpdatePlanAmount(8_500m);
        await context.OpeningBalanceOverrides.AddAsync(openingOverride, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // ── 9. Budget entries (plan-fact for April – September 2026) ──────────
        var budgets = new List<BudgetEntry>();

        void Plan(Category cat, int y, int mo, decimal amount)
            => budgets.Add(BudgetEntry.Create(cat.Id, y, mo, amount, uid));

        foreach (var (y, mo) in new[] { (2026,4),(2026,5),(2026,6),(2026,7),(2026,8),(2026,9) })
        {
            Plan(cSalary,     y, mo, 3800m);
            Plan(cRent,       y, mo,  950m);
            Plan(cUtilities,  y, mo,   65m);
            Plan(cInternet,   y, mo,   35m);
            Plan(cGym,        y, mo,   35m);
            Plan(cStreaming,  y, mo,   20m);
            Plan(cGroceries,  y, mo,  420m);
            Plan(cRestaurant, y, mo,  190m);
            Plan(cCoffee,     y, mo,   55m);
            Plan(cGas,        y, mo,   80m);
            Plan(cTransport,  y, mo,   30m);
            Plan(cClothing,   y, mo,   80m);
            Plan(cPharmacy,   y, mo,   40m);
        }

        // June: vacation was planned
        Plan(cFlights, 2026, 6, 400m);
        Plan(cHotels,  2026, 6, 700m);

        // May: freelance bonus was not planned (pleasant surprise)
        // April: doctor visit not in plan (surprise expense → shows in fact only)

        await context.BudgetEntries.AddRangeAsync(budgets, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
