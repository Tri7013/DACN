using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WEBTRUYEN.Data;
using WEBTRUYEN.Models;

namespace WEBTRUYEN.Controllers
{
    public class RatingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RatingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> RateProduct(int productId, int score, string? comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Lấy ID của người dùng đăng nhập

            if (score < 1 || score > 5)
            {
                return BadRequest("Điểm đánh giá phải từ 1 đến 5.");
            }

            // Kiểm tra xem người dùng đã đánh giá sản phẩm này chưa
            var existingRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);

            if (existingRating != null)
            {
                existingRating.Score = score; // Cập nhật điểm
                existingRating.Comment = comment; // Cập nhật bình luận
                _context.Ratings.Update(existingRating);
            }
            else
            {
                var rating = new Rating
                {
                    ProductId = productId,
                    UserId = userId,
                    Score = score,
                    Comment = comment
                };
                _context.Ratings.Add(rating);
            }

            await _context.SaveChangesAsync();

            // Tính điểm trung bình
            var averageRating = await _context.Ratings
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => r.Score);

            return Ok(new { averageRating = averageRating }); // Trả về điểm trung bình
        }





    }
}
