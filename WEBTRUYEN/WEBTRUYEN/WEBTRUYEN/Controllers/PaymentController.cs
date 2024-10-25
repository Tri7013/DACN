using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using WEBTRUYEN.Data;
using WEBTRUYEN.Data.Users;
using WEBTRUYEN.Models;
using WEBTRUYEN.VnPay;

namespace WEBTRUYEN.Controllers
{
    public class PaymentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PaymentController(ApplicationDbContext context, IVnPayService vnPayService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _vnPayService = vnPayService;
            _userManager = userManager; // Kh?i t?o _userManager
        }

        [HttpPost]
        public IActionResult Execute(int premiumPackageId)
        {
            var premiumPackage = _context.PremiumPackages.FirstOrDefault(p => p.Id == premiumPackageId);
            if (premiumPackage == null)
            {
                return NotFound();
            }

            var amount = premiumPackage.Price;

            var vnpayRequest = new VnPaymentRequestModel
            {
                PremiumPackageId = premiumPackageId,
                FullName = User.Identity.Name,
                Description = $"Thanh toán gói premium: {premiumPackage.Name}",
                Amount = (double)amount,
                CreatedDate = DateTime.Now
            };

            var vnpUrl = _vnPayService.CreatePaymentUrl(HttpContext, vnpayRequest);
            return Redirect(vnpUrl);
        }

        public IActionResult PaymentResult()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response.Success)
            {
                // Cập nhật trạng thái VIP của người dùng
                var userId = _userManager.GetUserId(User);
                var user = _context.Users.Find(userId);
                if (user != null)
                {
                    // Giả sử bạn đã có gói premium được chọn và có thời gian VIP là 1 tháng
                    int durationInMonths = 1; // Có thể thay đổi tùy theo gói
                    user.SetVipStatus(true, durationInMonths); // Thiết lập trạng thái VIP
                    _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
                }

                ViewBag.Message = "Thanh toán thành công!";
            }
            else
            {
                ViewBag.Message = "Thanh toán thất bại!";
            }
            return View();
        }

    }
}
