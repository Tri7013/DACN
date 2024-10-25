using System.Collections.Generic;
using WEBTRUYEN.Data.Users;

namespace WEBTRUYEN.Models
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<Comment> Comments { get; set; }
        public Rating? UserRating { get; set; } // Đánh giá của người dùng (nếu có)
        public int CommentCount => Comments?.Count ?? 0; // Số lượng bình luận

        public List<Chapter> Chapters { get; set; } // Danh sách các chương của sản phẩm
        public ApplicationUser User { get; set; } // Thêm dòng này
    }
}
