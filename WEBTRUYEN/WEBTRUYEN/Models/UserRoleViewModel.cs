using Microsoft.AspNetCore.Identity;

namespace WEBTRUYEN.Models
{
    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<IdentityRole> AllRoles { get; set; }
        public List<string> Roles { get; set; }

        // Các thuộc tính thêm từ ApplicationUser
        public bool IsPremium { get; set; }
        public DateTime? PremiumExpirationDate { get; set; }
        public int PremiumPackageDuration { get; set; }
    }
}
