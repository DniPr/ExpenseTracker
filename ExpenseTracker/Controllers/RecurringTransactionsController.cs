using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Controllers;

public class RecurringTransactionsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}