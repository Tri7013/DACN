using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;

namespace WEBTRUYEN.Data.Users
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public bool IsVip { get; set; } // Kiểm tra xem người dùng có quyền admin hoặc quyền đặc biệt không

        // Thuộc tính cho gói premium
        public DateTime? VipExpiryDate { get; set; } // Thời hạn VIP
        public void SetVipStatus(bool isVip, int durationInMonths)
        {
            IsVip = isVip;
            VipExpiryDate = isVip ? DateTime.Now.AddMonths(durationInMonths) : (DateTime?)null;
        }

        // Phương thức để kiểm tra xem VIP đã hết hạn chưa
        public bool IsVipActive()
        {
            return IsVip && (VipExpiryDate == null || VipExpiryDate > DateTime.Now);
        }
    }
}
