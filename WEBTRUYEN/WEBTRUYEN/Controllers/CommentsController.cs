using Microsoft.AspNetCore.Mvc;
using WEBTRUYEN.Data;
using WEBTRUYEN.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using WEBTRUYEN.Data.Users;

namespace WEBTRUYEN.Controllers
{
    public class CommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CommentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int productId, string content)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("Content cannot be empty.");
            }

            var comment = new Comment
            {
                UserId = userId,
                ProductId = productId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Home", new { id = productId });
        }
        [HttpPost]
        public async Task<IActionResult> Reply(int parentCommentId, string content)
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(content))
            {
                return BadRequest("Content cannot be empty.");
            }

            // Tìm bình luận cha
            var parentComment = await _context.Comments.FindAsync(parentCommentId);
            if (parentComment != null)
            {
                // Tạo bình luận mới
                var reply = new Comment
                {
                    UserId = userId, // Lấy ID người dùng hiện tại
                    ProductId = parentComment.ProductId, // Tham chiếu đến sản phẩm từ bình luận cha
                    Content = content, // Gán nội dung bình luận
                    CreatedAt = DateTime.UtcNow // Sử dụng UTC nếu cần
                };

                // Thêm bình luận con vào bình luận cha
                parentComment.Replies.Add(reply);

                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Details", "Home", new { id = parentComment.ProductId });
        }


    }
}
