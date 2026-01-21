using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace VNPAY
{
    public interface IVnpayClient
    {
        PaymentUrlResult CreatePaymentUrl(decimal amount, string orderInfo);
        PaymentResponse GetPaymentResponse(IQueryCollection query);
    }

    public class PaymentUrlResult
    {
        public string Url { get; set; }
    }

    public class PaymentResponse
    {
        public bool Success { get; set; }
        public string OrderInfo { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public string Message { get; set; }
    }

    public class VnpayService : IVnpayClient
    {
        private readonly IConfiguration _configuration;
        private readonly string _vnpUrl;
        private readonly string _vnpTmnCode;
        private readonly string _vnpHashSecret;
        private readonly string _returnUrl;

        public VnpayService(IConfiguration configuration)
        {
            _configuration = configuration;
            _vnpUrl = (configuration["VNPay:Url"] ?? "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html").Trim();
            _vnpTmnCode = (configuration["VNPay:TmnCode"] ?? "VNPAY_DEMO").Trim();
            _vnpHashSecret = (configuration["VNPay:HashSecret"] ?? "DEMO_SECRET_KEY").Trim();
            _returnUrl = (configuration["VNPay:ReturnUrl"] ?? "https://localhost:5001/Cart/PaymentCallback").Trim();
        }

        public PaymentUrlResult CreatePaymentUrl(decimal amount, string orderInfo)
        {
            var vnpay = new VnpayLibrary();
            
            vnpay.AddRequestData("vnp_Version", "2.1.0");
            vnpay.AddRequestData("vnp_Command", "pay");
            vnpay.AddRequestData("vnp_TmnCode", _vnpTmnCode);
            vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
            vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", "VND");
            vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
            vnpay.AddRequestData("vnp_Locale", "vn");
            vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
            vnpay.AddRequestData("vnp_OrderType", "other");
            vnpay.AddRequestData("vnp_ReturnUrl", _returnUrl);
            vnpay.AddRequestData("vnp_TxnRef", DateTime.Now.Ticks.ToString());

            string paymentUrl = vnpay.CreateRequestUrl(_vnpUrl, _vnpHashSecret);
            
            Console.WriteLine($"[VNPay] Creating payment URL for amount: {amount}, orderInfo: {orderInfo}");
            Console.WriteLine($"[VNPay] Payment URL: {paymentUrl}");

            return new PaymentUrlResult { Url = paymentUrl };
        }

        public PaymentResponse GetPaymentResponse(IQueryCollection query)
        {
            var vnpay = new VnpayLibrary();
            
            foreach (var (key, value) in query)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    vnpay.AddResponseData(key, value);
                }
            }

            string vnpSecureHash = query["vnp_SecureHash"];
            bool checkSignature = vnpay.ValidateSignature(vnpSecureHash, _vnpHashSecret);

            if (checkSignature)
            {
                string responseCode = vnpay.GetResponseData("vnp_ResponseCode");
                string transactionId = vnpay.GetResponseData("vnp_TransactionNo");
                string orderInfo = vnpay.GetResponseData("vnp_OrderInfo");
                string amountStr = vnpay.GetResponseData("vnp_Amount");
                
                decimal amount = 0;
                if (!string.IsNullOrEmpty(amountStr) && long.TryParse(amountStr, out long amountLong))
                {
                    amount = amountLong / 100m;
                }

                return new PaymentResponse
                {
                    Success = responseCode == "00",
                    OrderInfo = orderInfo,
                    Amount = amount,
                    TransactionId = transactionId,
                    Message = responseCode == "00" ? "Giao dịch thành công" : "Giao dịch thất bại"
                };
            }

            return new PaymentResponse
            {
                Success = false,
                Message = "Chữ ký không hợp lệ"
            };
        }
    }

    public class VnpayLibrary
    {
        private readonly SortedList<string, string> _requestData = new SortedList<string, string>();
        private readonly SortedList<string, string> _responseData = new SortedList<string, string>();

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _requestData[key] = value;
            }
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                _responseData[key] = value;
            }
        }

        public string GetResponseData(string key)
        {
            return _responseData.TryGetValue(key, out string value) ? value : string.Empty;
        }

        public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
        {
            var data = new StringBuilder();
            
            foreach (var kv in _requestData)
            {
                if (!string.IsNullOrEmpty(kv.Value))
                {
                    data.Append(Uri.EscapeDataString(kv.Key));
                    data.Append('=');
                    data.Append(Uri.EscapeDataString(kv.Value));
                    data.Append('&');
                }
            }

            string queryString = data.ToString();
            if (queryString.EndsWith("&"))
            {
                queryString = queryString.Substring(0, queryString.Length - 1);
            }

            string signData = queryString;
            string vnpSecureHash = HmacSHA512(vnpHashSecret, signData);
            
            return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
        }

        public bool ValidateSignature(string inputHash, string secretKey)
        {
            var data = new StringBuilder();
            
            foreach (var kv in _responseData)
            {
                if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
                {
                    data.Append(Uri.EscapeDataString(kv.Key));
                    data.Append('=');
                    data.Append(Uri.EscapeDataString(kv.Value));
                    data.Append('&');
                }
            }

            string signData = data.ToString();
            if (signData.EndsWith("&"))
            {
                signData = signData.Substring(0, signData.Length - 1);
            }

            string myChecksum = HmacSHA512(secretKey, signData);
            return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        private string HmacSHA512(string key, string inputData)
        {
            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var b in hashValue)
                {
                    hash.Append(b.ToString("x2"));
                }
            }

            return hash.ToString();
        }
    }
}
