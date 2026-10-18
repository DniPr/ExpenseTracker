using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Controllers;

public class FinancialGoalsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}