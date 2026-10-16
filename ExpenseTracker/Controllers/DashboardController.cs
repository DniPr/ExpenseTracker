using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Controllers;

public class DashboardController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}