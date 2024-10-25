using Microsoft.AspNetCore.Mvc;
using System.Linq;
using WEBTRUYEN.Data;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Controllers
{
    public class PremiumPackageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PremiumPackageController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Phương thức hiển thị danh sách gói premium
        public IActionResult Index()
        {
            var packages = _context.PremiumPackages.ToList(); // Lấy tất cả các gói từ DbContext
            return View(packages); // Trả về view hiển thị danh sách gói
        }
    }
}
