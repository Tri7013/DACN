namespace WEBTRUYEN.Models
{
    public class VnPaymentResponseModel
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderDescription { get; set; }
        public string TransactionId { get; set; }
        public string Token { get; set; }
        public string VnPayResponseCode { get; set; }
        public string PremiumPackageId { get; set; } // Thêm thuộc tính này
        public DateTime TransactionDate { get; set; } // Thêm thời gian giao dịch
        public double Amount { get; set; } // Số tiền thanh toán
    }
    public class VnPaymentRequestModel
    {
        public int PremiumPackageId { get; set; }
        public string FullName { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
