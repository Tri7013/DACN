using System.ComponentModel.DataAnnotations;

namespace WEBTRUYEN.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên truyện là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên truyện không được vượt quá 100 ký tự.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả truyện là bắt buộc.")]
        public string Description { get; set; }

        [Required]
        public bool IsPremium { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public int Likes { get; private set; } = 0;

        public int ViewCount { get; private set; } = 0;

        public virtual ICollection<ProductCategory> ProductCategories { get; set; } = new List<ProductCategory>();

        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

        public virtual ICollection<Chapter> Chapters { get; set; } = new List<Chapter>();

        public string PremiumStatus => IsPremium ? "VIP" : "Free";

        public bool IsFollowed { get; set; }

        public void IncrementLikes() => Likes++;
        public void IncrementViewCount() => ViewCount++;
    }
}
