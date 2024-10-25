using Microsoft.AspNetCore.Mvc;
using WEBTRUYEN.Data;
using WEBTRUYEN.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WEBTRUYEN.Data.Users;

namespace WEBTRUYEN.Controllers
{
    public class FollowController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public FollowController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> MyFollows()
        {
            var userId = _userManager.GetUserId(User); // Lấy ID của người dùng hiện tại
            var followedProducts = await _context.Follows
                .Include(f => f.Product) // Lấy thông tin sản phẩm
                .Where(f => f.UserId == userId)
                .Select(f => f.Product)
                .ToListAsync();

            return View(followedProducts); // Trả về view với danh sách sản phẩm đã theo dõi
        }

        [HttpPost]
        public async Task<IActionResult> FollowProduct([FromBody] int productId)
        {
            var userId = _userManager.GetUserId(User); // Lấy ID của người dùng hiện tại
            var follow = new Follow { UserId = userId, ProductId = productId };

            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existingFollow == null)
            {
                _context.Follows.Add(follow); // Thêm theo dõi mới
                await _context.SaveChangesAsync();
                return Ok(new { message = "Successfully followed the product." }); // Thông báo thành công
            }

            return BadRequest(new { message = "User is already following this product." }); // Thông báo người dùng đã theo dõi
        }


        [HttpPost]
        public async Task<IActionResult> UnfollowProduct([FromBody] int productId)
        {
            var userId = _userManager.GetUserId(User);
            var existingFollow = await _context.Follows
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

            if (existingFollow != null)
            {
                try
                {
                    _context.Follows.Remove(existingFollow);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Successfully unfollowed the product." }); // Thông điệp thành công
                }
                catch (DbUpdateException)
                {
                    return StatusCode(500, "Internal server error"); // Lỗi máy chủ
                }
            }

            return BadRequest(new { message = "User is not following this product." }); // Người dùng chưa theo dõi
        }


    }
}
