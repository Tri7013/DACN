using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WEBTRUYEN.Models
{
    public class ProductCategory
    {
        public int ProductId { get; set; } // Khóa ngoại từ Product
        public Product Product { get; set; } // Tham chiếu đến sản phẩm

        public int CategoryId { get; set; } // Khóa ngoại từ Category
        public Category Category { get; set; } // Tham chiếu đến thể loại
    }

}
