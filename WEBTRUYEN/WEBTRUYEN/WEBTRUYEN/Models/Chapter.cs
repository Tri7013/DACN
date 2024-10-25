using System.ComponentModel.DataAnnotations;

namespace WEBTRUYEN.Models
{
    public class Chapter
    {
        [Key]
        public int Id { get; set; } // Khóa chính của Chapter

        [Required]
        [StringLength(100)]
        public string Title { get; set; } // Tiêu đề chương

        [Required]
        public string Content { get; set; } // Nội dung của chương

        public int ProductId { get; set; } // Khóa ngoại liên kết với Product (Truyện)
        public Product? Product { get; set; } // Liên kết ngược đến truyện

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Ngày tạo

        public int ChapterNumber { get; set; } // Số thứ tự của chương
        public bool IsPremium { get; set; }
        public string? FilePath { get; set; } // Đường dẫn file chương truyện
    }


}
