using ExpenseTracker.DTOs.Dashboard;

namespace ExpenseTracker.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(string userId);
}