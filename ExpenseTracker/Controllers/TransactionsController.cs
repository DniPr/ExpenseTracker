using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Controllers;

public class TransactionsController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}