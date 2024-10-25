using Microsoft.AspNetCore.Mvc;

namespace WEBTRUYEN.Controllers
{
    public class BlogController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
