using ExpenseTracker.Data.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;

    public DbSet<Transaction> Transactions { get; set; } = null!;

    public DbSet<Budget> Budgets { get; set; } = null!;

    public DbSet<RecurringTransaction> RecurringTransactions { get; set; } = null!;

    public DbSet<FinancialGoal> FinancialGoals { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureCategory(builder);
        ConfigureTransaction(builder);
        ConfigureBudget(builder);
        ConfigureRecurringTransaction(builder);
        ConfigureFinancialGoal(builder);
    }

    private static void ConfigureCategory(ModelBuilder builder)
    {
        builder.Entity<Category>()
            .HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Category>()
            .HasIndex(c => new
            {
                c.Name,
                c.Type,
                c.UserId
            });
    }

    private static void ConfigureTransaction(ModelBuilder builder)
    {
        builder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasPrecision(18, 2);

        builder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Transaction>()
            .HasOne(t => t.Category)
            .WithMany(c => c.Transactions)
            .HasForeignKey(t => t.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureBudget(ModelBuilder builder)
    {
        builder.Entity<Budget>()
            .Property(b => b.Amount)
            .HasPrecision(18, 2);

        builder.Entity<Budget>()
            .HasOne(b => b.User)
            .WithMany(u => u.Budgets)
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Budget>()
            .HasOne(b => b.Category)
            .WithMany(c => c.Budgets)
            .HasForeignKey(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureRecurringTransaction(ModelBuilder builder)
    {
        builder.Entity<RecurringTransaction>()
            .Property(rt => rt.Amount)
            .HasPrecision(18, 2);

        builder.Entity<RecurringTransaction>()
            .HasOne(rt => rt.User)
            .WithMany(u => u.RecurringTransactions)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RecurringTransaction>()
            .HasOne(rt => rt.Category)
            .WithMany(c => c.RecurringTransactions)
            .HasForeignKey(rt => rt.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureFinancialGoal(ModelBuilder builder)
    {
        builder.Entity<FinancialGoal>()
            .Property(g => g.TargetAmount)
            .HasPrecision(18, 2);

        builder.Entity<FinancialGoal>()
            .Property(g => g.CurrentAmount)
            .HasPrecision(18, 2);

        builder.Entity<FinancialGoal>()
            .HasOne(g => g.User)
            .WithMany(u => u.FinancialGoals)
            .HasForeignKey(g => g.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}