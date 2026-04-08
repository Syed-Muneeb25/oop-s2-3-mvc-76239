using Microsoft.AspNetCore.Mvc;

namespace VgcCollege.Web.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
