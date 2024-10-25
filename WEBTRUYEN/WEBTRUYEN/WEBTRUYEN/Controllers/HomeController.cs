using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WEBTRUYEN.Data.Users;
using WEBTRUYEN.Data;

using WEBTRUYEN.Areas.Admin.Controllers;
using Microsoft.EntityFrameworkCore;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Controllers
{
    public class HomeController : Controller
    {

        private readonly ApplicationDbContext db;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;

            _logger = logger;
            _userManager = userManager; // Kh?i t?o _userManager
        }
        public IActionResult Watching(int chapterId)
        {
            // L?y ch??ng truy?n theo ID
            var chapter = db.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
            {
                return NotFound(); // Tr? v? 404 n?u ch??ng kh�ng t?n t?i
            }

            // L?y th�ng tin ng??i d�ng hi?n t?i
            var userId = _userManager.GetUserId(User);
            var user = userId != null ? db.Users.FirstOrDefault(u => u.Id == userId) : null;

            // Ki?m tra quy?n truy c?p
            if (chapter.IsPremium && (user == null || !user.IsVip))
            {
                // N?u ch??ng l� premium v� ng??i d�ng kh�ng c� quy?n VIP, chuy?n h??ng ??n trang y�u c?u mua g�i VIP
                return RedirectToAction("PremiumAccessRequired");
            }

            // ??c n?i dung t? file ch??ng
            string fileContent = string.Empty;
            if (!string.IsNullOrEmpty(chapter.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", chapter.FilePath);

                // Ki?m tra t?n t?i file v� ??c n?i dung
                if (System.IO.File.Exists(filePath))
                {
                    fileContent = System.IO.File.ReadAllText(filePath);
                }
                else
                {
                    // X? l� tr??ng h?p kh�ng t�m th?y file
                    ModelState.AddModelError("", "N?i dung ch??ng kh�ng c� s?n.");
                    return View("Error"); // Ho?c trang l?i t�y �
                }
            }

            // G�n n?i dung file v�o ViewBag ?? hi?n th? trong View
            ViewBag.FileContent = fileContent;
            return View(chapter); // Tr? v? View v?i ch??ng v� n?i dung
        }






        public async Task<IActionResult> Index(string searchTerm, List<int> categoryIds)
        {

            var query = db.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.Comments)
                .AsQueryable();

            // L?c s?n ph?m theo ti�u ch� t�m ki?m
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm)
                                      || p.Description.Contains(searchTerm));
            }

            // L?c s?n ph?m theo c�c category ?� ch?n
            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(p => p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
            }

            var products = await query.ToListAsync();

            // L?y t?t c? c�c Category ?? hi?n th? trong view
            var categories = await db.Categories.ToListAsync();
            ViewBag.Categories = categories; // Truy?n danh s�ch Category t?i View

            return View(products);
        }


        public async Task<IActionResult> Details(int id, int commentPage = 1, int commentPageSize = 5)
        {
            // L?y s?n ph?m c�ng v?i c�c th�ng tin li�n quan
            var product = await db.Products
                .Include(p => p.ProductCategories).ThenInclude(pc => pc.Category)
                .Include(p => p.Comments).ThenInclude(c => c.Replies)
                .Include(p => p.Chapters)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound("S?n ph?m kh�ng t?n t?i.");
            }

            // T?ng l??t xem v� l?u thay ??i
            product.IncrementViewCount();
            await db.SaveChangesAsync();

            // L?y th�ng tin ng??i d�ng hi?n t?i
            var userId = _userManager.GetUserId(User);
            var user = userId != null ? await db.Users.FindAsync(userId) : null;

            List<Chapter> chaptersToDisplay;

            // Ki?m tra quy?n truy c?p ch??ng
            if (product.IsPremium && (user == null || !user.IsVip))
            {
                ViewBag.Message = "B?n c?n ??ng k� VIP ?? xem danh s�ch ch??ng.";
                chaptersToDisplay = null; // kh�ng hi?n th? ch??ng
            }
            else
            {
                chaptersToDisplay = product.Chapters.ToList();
            }

            // T�nh ?i?m trung b�nh
            double averageRating = await CalculateAverageRating(id);
            ViewBag.AverageRating = averageRating;

            // Ki?m tra theo d�i s?n ph?m
            product.IsFollowed = await db.Follows.AnyAsync(f => f.UserId == userId && f.ProductId == id);

            // L?y ?�nh gi� c?a ng??i d�ng
            var userRating = await db.Ratings
                .FirstOrDefaultAsync(r => r.ProductId == id && r.UserId == userId);
            ViewBag.UserRating = userRating;

            // Ph�n trang b�nh lu?n
            var totalComments = await db.Comments.CountAsync(c => c.ProductId == id);
            var pagedComments = await db.Comments
                .Where(c => c.ProductId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((commentPage - 1) * commentPageSize)
                .Take(commentPageSize)
                .ToListAsync();

            ViewBag.CommentTotalPages = (int)Math.Ceiling((double)totalComments / commentPageSize);
            ViewBag.CommentCurrentPage = commentPage;

            // L?y 5 s?n ph?m li�n quan
            var relatedProducts = await db.Products
                .Where(p => p.Id != id) // Kh�ng l?y s?n ph?m hi?n t?i
                .OrderByDescending(p => Guid.NewGuid()) // L?y ng?u nhi�n
                .Take(5)
                .ToListAsync();
            ViewBag.RelatedProducts = relatedProducts; // Truy?n v�o ViewBag

            // Truy?n d? li?u sang ViewModel
            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Comments = pagedComments,
                UserRating = userRating,
                Chapters = chaptersToDisplay,
                User = user
            };

            return View(viewModel);
        }

        // Ph??ng th?c t�nh ?i?m trung b�nh
        private async Task<double> CalculateAverageRating(int productId)
        {
            var ratings = await db.Ratings.Where(r => r.ProductId == productId).ToListAsync();
            return ratings.Any() ? ratings.Average(r => r.Score) : 0;
        }




    }
}
