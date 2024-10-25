using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEBTRUYEN.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; } // Khóa chính

        [Required(ErrorMessage = "Tên thể loại là bắt buộc.")]
        [StringLength(100, ErrorMessage = "Tên thể loại không được vượt quá 100 ký tự.")]
        public string Name { get; set; } // Tên thể loại

       
       

        // Danh sách các sản phẩm thuộc thể loại này
      
        public ICollection<Product> ProductCategories { get; set; } = new List<Product>();
    }
}
