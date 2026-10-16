using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Controllers;

public class BudgetsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}