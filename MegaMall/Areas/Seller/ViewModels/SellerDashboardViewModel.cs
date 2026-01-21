using MegaMall.Domain.Entities;

namespace MegaMall.Areas.Seller.ViewModels
{
    public class SellerDashboardViewModel
    {
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<OrderItem> RecentOrderItems { get; set; }
    }
}
