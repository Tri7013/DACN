using WEBTRUYEN.Data.Users;
using WEBTRUYEN.Models;

public class Follow
{
    public int Id { get; set; } // Khóa chính
    public string UserId { get; set; } // ID của người dùng
    public int ProductId { get; set; } // ID của sản phẩm

    public virtual ApplicationUser User { get; set; } // Tham chiếu đến người dùng
    public virtual Product Product { get; set; } // Tham chiếu đến sản phẩm
}
