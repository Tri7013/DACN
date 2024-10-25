using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WEBTRUYEN.Data.Users;
using WEBTRUYEN.Data;

using WEBTRUYEN.Areas.Admin.Controllers;
using Microsoft.EntityFrameworkCore;
using WEBTRUYEN.Models;
using Microsoft.AspNetCore.Hosting;

namespace WEBTRUYEN.Controllers
{
    public class CategoriesController : Controller
    {
        
        private readonly ApplicationDbContext db;
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public CategoriesController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            db = context;
           
            _logger = logger;
            _userManager = userManager; // Kh?i t?o _userManager
            _webHostEnvironment = webHostEnvironment;
        }
        public async Task<IActionResult> Watching(int chapterId)
        {
            // Lấy thông tin chương theo ID
            var chapter = await db.Chapters.FirstOrDefaultAsync(c => c.Id == chapterId);

            if (chapter == null)
            {
                return NotFound();
            }

            // Lấy tất cả các chương của truyện
            var allChapters = await db.Chapters
                .Where(c => c.ProductId == chapter.ProductId) // Giả sử bạn có ProductId để lọc các chương theo truyện
                .OrderBy(c => c.Id)

                .ToListAsync();

            // Tìm chỉ số của chương hiện tại trong danh sách
            int currentIndex = allChapters.FindIndex(c => c.Id == chapterId);

            // Xác định chương trước và chương tiếp theo
            chapter.PreviousChapterId = (currentIndex > 0) ? allChapters[currentIndex - 1].Id : (int?)null;
            chapter.NextChapterId = (currentIndex < allChapters.Count - 1) ? allChapters[currentIndex + 1].Id : (int?)null;

            // Lấy danh sách bình luận của chương
            var chapterComments = await db.ChapterComments
                .Where(cc => cc.ChapterId == chapterId)
                .OrderByDescending(cc => cc.CreatedAt) // Giả sử bạn có thuộc tính CreatedAt để sắp xếp theo thời gian
                .ToListAsync();

            // Truyền dữ liệu vào ViewModel
            var viewModel = new ProductDetailsViewModel
            {
                Chapter = chapter,
                AllChapters = allChapters, // Danh sách tất cả các chương
                ChapterComments = chapterComments // Thêm bình luận của chương vào view model
            };

            // Lấy đường dẫn đầy đủ tới file
            var extension = System.IO.Path.GetExtension(chapter.FilePath).ToLower();
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, chapter.FilePath.TrimStart('~', '/'));

            // Kiểm tra xem file có tồn tại không
            if (System.IO.File.Exists(filePath))
            {
                // Gán thuộc tính Content dựa trên loại file
                if (extension == ".txt")
                {
                    chapter.Content = await System.IO.File.ReadAllTextAsync(filePath);
                }
                else if (extension == ".jpg" || extension == ".png" || extension == ".gif")
                {
                    // Nếu là hình ảnh, có thể lưu URL để hiển thị trong view
                    chapter.Content = $"<img src='{Url.Content(chapter.FilePath)}' alt='Content Image' style='max-width: 100%; height: auto;' />";
                }
                else if (extension == ".pdf")
                {
                    // Nếu là PDF, có thể lưu URL để hiển thị trong view
                    chapter.Content = $"<iframe src='{Url.Content(chapter.FilePath)}' width='100%' height='600px' style='border: none;'></iframe>";
                }
                else
                {
                    chapter.Content = "Định dạng tệp không được hỗ trợ.";
                }
            }
            else
            {
                chapter.Content = "Tệp không tồn tại.";
            }

            return View(viewModel); // Truyền dữ liệu chapter và bình luận tới view
        }

        [HttpPost]
        public async Task<IActionResult> AddComment(int chapterId, string content)
        {
            // Kiểm tra nếu người dùng đã đăng nhập
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            // Lấy thông tin người dùng hiện tại
            var user = await _userManager.GetUserAsync(User);

            // Kiểm tra chương có tồn tại không
            var chapter = await db.Chapters.FirstOrDefaultAsync(c => c.Id == chapterId);
            if (chapter == null)
            {
                return NotFound();
            }

            // Tạo bình luận mới
            var newComment = new ChapterComment
            {
                ChapterId = chapterId,
                Content = content,
                UserId = user.Id, // Gán người dùng hiện tại làm tác giả
                CreatedAt = DateTime.Now
            };

            // Thêm bình luận vào cơ sở dữ liệu
            db.ChapterComments.Add(newComment);
            await db.SaveChangesAsync();

            // Chuyển hướng lại trang Watching để hiển thị bình luận mới
            return RedirectToAction("Watching", new { chapterId = chapterId });
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
                else if (user == null)
                {
                    // Nếu chưa đăng nhập, hiển thị thông báo yêu cầu đăng nhập
                    ViewBag.Message = "Bạn cần đăng nhập để xem danh sách chương.";
                }
                else
                {
                    // Nếu không, không hiển thị chương
                    ViewBag.Message = "Bạn cần đăng ký VIP để xem danh sách chương.";
                }
            }
            else
            {
                // Nếu truyện không phải là premium, kiểm tra người dùng đã đăng nhập chưa
                if (user == null)
                {
                    // Nếu chưa đăng nhập, hiển thị thông báo yêu cầu đăng nhập
                    ViewBag.Message = "Bạn cần đăng nhập để xem danh sách chương.";
                }
                else
                {
                    // Nếu người dùng đã đăng nhập, hiển thị tất cả các chương
                    chaptersToDisplay = product.Chapters.ToList();
                }
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
                Chapters = chaptersToDisplay, // Chỉ truyền danh sách chương nếu có quyền truy cập
                User = user // Thêm người dùng vào ViewModel
            };

            return View(viewModel);
        }





    }
}
