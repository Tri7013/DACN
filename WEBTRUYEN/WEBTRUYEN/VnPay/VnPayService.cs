using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using WEBTRUYEN.Models;
using Microsoft.Extensions.Configuration;

namespace WEBTRUYEN.VnPay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _config;

        public VnPayService(IConfiguration config)
        {
            _config = config;
        }

        public string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model)
        {
            // Tạo request đến VNPay
            var vnpay = new VnPayLibrary();
            var transactionRef = DateTime.Now.Ticks.ToString(); // Mã giao dịch duy nhất

            vnpay.AddRequestData("vnp_Version", _config["VnPay:Version"]);
            vnpay.AddRequestData("vnp_Command", _config["VnPay:Command"]);
            vnpay.AddRequestData("vnp_TmnCode", _config["VnPay:TmnCode"]);
            vnpay.AddRequestData("vnp_Amount", (model.Amount * 100).ToString());
            vnpay.AddRequestData("vnp_CreateDate", model.CreatedDate.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", _config["VnPay:CurrCode"]);
            vnpay.AddRequestData("vnp_IpAddr", Utils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", _config["VnPay:Locale"]);
            vnpay.AddRequestData("vnp_OrderInfo", model.Description);
            vnpay.AddRequestData("vnp_OrderType", "other"); // Thay đổi loại hàng hóa theo nhu cầu
            vnpay.AddRequestData("vnp_ReturnUrl", _config["VnPay:PaymentBackReturnUrl"]);
            vnpay.AddRequestData("vnp_ExpireDate", DateTime.Now.AddMinutes(15).ToString("yyyyMMddHHmmss")); // Hết hạn sau 15 phút
            vnpay.AddRequestData("vnp_TxnRef", transactionRef); // Mã đơn hàng

            // Tạo URL thanh toán
            var paymentUrl = vnpay.CreateRequestUrl(_config["VnPay:BaseUrl"], _config["VnPay:HashSecret"]);
            return paymentUrl;
        }

        public VnPaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            var vnpayData = new VnPayLibrary();

            // Thêm tất cả các tham số trả về vào đối tượng vnpayData
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpayData.AddResponseData(key, value.ToString());
                }
            }

            // Lấy thông tin từ dữ liệu trả về
            var vnpPremiumPackageId = Convert.ToInt64(vnpayData.GetResponseData("vnp_TxnRef"));
            var vnpTransactionId = Convert.ToInt64(vnpayData.GetResponseData("vnp_TransactionNo"));
            var vnpSecureHash = collections.FirstOrDefault(p => p.Key == "vnp_SecureHash").Value;
            var vnpResponseCode = vnpayData.GetResponseData("vnp_ResponseCode");
            var vnpOrderInfo = vnpayData.GetResponseData("vnp_OrderInfo");
            bool isValid = vnpayData.ValidateSignature(vnpSecureHash, _config["VnPay:HashSecret"]);

            // Kiểm tra tính hợp lệ của chữ ký
            if (!isValid)
            {
                return new VnPaymentResponseModel
                {
                    Success = false
                };
            }

            // Trả về kết quả thanh toán
            return new VnPaymentResponseModel
            {
                Success = true,
                PaymentMethod = "VnPay",
                OrderDescription = vnpOrderInfo,
                PremiumPackageId = vnpPremiumPackageId.ToString(),
                TransactionId = vnpTransactionId.ToString(),
                Token = vnpSecureHash,
                VnPayResponseCode = vnpResponseCode
            };
        }
    }
}
