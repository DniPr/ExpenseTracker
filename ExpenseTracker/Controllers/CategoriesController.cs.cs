using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.Web.Controllers;

public class CategoriesController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}