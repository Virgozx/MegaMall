using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using MegaMall.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using MegaMall.Interfaces;
using System.Text;

namespace MegaMall.Controllers
{
    public class SupportController : Controller
    {
        private readonly MallDbContext _context;
        private readonly IAIService _aiService;

        public SupportController(MallDbContext context, IAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            var msg = request.Message?.Trim() ?? "";
            if (string.IsNullOrEmpty(msg)) return Json(new { response = "Bạn cần giúp gì không ạ?" });

            // 1. Search for relevant products to provide as context
            var query = _context.Products
                .Include(p => p.Variants)
                .AsQueryable();

            // Simple keyword search to filter context
            var keywords = msg.ToLower().Split(new[] { ' ', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 2 && !new[] { "cái", "thì", "là", "của" }.Contains(w))
                .Take(5) // limit keywords
                .ToList();

            if (keywords.Any())
            {
                // Simple search rank
                query = query.Where(p => keywords.Any(k => p.Name.Contains(k) || (p.Description != null && p.Description.Contains(k))));
            }
            
            // Take top 5 relevant products
            var relevantProducts = await query.Take(5).ToListAsync();
            
            // If no specific products found but query is generic, maybe take top 5 newest
            if (!relevantProducts.Any() && (msg.Contains("sản phẩm") || msg.Contains("bán") || msg.Contains("mua")))
            {
                relevantProducts = await _context.Products.Include(p => p.Variants).OrderByDescending(p => p.Id).Take(5).ToListAsync();
            }

            // 2. Build Context String for AI
            var sb = new StringBuilder();
            sb.AppendLine("You are MegaMall AI, a smart and friendly shopping assistant for a website called 'MegaMall'.");
            sb.AppendLine("Your goal is to help users find products, answer questions about prices, and explain store policies.");
            sb.AppendLine("Always answer in Vietnamese (Tiếng Việt). Use HTML formatting (<b>, <br>, <i>, <ul>, <li>) for clarity.");
            sb.AppendLine("Do NOT mention you are an AI model from Google. Speak as the store staff.");
            
            sb.AppendLine("\n--- STORE POLICIES ---");
            sb.AppendLine("- Delivery: 1-2 days inner city, 2-4 days others. Freeship for orders > 500k.");
            sb.AppendLine("- Warranty: 12 months for electronics, 7 days return for errors.");
            sb.AppendLine("- Payment: COD (Cash), VNPay, Bank Transfer.");
            sb.AppendLine("- Support: Hotline 1900-3003.");

            sb.AppendLine("\n--- RELEVANT PRODUCTS IN STOCK ---");
            if (relevantProducts.Any())
            {
                foreach (var p in relevantProducts)
                {
                    var price = p.Variants.Any() ? p.Variants.Min(v => v.Price).ToString("N0") : "Contact";
                    sb.AppendLine($"- ID: {p.Id} | Name: {p.Name} | Price: from {price} VND | Desc: {p.Description?.Substring(0, Math.Min(100, p.Description?.Length ?? 0))}...");
                }
            }
            else
            {
                sb.AppendLine("(No specific products found matching the user query in database)");
            }

            sb.AppendLine("\n--- USER QUERY ---");
            sb.AppendLine(msg);

            sb.AppendLine("\n--- INSTRUCTIONS ---");
            sb.AppendLine("1. If the user asks for a product, recommend from the list above. Include links like <a href='/Product/Details/{id}'>{name}</a>.");
            sb.AppendLine("2. If the user asks about policy, answer based on the STORE POLICIES section.");
            sb.AppendLine("3. If the user asks something off-topic (weather, politics), politely refuse and guide back to shopping.");
            sb.AppendLine("4. Be enthusiastic and professional. Emojis are allowed.");
            
            // 3. Call AI
            string aiResponse = await _aiService.GenerateTextAsync(sb.ToString(), maxTokens: 800, temperature: 0.7);

            if (string.IsNullOrEmpty(aiResponse))
            {
                // Fallback if AI fails
                aiResponse = "Xin lỗi, hệ thống đang bận. Vui lòng thử lại sau hoặc liên hệ Hotline 1900-3003.";
            }

            return Json(new { response = aiResponse });
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
    }
}
