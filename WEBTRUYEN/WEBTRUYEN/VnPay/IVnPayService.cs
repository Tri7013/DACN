using WEBTRUYEN.Models;

namespace WEBTRUYEN.VnPay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, VnPaymentRequestModel model);
        VnPaymentResponseModel PaymentExecute(IQueryCollection query);
        //string CreateSecureHash(Dictionary<string, string> parameters);
    }
}
