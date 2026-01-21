using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using MegaMall.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;

namespace MegaMall.Controllers
{
    public class SupportController : Controller
    {
        private readonly MallDbContext _context;

        public SupportController(MallDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            // Simulate AI processing time
            await Task.Delay(100); 

            var msg = request.Message?.ToLower() ?? "";
            string response = "";

            // Define allowed topics (only e-commerce related)
            var allowedTopics = new[] {
                // Greetings
                "chào", "hello", "hi", "xin chào", "hey",
                // Product related
                "sản phẩm", "hàng", "mua", "có", "tìm", "giá", "bán", 
                "điện thoại", "laptop", "máy tính", "tai nghe", "tivi", "đồng hồ",
                "quần", "áo", "giày", "túi", "phụ kiện", "mỹ phẩm", "skincare",
                "đồ gia dụng", "nội thất", "nhà cửa", "sách", "văn phòng phẩm",
                "thể thao", "game", "đồ chơi", "em bé", "mẹ bầu", "điện máy",
                "rẻ", "đắt", "khuyến mãi", "sale", "giảm giá", "mới", "cũ",
                // E-commerce policies
                "bảo hành", "giao hàng", "ship", "vận chuyển", "đổi trả", "hoàn tiền",
                "thanh toán", "payment", "cod", "chuyển khoản", "ví điện tử",
                "liên hệ", "hotline", "số điện thoại", "email", "hỗ trợ",
                "đặt hàng", "order", "mua hàng", "giỏ hàng", "cart", "checkout",
                "tài khoản", "đăng ký", "đăng nhập", "quên mật khẩu",
                // Common words
                "như thế nào", "bao lâu", "khi nào", "ở đâu", "nào", "không"
            };

            // Off-topic keywords (questions not related to shopping)
            var offTopicIndicators = new[] {
                "thời tiết", "bóng đá", "world cup", "tin tức", "chính trị", "kinh tế vĩ mô",
                "lịch sử", "địa lý", "toán học", "vật lý", "hóa học", "sinh học",
                "nấu ăn", "công thức", "recipe", "du lịch", "khách sạn", "máy bay",
                "âm nhạc", "ca sĩ", "diễn viên", "phim", "movie", "bài hát",
                "tình yêu", "hẹn hò", "date", "relationship", "sức khỏe", "bệnh",
                "luật pháp", "pháp luật", "thuế", "ngân hàng interest rate",
                "chứng khoán", "bitcoin", "cryptocurrency", "forex",
                "lập trình", "coding", "python", "java", "javascript",
                "ai là", "who is", "khi nào sinh", "bao nhiêu tuổi",
                "thủ đô", "dân số", "diện tích", "tổng thống", "thủ tướng"
            };

            // Check if question is off-topic
            bool isOffTopic = offTopicIndicators.Any(k => msg.Contains(k));
            
            // If clearly off-topic, reject immediately
            if (isOffTopic)
            {
                response = "😅 <b>Xin lỗi, tôi không được hỗ trợ để trả lời câu hỏi này.</b><br><br>" +
                          "Tôi là trợ lý ảo của <b style='color: #ff6b00;'>MegaMall</b> - chỉ chuyên hỗ trợ về:<br><br>" +
                          "🛍️ <b>Sản phẩm & hàng hóa</b> có bán trên website<br>" +
                          "💰 <b>Giá cả & khuyến mãi</b><br>" +
                          "📦 <b>Đặt hàng, giao hàng, thanh toán</b><br>" +
                          "🔄 <b>Chính sách bảo hành, đổi trả</b><br><br>" +
                          "<i>Hãy hỏi tôi về các sản phẩm bạn muốn mua nhé! 😊</i>";
                return Json(new { response = response });
            }

            // Check for specific policy/info keywords first
            var policyKeywords = new[] { 
                "bảo hành", "giao hàng", "ship", "vận chuyển", "đổi trả", "hoàn tiền",
                "khuyến mãi", "sale", "giảm giá", "thanh toán", "payment", 
                "liên hệ", "hotline", "số điện thoại", "email"
            };

            bool isPolicyQuestion = policyKeywords.Any(k => msg.Contains(k));
            bool isGreeting = msg.Contains("chào") || msg.Contains("hello") || msg.Contains("hi") || msg.Contains("xin chào") || msg.Contains("hey");

            // If asking about policy/info, handle those first
            if (isGreeting)
            {
                // Will handle below
            }
            else if (!isPolicyQuestion)
            {
                // Try to search products for ANY query that's not a greeting or policy question
                var query = _context.Products
                    .Where(p => p.IsPublished && !p.IsDeleted)
                    .Include(p => p.Variants)
                    .AsQueryable();

                // Extract meaningful words from the message (remove common words)
                var commonWords = new[] { "có", "không", "gì", "thì", "sao", "như", "thế", "nào", "à", "ạ", "vậy", "đây", "kia", "này", "bao", "nhiêu" };
                var words = msg.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2 && !commonWords.Contains(w))
                    .ToList();

                // Search by any word in the message
                if (words.Any())
                {
                    query = query.Where(p => 
                        words.Any(word => 
                            p.Name.ToLower().Contains(word) || 
                            (p.Description != null && p.Description.ToLower().Contains(word))
                        )
                    );
                }

                // Sort logic based on keywords in message
                if (msg.Contains("rẻ") || msg.Contains("giá thấp"))
                {
                    query = query.OrderBy(p => p.Variants.Min(v => v.Price));
                }
                else if (msg.Contains("đắt") || msg.Contains("giá cao") || msg.Contains("cao cấp"))
                {
                    query = query.OrderByDescending(p => p.Variants.Max(v => v.Price));
                }
                else if (msg.Contains("mới"))
                {
                    query = query.OrderByDescending(p => p.CreatedDate);
                }
                else
                {
                    query = query.OrderByDescending(p => p.CreatedDate);
                }

                var products = await query.Take(3).ToListAsync();

                if (products.Any())
                {
                    var productList = products.Select(p => {
                        var minPrice = p.Variants.Any() ? p.Variants.Min(v => v.Price) : 0;
                        return $"<div style='margin: 10px 0; padding: 10px; background: #f8f9fa; border-radius: 8px;'>" +
                               $"• <b><a href='/Product/Details/{p.Id}' target='_blank' style='color: #0066cc; text-decoration: none;'>{p.Name}</a></b>" +
                               $"<br><span style='color: #ff6b00; font-weight: bold;'>Từ {minPrice:N0}đ</span></div>";
                    });
                    
                    var moreText = products.Count < 3 ? "" : "<br><i style='font-size: 0.9em; color: #666;'>💡 Gõ từ khóa cụ thể hơn để tìm thêm sản phẩm khác nhé!</i>";
                    response = $"✅ Chúng tôi có <b>{products.Count} sản phẩm</b> phù hợp:{string.Join("", productList)}{moreText}<br><br>Bạn có muốn xem chi tiết sản phẩm nào không?";
                    return Json(new { response = response });
                }
                // If no products found but seems like product query
                else if (words.Any())
                {
                    response = "😔 <b>Rất tiếc, hiện tại chúng tôi chưa có sản phẩm này.</b><br><br>" +
                              "Bạn có thể:<br>" +
                              "🔍 Thử tìm với từ khóa khác<br>" +
                              "📧 Để lại thông tin, chúng tôi sẽ thông báo khi có hàng<br>" +
                              "🛍️ <a href='/' style='color: #ff6b00;'>Xem các sản phẩm khác tại đây</a><br><br>" +
                              "<i>Hoặc hỏi: \"Có những sản phẩm gì?\" để xem danh mục! 😊</i>";
                    return Json(new { response = response });
                }
            }

            // Handle greetings
            if (isGreeting)
            {
                response = "👋 <b>Xin chào! Rất vui được hỗ trợ bạn!</b><br><br>" +
                          "Tôi là <b style='color: #ff6b00;'>MegaMall AI</b> - trợ lý ảo thông minh. Tôi có thể giúp bạn:<br><br>" +
                          "🔍 <b>Tìm kiếm sản phẩm</b><br>" +
                          "💰 <b>Tư vấn giá cả & khuyến mãi</b><br>" +
                          "🛒 <b>Hỗ trợ đặt hàng</b><br>" +
                          "📋 <b>Giải đáp chính sách</b> (bảo hành, đổi trả, giao hàng)<br><br>" +
                          "<i style='color: #666;'>Hãy hỏi tôi bất cứ điều gì về mua sắm tại MegaMall nhé! 💬</i>";
            }
            else if (msg.Contains("bảo hành"))
            {
                response = "✅ <b>Chính sách bảo hành tại MegaMall:</b><br><br>" +
                          "• 📱 <b>Điện thoại, Laptop:</b> 12-24 tháng bảo hành chính hãng<br>" +
                          "• 🎧 <b>Phụ kiện điện tử:</b> 6-12 tháng<br>" +
                          "• 👕 <b>Thời trang:</b> Đổi size miễn phí trong 30 ngày<br>" +
                          "• 🏠 <b>Đồ gia dụng:</b> Theo chính sách nhà sản xuất<br><br>" +
                          "<i>💡 Lưu ý: Giữ hóa đơn và tem bảo hành để được hỗ trợ tốt nhất!</i>";
            }
            else if (msg.Contains("giao") || msg.Contains("ship") || msg.Contains("vận chuyển"))
            {
                response = "🚚 <b>Chính sách giao hàng MegaMall:</b><br><br>" +
                          "• 🏙️ <b>Nội thành:</b> Giao trong 1-2 ngày<br>" +
                          "• 🌆 <b>Ngoại thành:</b> Giao trong 2-4 ngày<br>" +
                          "• 🎁 <b>Miễn phí ship:</b> Đơn hàng từ 500.000đ<br>" +
                          "• 💵 <b>COD:</b> Thanh toán khi nhận hàng toàn quốc<br><br>" +
                          "<i>📦 Đóng gói cẩn thận, bảo đảm hàng nguyên vẹn!</i>";
            }
            else if (msg.Contains("đổi") || msg.Contains("trả") || msg.Contains("hoàn"))
            {
                response = "🔄 <b>Chính sách đổi trả hàng:</b><br><br>" +
                          "• ⏰ <b>Thời gian:</b> Đổi trả miễn phí trong 7 ngày<br>" +
                          "• ✔️ <b>Điều kiện:</b> Sản phẩm còn nguyên tem, mác, hóa đơn<br>" +
                          "• 💰 <b>Hoàn tiền:</b> 100% nếu lỗi nhà sản xuất<br>" +
                          "• 🔧 <b>Bảo hành:</b> Sửa chữa hoặc đổi mới nếu có lỗi<br><br>" +
                          "<i>📞 Liên hệ hotline để được hỗ trợ nhanh chóng!</i>";
            }
            else if (msg.Contains("khuyến mãi") || msg.Contains("sale") || msg.Contains("giảm giá"))
            {
                response = "🔥 <b>Khuyến mãi HOT hiện tại:</b><br><br>" +
                          "• ⚡ <b>Flash Sale:</b> Mỗi ngày lúc 9h, 12h, 18h, 21h<br>" +
                          "• 🎯 <b>Giảm giá:</b> Lên đến 50% nhiều sản phẩm<br>" +
                          "• 🚚 <b>Freeship:</b> Miễn phí vận chuyển mọi đơn<br>" +
                          "• 🎁 <b>Tích điểm:</b> Đổi quà, voucher hấp dẫn<br><br>" +
                          "<a href='/' style='color: #ff6b00; font-weight: bold;'>👉 Xem ngay các deal hot!</a>";
            }
            else if (msg.Contains("thanh toán") || msg.Contains("payment"))
            {
                response = "💳 <b>Hỗ trợ các hình thức thanh toán:</b><br><br>" +
                          "• 💵 <b>Tiền mặt (COD):</b> Thanh toán khi nhận hàng<br>" +
                          "• 🏦 <b>Chuyển khoản:</b> Qua ngân hàng (có hướng dẫn)<br>" +
                          "• 📱 <b>Ví điện tử:</b> MoMo, ZaloPay, VNPay<br>" +
                          "• 💳 <b>Thẻ:</b> Visa, Mastercard, JCB<br><br>" +
                          "<i>🔒 Giao dịch an toàn, bảo mật 100%!</i>";
            }
            else if (msg.Contains("liên hệ") || msg.Contains("hotline") || msg.Contains("số điện thoại"))
            {
                response = "📞 <b>Thông tin liên hệ MegaMall:</b><br><br>" +
                          "• ☎️ <b>Hotline:</b> 1900-3003 (8h-22h hàng ngày)<br>" +
                          "• 📧 <b>Email:</b> support@megamall.vn<br>" +
                          "• 💬 <b>Chat:</b> Trực tuyến 24/7 (như bây giờ đây!)<br>" +
                          "• 📍 <b>Địa chỉ:</b> Tầng 18, Toà UOA, Tân Trào, TP.HCM<br><br>" +
                          "<i>Chúng tôi luôn sẵn sàng hỗ trợ bạn! 😊</i>";
            }
            else
            {
                // If message is too vague or off-topic, reject politely
                response = "😅 <b>Xin lỗi, tôi không được hỗ trợ để trả lời câu hỏi này.</b><br><br>" +
                          "Tôi là trợ lý ảo của <b style='color: #ff6b00;'>MegaMall</b> - chỉ chuyên hỗ trợ về:<br><br>" +
                          "🛍️ <b>Tìm kiếm & tư vấn sản phẩm</b><br>" +
                          "💰 <b>Giá cả & khuyến mãi</b><br>" +
                          "📦 <b>Đặt hàng & giao hàng</b><br>" +
                          "🔄 <b>Bảo hành & đổi trả</b><br><br>" +
                          "Ví dụ các câu hỏi tôi có thể trả lời:<br>" +
                          "💬 <i>\"Có điện thoại Samsung không?\"</i><br>" +
                          "💬 <i>\"Laptop giá rẻ nhất\"</i><br>" +
                          "💬 <i>\"Chính sách giao hàng?\"</i><br>" +
                          "💬 <i>\"Đổi trả trong bao lâu?\"</i><br><br>" +
                          "<b>Hãy hỏi tôi về các sản phẩm bạn muốn mua nhé! 😊</b>";
            }

            return Json(new { response = response });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
