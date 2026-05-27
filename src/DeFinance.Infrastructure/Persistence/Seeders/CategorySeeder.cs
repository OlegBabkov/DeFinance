using DeFinance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DeFinance.Infrastructure.Persistence.Seeders;

public static class CategorySeeder
{
    private static readonly IReadOnlyList<(string Name, CategoryType Type, string Color, string Icon)> _categories =
    [
        // Income
        ("Salary",            CategoryType.Income,  "#22C55E", "💰"),
        ("Main Income",       CategoryType.Income,  "#16A34A", "💵"),
        ("Secondary Income",  CategoryType.Income,  "#4ADE80", "💸"),
        ("Freelance",         CategoryType.Income,  "#15803D", "💻"),
        ("Business",          CategoryType.Income,  "#166534", "🏢"),
        ("Investments",       CategoryType.Income,  "#86EFAC", "📈"),
        ("Dividends",         CategoryType.Income,  "#34D399", "📊"),
        ("Rental Income",     CategoryType.Income,  "#6EE7B7", "🏠"),
        ("Pension",           CategoryType.Income,  "#A7F3D0", "👴"),
        ("Gifts Received",    CategoryType.Income,  "#BBF7D0", "🎁"),

        // Housing
        ("Apartment Rent",    CategoryType.Expense, "#EF4444", "🏠"),
        ("Mortgage",          CategoryType.Expense, "#DC2626", "🏦"),
        ("Utilities",         CategoryType.Expense, "#F87171", "💡"),
        ("Internet & Phone",  CategoryType.Expense, "#FCA5A5", "📱"),
        ("Home Insurance",    CategoryType.Expense, "#FECACA", "🛡️"),
        ("Home Maintenance",  CategoryType.Expense, "#FEE2E2", "🔨"),

        // Food
        ("Groceries",         CategoryType.Expense, "#F97316", "🛒"),
        ("Restaurants",       CategoryType.Expense, "#FB923C", "🍽️"),
        ("Coffee & Cafes",    CategoryType.Expense, "#FDBA74", "☕"),
        ("Fast Food",         CategoryType.Expense, "#FED7AA", "🍔"),
        ("Alcohol & Bars",    CategoryType.Expense, "#FFEDD5", "🍺"),

        // Transport
        ("Gasoline",          CategoryType.Expense, "#EAB308", "⛽"),
        ("Car Insurance",     CategoryType.Expense, "#CA8A04", "🚗"),
        ("Car Maintenance",   CategoryType.Expense, "#A16207", "🔧"),
        ("Parking",           CategoryType.Expense, "#854D0E", "🅿️"),
        ("Public Transport",  CategoryType.Expense, "#FACC15", "🚌"),
        ("Taxi & Rideshare",  CategoryType.Expense, "#FDE047", "🚕"),

        // Health
        ("Doctor Visits",     CategoryType.Expense, "#EC4899", "👨‍⚕️"),
        ("Pharmacy",          CategoryType.Expense, "#F472B6", "💊"),
        ("Gym & Fitness",     CategoryType.Expense, "#FBCFE8", "🏋️"),
        ("Health Insurance",  CategoryType.Expense, "#FCE7F3", "🏥"),

        // Shopping
        ("Clothing",          CategoryType.Expense, "#8B5CF6", "👗"),
        ("Electronics",       CategoryType.Expense, "#7C3AED", "📱"),
        ("Home & Garden",     CategoryType.Expense, "#6D28D9", "🏡"),
        ("Books & Education", CategoryType.Expense, "#5B21B6", "📚"),
        ("Gifts & Presents",  CategoryType.Expense, "#4C1D95", "🎁"),

        // Entertainment
        ("Movies & Theater",  CategoryType.Expense, "#3B82F6", "🎬"),
        ("Concerts & Events", CategoryType.Expense, "#2563EB", "🎵"),
        ("Streaming Services",CategoryType.Expense, "#1D4ED8", "📺"),
        ("Gaming",            CategoryType.Expense, "#1E40AF", "🎮"),

        // Travel
        ("Flights",           CategoryType.Expense, "#06B6D4", "✈️"),
        ("Hotels",            CategoryType.Expense, "#0891B2", "🏨"),
        ("Car Rental",        CategoryType.Expense, "#0E7490", "🚙"),
        ("Tourist Activities",CategoryType.Expense, "#164E63", "🗺️"),

        // Kids
        ("Childcare",         CategoryType.Expense, "#F43F5E", "👶"),
        ("School & Tuition",  CategoryType.Expense, "#E11D48", "🎒"),
        ("Toys & Activities", CategoryType.Expense, "#BE185D", "🧸"),

        // Personal Care
        ("Haircut & Beauty",  CategoryType.Expense, "#D946EF", "💇"),
        ("Personal Care",     CategoryType.Expense, "#C026D3", "🧴"),

        // Finance
        ("Bank Fees",         CategoryType.Expense, "#6B7280", "🏦"),
        ("Loan Repayment",    CategoryType.Expense, "#4B5563", "💳"),
        ("Taxes",             CategoryType.Expense, "#374151", "📋"),
        ("Subscriptions",     CategoryType.Expense, "#1F2937", "📦"),

        // Other
        ("Pet Care",              CategoryType.Expense, "#84CC16", "🐾"),
        ("Charity",               CategoryType.Expense, "#65A30D", "❤️"),
        ("Expenses for Relatives",CategoryType.Expense, "#F59E0B", "👨‍👩‍👧‍👦"),
    ];

    private static readonly IReadOnlyList<(string Name, CategoryType Type, string Color, string Icon)> _transferCategories =
    [
        ("Transfer In",  CategoryType.TransferIn,  "#64748B", "🔄"),
        ("Transfer Out", CategoryType.TransferOut, "#64748B", "🔄"),
    ];

    public static async Task SeedAsync(DeFinanceDbContext context, CancellationToken cancellationToken = default)
    {
        if (!await context.Categories.AnyAsync(cancellationToken))
        {
            var toAdd = _categories
                .Select(c => Category.Create(c.Name, c.Type, c.Color, c.Icon, null, null))
                .ToList();
            await context.Categories.AddRangeAsync(toAdd, cancellationToken);
        }

        // Always ensure transfer categories exist (for existing installations too)
        if (!await context.Categories.AnyAsync(c => c.Type == CategoryType.TransferIn, cancellationToken))
            await context.Categories.AddAsync(Category.Create("Transfer In", CategoryType.TransferIn, "#64748B", "🔄", null, null), cancellationToken);
        if (!await context.Categories.AnyAsync(c => c.Type == CategoryType.TransferOut, cancellationToken))
            await context.Categories.AddAsync(Category.Create("Transfer Out", CategoryType.TransferOut, "#64748B", "🔄", null, null), cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}
