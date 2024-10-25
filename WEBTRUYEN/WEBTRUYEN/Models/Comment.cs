using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WEBTRUYEN.Data.Users;

namespace WEBTRUYEN.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; } // Khóa chính của bình luận

        [Required(ErrorMessage = "Nội dung bình luận là bắt buộc.")]
        [StringLength(1000, ErrorMessage = "Nội dung bình luận không được vượt quá 1000 ký tự.")]
        public string Content { get; set; } // Nội dung bình luận

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo bình luận

        // Khóa ngoại liên kết đến người dùng (tác giả bình luận)
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } // Tác giả bình luận

        // Khóa ngoại liên kết đến sản phẩm
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; } // Sản phẩm mà bình luận này thuộc về
        public List<Comment> Replies { get; set; } = new List<Comment>();
    }
}

       