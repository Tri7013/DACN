using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEBTRUYEN.Models
{
    public class ProductCreateViewModel
    {
        [Required(ErrorMessage = "Tên truyện là bắt buộc.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Mô tả truyện là bắt buộc.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Chọn trạng thái premium.")]
        public bool IsPremium { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true; // Thêm thuộc tính IsActive

        // Danh sách ID của các thể loại được chọn
        public List<int> SelectedCategoryIds { get; set; } = new List<int>();
    }
}
