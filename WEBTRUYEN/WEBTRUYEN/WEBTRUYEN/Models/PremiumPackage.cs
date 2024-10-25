namespace WEBTRUYEN.Models
{
    public class PremiumPackage
    {
        public int Id { get; set; } // ID gói premium
        public string Name { get; set; } // Tên gói
        public decimal Price { get; set; } // Giá
        public int DurationInMonths { get; set; } // Thời gian sử dụng (tháng)
    }
}
