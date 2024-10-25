using System.ComponentModel.DataAnnotations;
using WEBTRUYEN.Data.Users;

namespace WEBTRUYEN.Models
{
    public class Rating
    {
        [Key]
        public int Id { get; set; } // Khóa chính

        public int ProductId { get; set; } // ID của truyện được đánh giá
        public Product Product { get; set; } // Truyện được đánh giá

        public string UserId { get; set; } // ID của người dùng đánh giá
        public ApplicationUser User { get; set; } // Người dùng đánh giá

        [Range(1, 5)] // Giới hạn đánh giá từ 1 đến 5
        public int Score { get; set; } // Điểm đánh giá

        public string? Comment { get; set; } // Bình luận bổ sung (tùy chọn)
    }

}
