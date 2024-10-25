using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WEBTRUYEN.Data.Users;
using WEBTRUYEN.Data;

using WEBTRUYEN.Areas.Admin.Controllers;
using Microsoft.EntityFrameworkCore;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Controllers
{
    public class CategoriesController : Controller
    {
        
        private readonly ApplicationDbContext db;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        public CategoriesController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            db = context;
           
            _logger = logger;
            _userManager = userManager; // Kh?i t?o _userManager
        }
        public IActionResult Watching(int chapterId)
        {
            // Lấy chương truyện theo ID
            var chapter = db.Chapters.FirstOrDefault(c => c.Id == chapterId);
            if (chapter == null)
            {
                return NotFound(); // Trả về 404 nếu chương không tồn tại
            }

            // Lấy thông tin người dùng hiện tại
            var userId = _userManager.GetUserId(User);
            var user = userId != null ? db.Users.FirstOrDefault(u => u.Id == userId) : null;

            // Kiểm tra quyền truy cập
            if (chapter.IsPremium && (user == null || !user.IsVip))
            {
                // Nếu chương là premium và người dùng không có quyền VIP, chuyển hướng đến trang yêu cầu mua gói VIP
                return RedirectToAction("PremiumAccessRequired");
            }

            // Đọc nội dung từ file chương
            string fileContent = string.Empty;
            if (!string.IsNullOrEmpty(chapter.FilePath))
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files", chapter.FilePath);

                // Kiểm tra tồn tại file và đọc nội dung
                if (System.IO.File.Exists(filePath))
                {
                    fileContent = System.IO.File.ReadAllText(filePath);
                }
                else
                {
                    // Xử lý trường hợp không tìm thấy file
                    ModelState.AddModelError("", "Nội dung chương không có sẵn.");
                    return View("Error"); // Hoặc trang lỗi tùy ý
                }
            }

            // Gán nội dung file vào ViewBag để hiển thị trong View
            ViewBag.FileContent = fileContent;
            return View(chapter); // Trả về View với chương và nội dung
        }






        public async Task<IActionResult> Index(string searchTerm, List<int> categoryIds)
        {
            var query = db.Products
                .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
                .Include(p => p.Comments)
                .AsQueryable();

            // Lọc sản phẩm theo tiêu chí tìm kiếm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm)
                                      || p.Description.Contains(searchTerm));
            }

            // Lọc sản phẩm theo các category đã chọn
            if (categoryIds != null && categoryIds.Any())
            {
                query = query.Where(p => p.ProductCategories.Any(pc => categoryIds.Contains(pc.CategoryId)));
            }

            var products = await query.ToListAsync();

            // Lấy tất cả các Category để hiển thị trong view
            var categories = await db.Categories.ToListAsync();
            ViewBag.Categories = categories; // Truyền danh sách Category tới View

            return View(products);
        }


        public async Task<IActionResult> Details(int id, int commentPage = 1, int commentPageSize = 5)
        {
            var product = await db.Products
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.Comments)
                    .ThenInclude(c => c.Replies)
                .Include(p => p.Chapters) // Đảm bảo tải danh sách chương
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // Tăng số lượt xem
            product.IncrementViewCount();
            await db.SaveChangesAsync();

            // Lấy ID người dùng hiện tại
            var userId = _userManager.GetUserId(User);
            var user = userId != null ? await db.Users.FindAsync(userId) : null;

            // Kiểm tra quyền truy cập để hiển thị danh sách chương
            List<Chapter> chaptersToDisplay = null;

            if (product.IsPremium) // Nếu truyện là premium
            {
                if (user != null && user.IsVip)
                {
                    // Nếu người dùng là VIP, hiển thị tất cả các chương
                    chaptersToDisplay = product.Chapters.ToList();
                }
                else
                {
                    // Nếu không, không hiển thị chương
                    ViewBag.Message = "Bạn cần đăng ký VIP để xem danh sách chương.";
                }
            }
            else
            {
                // Nếu truyện không phải là premium, hiển thị tất cả các chương
                chaptersToDisplay = product.Chapters.ToList();
            }

            // Tính điểm trung bình cho sản phẩm
            var ratings = await db.Ratings.Where(r => r.ProductId == id).ToListAsync();
            double averageRating = ratings.Any() ? ratings.Average(r => r.Score) : 0;
            ViewBag.AverageRating = averageRating;

            // Kiểm tra xem người dùng có theo dõi sản phẩm không
            product.IsFollowed = await db.Follows.AnyAsync(f => f.UserId == userId && f.ProductId == id);

            // Lấy đánh giá của người dùng hiện tại (nếu có)
            var userRating = await db.Ratings
                .FirstOrDefaultAsync(r => r.ProductId == id && r.UserId == userId);
            ViewBag.UserRating = userRating;

            // Phân trang bình luận
            var totalComments = await db.Comments.CountAsync(c => c.ProductId == id);
            var pagedComments = await db.Comments
                .Where(c => c.ProductId == id)
                .OrderByDescending(c => c.CreatedAt)
                .Skip((commentPage - 1) * commentPageSize)
                .Take(commentPageSize)
                .ToListAsync();

            ViewBag.CommentTotalPages = (int)Math.Ceiling((double)totalComments / commentPageSize);
            ViewBag.CommentCurrentPage = commentPage;

            // Truyền dữ liệu sang ViewModel
            var viewModel = new ProductDetailsViewModel
            {
                Product = product,
                Comments = pagedComments,
                UserRating = userRating,
                Chapters = chaptersToDisplay, // Truyền danh sách chương vào ViewModel
                User = user // Thêm người dùng vào ViewModel
            };

            return View(viewModel);
        }

        

    }
}
