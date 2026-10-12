using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace ExpenseTracker.Data.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateTime CreatedOn { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; }
        = new HashSet<Transaction>();

    public virtual ICollection<Category> Categories { get; set; }
        = new HashSet<Category>();

    public virtual ICollection<Budget> Budgets { get; set; }
        = new HashSet<Budget>();

    public virtual ICollection<RecurringTransaction> RecurringTransactions { get; set; }
        = new HashSet<RecurringTransaction>();

    public virtual ICollection<FinancialGoal> FinancialGoals { get; set; }
        = new HashSet<FinancialGoal>();
}