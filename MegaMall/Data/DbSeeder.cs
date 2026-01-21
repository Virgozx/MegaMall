using MegaMall.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MegaMall.Data
{
    public static class DbSeeder
    {
        private static readonly Dictionary<string, List<string>> CategoryImages = new()
        {
            { "Bất động sản", new List<string> {
                "https://images.unsplash.com/photo-1564013799919-ab600027ffc6?w=500",
                "https://images.unsplash.com/photo-1570129477492-45c003edd2be?w=500",
                "https://images.unsplash.com/photo-1580587771525-78b9dba3b91d?w=500",
                "https://images.unsplash.com/photo-1600596542815-27b5c0c8aa2b?w=500"
            }},
            { "Xe cộ", new List<string> {
                "https://images.unsplash.com/photo-1533473359331-0135ef1b58bf?w=500",
                "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=500",
                "https://images.unsplash.com/photo-1503376763036-066120622c74?w=500",
                "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?w=500"
            }},
            { "Thú cưng", new List<string> {
                "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=500",
                "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=500",
                "https://images.unsplash.com/photo-1583511655857-d19b40a7a54e?w=500",
                "https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?w=500"
            }},
            { "Đồ gia dụng, nội thất, cây cảnh", new List<string> {
                "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=500",
                "https://images.unsplash.com/photo-1524758631624-e2822e304c36?w=500",
                "https://images.unsplash.com/photo-1586023492125-27b2c045efd7?w=500",
                "https://images.unsplash.com/photo-1616486338812-3dadae4b4f9d?w=500"
            }},
            { "Giải trí, Thể thao, Sở thích", new List<string> {
                "https://images.unsplash.com/photo-1517649763962-0c623066013b?w=500",
                "https://images.unsplash.com/photo-1526628953301-3e589a6a8b74?w=500",
                "https://images.unsplash.com/photo-1511512578047-dfb367046420?w=500"
            }},
            { "Mẹ và bé", new List<string> {
                "https://images.unsplash.com/photo-1519689680058-324335c77eba?w=500",
                "https://images.unsplash.com/photo-1555252333-9f8e92e65df9?w=500",
                "https://images.unsplash.com/photo-1522771753035-a15806bb13ad?w=500"
            }},
            { "Dịch vụ, Du lịch", new List<string> {
                "https://images.unsplash.com/photo-1476514525535-07fb3b4ae5f1?w=500",
                "https://images.unsplash.com/photo-1488646953014-85cb44e25828?w=500",
                "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=500"
            }},
            { "Đồ điện tử", new List<string> {
                "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500",
                "https://images.unsplash.com/photo-1550009158-9ebf69173e03?w=500",
                "https://images.unsplash.com/photo-1526738549149-8e07eca6c147?w=500",
                "https://images.unsplash.com/photo-1546868871-7041f2a55e12?w=500"
            }},
            { "Tủ lạnh, máy lạnh, máy giặt", new List<string> {
                "https://images.unsplash.com/photo-1571175443880-49e1d58b794a?w=500",
                "https://images.unsplash.com/photo-1584622050111-993a426fbf0a?w=500",
                "https://images.unsplash.com/photo-1626806819282-2c1dc01a5e0c?w=500"
            }},
            { "Thời trang, Đồ dùng cá nhân", new List<string> {
                "https://images.unsplash.com/photo-1523381210434-271e8be1f52b?w=500",
                "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=500",
                "https://images.unsplash.com/photo-1576566588028-4147f3842f27?w=500",
                "https://images.unsplash.com/photo-1483985988355-763728e1935b?w=500"
            }},
            { "Đồ ăn, thực phẩm và các loại khác", new List<string> {
                "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=500",
                "https://images.unsplash.com/photo-1565299624946-b28f40a0ae38?w=500",
                "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=500"
            }},
            { "Đồ dùng văn phòng, công nông nghiệp", new List<string> {
                "https://images.unsplash.com/photo-1497215728101-856f4ea42174?w=500",
                "https://images.unsplash.com/photo-1625246333195-78d9c38ad449?w=500"
            }},
            { "Việc làm", new List<string> {
                "https://images.unsplash.com/photo-1486312338219-ce68d2c6f44d?w=500",
                "https://images.unsplash.com/photo-1521737604893-d14cc237f11d?w=500"
            }},
            { "Dịch vụ chăm sóc nhà cửa", new List<string> {
                "https://images.unsplash.com/photo-1581578731117-104f2a863a30?w=500",
                "https://images.unsplash.com/photo-1528740561666-dc24705f08a7?w=500"
            }},
            { "Cho tặng miễn phí", new List<string> {
                "https://images.unsplash.com/photo-1513885535751-8b9238bd345a?w=500"
            }}
        };

        public static async Task SeedAsync(MallDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // 1. Seed Roles
            if (!await roleManager.RoleExistsAsync("Admin")) await roleManager.CreateAsync(new IdentityRole("Admin"));
            if (!await roleManager.RoleExistsAsync("Seller")) await roleManager.CreateAsync(new IdentityRole("Seller"));
            if (!await roleManager.RoleExistsAsync("Buyer")) await roleManager.CreateAsync(new IdentityRole("Buyer"));

            // 2. Seed Admin
            var adminEmail = "admin@megamall.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    ShopName = "Admin",
                    ShopDescription = "Admin",
                    IsSellerApproved = true,
                    EmailConfirmed = true,
                    Address = "Admin HQ",
                    City = "Admin City"
                };
                await userManager.CreateAsync(adminUser, "Admin@123!");
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 3. Seed 10 Sellers
            var sellers = new List<ApplicationUser>();
            for (int i = 1; i <= 10; i++)
            {
                var email = $"seller{i}@megamall.com";
                var seller = await userManager.FindByEmailAsync(email);
                if (seller == null)
                {
                    seller = new ApplicationUser
                    {
                        UserName = email,
                        Email = email,
                        FullName = $"Seller {i}",
                        ShopName = $"Shop {i} Official",
                        ShopDescription = $"Official store of Seller {i}",
                        IsSellerApproved = true,
                        EmailConfirmed = true,
                        Address = $"{i} Market Street",
                        City = "Hanoi"
                    };
                    await userManager.CreateAsync(seller, "P@ssword123!");
                    await userManager.AddToRoleAsync(seller, "Seller");
                }
                sellers.Add(seller);
            }

            // 4. Seed Categories (Synced with Index.cshtml)
            var categoryNames = new[]
            {
                "Bất động sản", "Xe cộ", "Thú cưng", "Đồ gia dụng, nội thất, cây cảnh",
                "Giải trí, Thể thao, Sở thích", "Mẹ và bé", "Dịch vụ, Du lịch", "Cho tặng miễn phí",
                "Việc làm", "Đồ điện tử", "Tủ lạnh, máy lạnh, máy giặt", "Đồ dùng văn phòng, công nông nghiệp",
                "Thời trang, Đồ dùng cá nhân", "Đồ ăn, thực phẩm và các loại khác", "Dịch vụ chăm sóc nhà cửa"
            };

            foreach (var name in categoryNames)
            {
                if (!context.Categories.Any(c => c.Name == name))
                {
                    context.Categories.Add(new Category { Name = name, Description = name });
                }
            }
            await context.SaveChangesAsync();

            // 5. Seed Products (Only if no products exist)
            // Skip seeding if products already exist to preserve user data
            if (context.Products.Any())
            {
                return; // Exit early - don't reseed if products already exist
            }

            var categories = await context.Categories.ToListAsync();
            var demoSeller = sellers.First(); // Assign all to first seller for simplicity

            var newProductsData = new[] 
            {
                new { Name = "Lumina X1 Pro Smartphone", Description = "Trải nghiệm công nghệ đỉnh cao với màn hình OLED 120Hz và camera 108MP. Thiết kế sang trọng, hiệu năng mạnh mẽ cho mọi tác vụ.", Category = "Đồ điện tử", Price = 15990000m, ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=500" },
                new { Name = "Vesta Modern Sofa Grey", Description = "Sofa nỉ cao cấp phong cách Bắc Âu, mang lại vẻ đẹp hiện đại cho phòng khách của bạn. Đệm mút êm ái, khung gỗ sồi chắc chắn.", Category = "Đồ gia dụng, nội thất, cây cảnh", Price = 8500000m, ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=500" },
                new { Name = "AeroSwift Sport Bike 2025", Description = "Xe đạp thể thao địa hình với khung hợp kim nhôm siêu nhẹ. Hệ thống truyền động Shimano 21 tốc độ, phanh đĩa an toàn.", Category = "Giải trí, Thể thao, Sở thích", Price = 4200000m, ImageUrl = "https://images.unsplash.com/photo-1485965120184-e224f7a1d7f6?auto=format&fit=crop&w=800&q=60" },
                new { Name = "BellaVogue Silk Dress", Description = "Đầm lụa thiết kế sang trọng, tôn dáng, phù hợp cho các buổi tiệc tối. Chất liệu lụa tơ tằm mềm mại, thoáng mát.", Category = "Thời trang, Đồ dùng cá nhân", Price = 1250000m, ImageUrl = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?w=500" },
                new { Name = "Purrfect Persian Cat Purebred", Description = "Mèo Ba Tư thuần chủng, lông trắng muốt, tính cách hiền lành, quấn chủ. Đã tiêm phòng đầy đủ, có giấy tờ nguồn gốc.", Category = "Thú cưng", Price = 5500000m, ImageUrl = "https://images.unsplash.com/photo-1543466835-00a7907e9de1?w=500" },
                new { Name = "FrostGuard Inverter Fridge 300L", Description = "Tủ lạnh Inverter tiết kiệm điện, dung tích 300L phù hợp cho gia đình 3-4 người. Công nghệ làm lạnh đa chiều, khử mùi diệt khuẩn.", Category = "Tủ lạnh, máy lạnh, máy giặt", Price = 7800000m, ImageUrl = "https://images.unsplash.com/photo-1584568694244-14fbdf83bd30?auto=format&fit=crop&w=800&q=60" },
                new { Name = "SoundWave Bluetooth Speaker", Description = "Loa Bluetooth chống nước IPX7, âm bass mạnh mẽ, thời lượng pin lên đến 12 giờ. Kết nối ổn định, thiết kế nhỏ gọn dễ mang theo.", Category = "Đồ điện tử", Price = 990000m, ImageUrl = "https://images.unsplash.com/photo-1608043152269-423dbba4e7e1?w=500" },
                new { Name = "BabySafe Comfort Stroller", Description = "Xe đẩy em bé gấp gọn tiện lợi, khung xe chắc chắn, đệm ngồi êm ái. Có mái che nắng rộng, giỏ đựng đồ lớn.", Category = "Mẹ và bé", Price = 2100000m, ImageUrl = "https://images.unsplash.com/photo-1519689680058-324335c77eba?w=500" },
                new { Name = "GreenLeaf Organic Tea Set", Description = "Bộ trà hữu cơ cao cấp, hương vị thanh mát, tốt cho sức khỏe. Bao gồm trà xanh, trà hoa cúc và trà ô long thượng hạng.", Category = "Đồ ăn, thực phẩm và các loại khác", Price = 450000m, ImageUrl = "https://images.unsplash.com/photo-1546069901-ba9599a7e63c?w=500" },
                new { Name = "UrbanChic Denim Jacket", Description = "Áo khoác Jeans phong cách bụi bặm, cá tính. Chất liệu denim bền đẹp, không phai màu, dễ dàng phối đồ.", Category = "Thời trang, Đồ dùng cá nhân", Price = 680000m, ImageUrl = "https://images.unsplash.com/photo-1523381210434-271e8be1f52b?w=500" },
                new { Name = "Laptop Gaming Asus ROG Strix G15", Description = "Chiến mọi tựa game với cấu hình khủng Ryzen 7, RTX 3060. Màn hình 144Hz siêu mượt, tản nhiệt thông minh.", Category = "Đồ điện tử", Price = 28500000m, ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?w=500" },
                new { Name = "Tai nghe Sony WH-1000XM5", Description = "Công nghệ chống ồn hàng đầu thế giới, âm thanh Hi-Res chuẩn studio. Pin trâu 30 giờ, thiết kế đeo thoải mái cả ngày.", Category = "Đồ điện tử", Price = 6990000m, ImageUrl = "https://images.unsplash.com/photo-1618366712010-f4ae9c647dcb?w=500" },
                new { Name = "Máy ảnh Fujifilm X-T5 Body", Description = "Cảm biến 40MP sắc nét, màu phim hoài cổ đặc trưng. Quay video 6K, chống rung 5 trục, hoàn hảo cho nhiếp ảnh gia.", Category = "Đồ điện tử", Price = 41500000m, ImageUrl = "https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=500" },
                new { Name = "Đồng hồ thông minh Apple Watch Series 9", Description = "Chip S9 mạnh mẽ, màn hình sáng gấp đôi. Theo dõi sức khỏe toàn diện, đo oxy trong máu, ECG mọi lúc mọi nơi.", Category = "Đồ điện tử", Price = 10200000m, ImageUrl = "https://images.unsplash.com/photo-1579586337278-3befd40fd17a?w=500" },
                new { Name = "Áo sơ mi nam Aristino Slimfit", Description = "Chất liệu Bamboo sợi tre kháng khuẩn, chống nhăn tự nhiên. Form dáng ôm vừa vặn, lịch lãm cho quý ông công sở.", Category = "Thời trang, Đồ dùng cá nhân", Price = 650000m, ImageUrl = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=500" },
                new { Name = "Giày Sneaker Biti's Hunter X", Description = "Công nghệ đế LiteFlex siêu nhẹ, êm ái từng bước chân. Thiết kế trẻ trung, năng động, phù hợp đi học, đi chơi.", Category = "Thời trang, Đồ dùng cá nhân", Price = 899000m, ImageUrl = "https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=500" },
                new { Name = "Túi xách nữ da thật Vascara", Description = "Da bò cao cấp mềm mịn, đường may tinh tế. Kiểu dáng sang trọng, dễ phối đồ, điểm nhấn hoàn hảo cho phái đẹp.", Category = "Thời trang, Đồ dùng cá nhân", Price = 1450000m, ImageUrl = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?w=500" },
                new { Name = "Kính mát Ray-Ban Aviator", Description = "Biểu tượng thời trang vượt thời gian, bảo vệ mắt 100% khỏi tia UV. Gọng kim loại mạ vàng sang trọng, đẳng cấp.", Category = "Thời trang, Đồ dùng cá nhân", Price = 3200000m, ImageUrl = "https://images.unsplash.com/photo-1572635196237-14b3f281503f?w=500" },
                new { Name = "Bộ nồi inox 5 đáy Sunhouse", Description = "Chất liệu inox 304 an toàn sức khỏe, đáy 5 lớp truyền nhiệt nhanh. Vung kính cường lực, quai cầm chống nóng.", Category = "Đồ gia dụng, nội thất, cây cảnh", Price = 1890000m, ImageUrl = "https://images.unsplash.com/photo-1584992236310-6edddc08acff?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Đèn bàn học chống cận Rạng Đông", Description = "Ánh sáng trung thực, không nhấp nháy, bảo vệ thị lực. Điều chỉnh độ sáng linh hoạt, tích hợp sạc điện thoại.", Category = "Đồ gia dụng, nội thất, cây cảnh", Price = 350000m, ImageUrl = "https://images.unsplash.com/photo-1507473888900-52e1adad5452?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Cây Kim Tiền để bàn phong thủy", Description = "Mang lại tài lộc, may mắn cho gia chủ. Cây xanh tốt, dễ chăm sóc, thanh lọc không khí văn phòng hiệu quả.", Category = "Đồ gia dụng, nội thất, cây cảnh", Price = 150000m, ImageUrl = "https://images.unsplash.com/photo-1599598425947-72e0a2a6b916?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Robot hút bụi Xiaomi Vacuum Mop 2", Description = "Hút bụi lau nhà 2 trong 1, lực hút mạnh mẽ 2700Pa. Điều khiển qua app, tự động quay về dock sạc khi pin yếu.", Category = "Đồ gia dụng, nội thất, cây cảnh", Price = 5490000m, ImageUrl = "https://images.unsplash.com/photo-1518640467707-6811f4a6ab73?w=500" },
                new { Name = "Sữa bột Meiji số 0 Nhật Bản", Description = "Dinh dưỡng tối ưu cho trẻ sơ sinh, hỗ trợ phát triển trí não và chiều cao. Vị nhạt mát giống sữa mẹ, dễ tiêu hóa.", Category = "Mẹ và bé", Price = 550000m, ImageUrl = "https://images.unsplash.com/photo-1565120130276-dfbd9a7a3ad7?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Bỉm tã quần Merries size L", Description = "Siêu mềm mại, thoáng khí, chống hăm hiệu quả. Lưng thun co giãn linh hoạt, giúp bé thoải mái vận động cả ngày.", Category = "Mẹ và bé", Price = 385000m, ImageUrl = "https://images.unsplash.com/photo-1515488042361-ee00e0ddd4e4?w=500" },
                new { Name = "Ghế ăn dặm Hanbei đa năng", Description = "Điều chỉnh độ cao linh hoạt, có bánh xe di chuyển. Khay ăn rộng rãi, dễ vệ sinh, giúp bé rèn luyện thói quen ăn uống.", Category = "Mẹ và bé", Price = 620000m, ImageUrl = "https://images.unsplash.com/photo-1544126566-475d6cf0a4a8?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Máy hút sữa điện đôi Spectra", Description = "Hút êm ái, mô phỏng nhịp bú của bé, kích sữa hiệu quả. Màn hình LCD hiển thị rõ ràng, pin sạc tiện lợi.", Category = "Mẹ và bé", Price = 2850000m, ImageUrl = "https://images.unsplash.com/photo-1555252333-9f8e92e65df9?w=500" },
                new { Name = "Thức ăn hạt cho mèo Royal Canin", Description = "Công thức cân bằng dinh dưỡng, giúp lông bóng mượt, hệ tiêu hóa khỏe mạnh. Hạt nhỏ dễ nhai, hương vị hấp dẫn.", Category = "Thú cưng", Price = 180000m, ImageUrl = "https://images.unsplash.com/photo-1583337130417-3346a1be7dee?w=500" },
                new { Name = "Chuồng chó inox lắp ghép", Description = "Khung inox 304 chắc chắn, không gỉ sét, dễ dàng vệ sinh. Thiết kế rộng rãi, thoáng mát, có khay hứng chất thải.", Category = "Thú cưng", Price = 1200000m, ImageUrl = "https://images.unsplash.com/photo-1591382386627-349b692688ff?w=500" },
                new { Name = "Cát vệ sinh cho mèo Nhật Bản", Description = "Khử mùi cực tốt, vón cục nhanh, không bụi. Hương chanh sả dễ chịu, tiết kiệm chi phí cho sen.", Category = "Thú cưng", Price = 65000m, ImageUrl = "https://images.unsplash.com/photo-1514888286974-6c03e2ca1dba?w=500" },
                new { Name = "Balo vận chuyển thú cưng phi hành gia", Description = "Thiết kế trong suốt độc đáo, thoáng khí với nhiều lỗ thông hơi. Quai đeo chắc chắn, giúp bạn đưa boss đi khắp thế gian.", Category = "Thú cưng", Price = 250000m, ImageUrl = "https://images.unsplash.com/photo-1537151608828-ea2b11777ee8?w=500" },
                new { Name = "Xe máy điện VinFast Klara S", Description = "Thiết kế thanh lịch, vận hành êm ái, thân thiện môi trường. Pin Lithium cao cấp, quãng đường di chuyển 120km/lần sạc.", Category = "Xe cộ", Price = 39900000m, ImageUrl = "https://images.unsplash.com/photo-1558981403-c5f9899a28bc?w=500" },
                new { Name = "Mũ bảo hiểm 3/4 Royal M139", Description = "Kính âm tiện lợi, lót tháo rời vệ sinh dễ dàng. Vỏ nhựa ABS chịu lực tốt, bảo vệ an toàn tối đa.", Category = "Xe cộ", Price = 550000m, ImageUrl = "https://images.unsplash.com/photo-1552519507-da3b142c6e3d?w=500" },
                new { Name = "Camera hành trình Vietmap C61", Description = "Ghi hình Ultra HD 4K sắc nét, cảnh báo giao thông bằng giọng nói. Kết nối Wifi xem video trực tiếp trên điện thoại.", Category = "Xe cộ", Price = 2990000m, ImageUrl = "https://images.unsplash.com/photo-1518458028785-8fbcd101ebb9?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Dầu nhớt Castrol Power1 Ultimate", Description = "Công thức 5 trong 1 giúp động cơ vận hành tối ưu. Tăng tốc vượt trội, bảo vệ động cơ bền bỉ.", Category = "Xe cộ", Price = 120000m, ImageUrl = "https://images.unsplash.com/photo-1568392367068-22f80e94785d?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Vợt cầu lông Yonex Astrox 77", Description = "Công nghệ Namd giúp tăng lực đập cầu, kiểm soát tốt. Khung vợt nhẹ, linh hoạt, phù hợp lối đánh công thủ toàn diện.", Category = "Giải trí, Thể thao, Sở thích", Price = 2600000m, ImageUrl = "https://images.unsplash.com/photo-1626224583764-847649623d1c?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Đàn Guitar Acoustic Yamaha F310", Description = "Âm thanh vang sáng, bấm êm tay, phù hợp cho người mới tập. Gỗ vân sam chất lượng, độ bền cao.", Category = "Giải trí, Thể thao, Sở thích", Price = 3200000m, ImageUrl = "https://images.unsplash.com/photo-1511379938547-c1f69419868d?w=500" },
                new { Name = "Giày bóng đá Nike Mercurial", Description = "Đế đinh bám sân cực tốt, hỗ trợ bứt tốc nhanh. Chất liệu da tổng hợp ôm chân, cảm giác bóng chân thực.", Category = "Giải trí, Thể thao, Sở thích", Price = 1850000m, ImageUrl = "https://images.unsplash.com/photo-1511886929837-354d827aae26?w=500" },
                new { Name = "Bộ màu vẽ Acrylic Mont Marte", Description = "Màu sắc tươi sáng, độ che phủ cao, bền màu theo thời gian. Phù hợp vẽ trên vải, gỗ, tường, canvas.", Category = "Giải trí, Thể thao, Sở thích", Price = 150000m, ImageUrl = "https://images.unsplash.com/photo-1513364776144-60967b0f800f?w=500" },
                new { Name = "Hạt điều rang muối Bình Phước", Description = "Hạt to đều, giòn rụm, vị béo ngậy tự nhiên. Đóng hộp sang trọng, thích hợp làm quà biếu hoặc ăn vặt.", Category = "Đồ ăn, thực phẩm và các loại khác", Price = 250000m, ImageUrl = "https://images.unsplash.com/photo-1536591375315-1988d6960545?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Cà phê Trung Nguyên Legend", Description = "Hương thơm nồng nàn, vị đắng đậm đà chuẩn gu người Việt. Tuyệt phẩm cà phê năng lượng cho ngày mới tỉnh táo.", Category = "Đồ ăn, thực phẩm và các loại khác", Price = 110000m, ImageUrl = "https://images.unsplash.com/photo-1559056199-641a0ac8b55e?w=500" },
                new { Name = "Mật ong hoa cà phê Đắk Lắk", Description = "Mật ong nguyên chất 100%, màu vàng óng, vị ngọt thanh. Tốt cho sức khỏe, hỗ trợ tiêu hóa và làm đẹp da.", Category = "Đồ ăn, thực phẩm và các loại khác", Price = 180000m, ImageUrl = "https://images.unsplash.com/photo-1558642452-9d2a7deb7f62?w=500" },
                new { Name = "Yến sào Khánh Hòa tinh chế", Description = "Tổ yến nguyên chất, sợi dài, nở nhiều khi chưng. Bồi bổ sức khỏe, tăng cường đề kháng cho cả gia đình.", Category = "Đồ ăn, thực phẩm và các loại khác", Price = 3500000m, ImageUrl = "https://images.unsplash.com/photo-1606623927447-226396b023f2?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Ghế văn phòng Ergonomic", Description = "Thiết kế công thái học bảo vệ cột sống, lưới thoáng khí. Tựa đầu, kê tay điều chỉnh linh hoạt, ngồi lâu không mỏi.", Category = "Đồ dùng văn phòng, công nông nghiệp", Price = 2100000m, ImageUrl = "https://images.unsplash.com/photo-1505843490538-5133c6c7d0e1?w=500" },
                new { Name = "Máy in Canon LBP 2900", Description = "Huyền thoại máy in laser, bền bỉ, bản in sắc nét. Tốc độ in nhanh, hộp mực lớn tiết kiệm chi phí.", Category = "Đồ dùng văn phòng, công nông nghiệp", Price = 3800000m, ImageUrl = "https://images.unsplash.com/photo-1612815154858-60aa4c59eaa6?w=500" },
                new { Name = "Phân bón hữu cơ vi sinh", Description = "Cải tạo đất, giúp cây trồng phát triển xanh tốt, bền vững. An toàn cho môi trường và sức khỏe người nông dân.", Category = "Đồ dùng văn phòng, công nông nghiệp", Price = 80000m, ImageUrl = "https://images.unsplash.com/photo-1466692476868-aef1dfb1e735?w=500" },
                new { Name = "Máy cắt cỏ Honda cầm tay", Description = "Động cơ 4 thì mạnh mẽ, tiết kiệm nhiên liệu, ít tiếng ồn. Lưỡi cắt sắc bén, xử lý cỏ dại nhanh chóng.", Category = "Đồ dùng văn phòng, công nông nghiệp", Price = 4500000m, ImageUrl = "https://images.unsplash.com/photo-1592424034827-5d2a7968e395?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Máy giặt lồng ngang Electrolux 9kg", Description = "Công nghệ giặt hơi nước diệt khuẩn, giảm nhăn quần áo. Động cơ Inverter vận hành êm ái, tiết kiệm điện nước.", Category = "Tủ lạnh, máy lạnh, máy giặt", Price = 8990000m, ImageUrl = "https://images.unsplash.com/photo-1604335399105-a0c585fd81a1?w=500" },
                new { Name = "Điều hòa Daikin Inverter 1HP", Description = "Làm lạnh nhanh Coanda, luồng gió dễ chịu. Phin lọc Enzyme Blue khử mùi, diệt khuẩn, bảo vệ sức khỏe.", Category = "Tủ lạnh, máy lạnh, máy giặt", Price = 9500000m, ImageUrl = "https://images.unsplash.com/photo-1545259741-2ea3ebf61fa3?auto=format&fit=crop&w=800&q=60" },
                new { Name = "Tủ đông Sanaky 2 ngăn", Description = "Dung tích 400L, 1 ngăn đông 1 ngăn mát tiện lợi. Dàn lạnh đồng làm lạnh sâu, giữ thực phẩm tươi ngon lâu dài.", Category = "Tủ lạnh, máy lạnh, máy giặt", Price = 5800000m, ImageUrl = "https://images.unsplash.com/photo-1584568694244-14fbdf83bd30?w=500" },
                new { Name = "Máy lọc không khí Sharp", Description = "Công nghệ Plasmacluster Ion diệt khuẩn, lọc bụi mịn PM2.5. Cân bằng độ ẩm, mang lại không gian sống trong lành.", Category = "Tủ lạnh, máy lạnh, máy giặt", Price = 3200000m, ImageUrl = "https://images.unsplash.com/photo-1584622650111-993a426fbf0a?w=500" }
            };

            var products = new List<Product>();
            foreach (var item in newProductsData)
            {
                var category = categories.FirstOrDefault(c => c.Name == item.Category) ?? categories.First();
                
                var product = new Product
                {
                    Name = item.Name,
                    Description = item.Description,
                    AttributesJson = "{\"Origin\": \"Vietnam\", \"Condition\": \"New\"}",
                    IsPublished = true,
                    IsDeleted = false,
                    SellerId = demoSeller.Id,
                    CategoryId = category.Id,
                    CreatedDate = DateTime.Now,
                    Images = new List<ProductImage>
                    {
                        new ProductImage { ImageUrl = item.ImageUrl, IsMain = true }
                    },
                    Variants = new List<ProductVariant>
                    {
                        new ProductVariant
                        {
                            Sku = $"SKU-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}",
                            Price = item.Price,
                            OriginalPrice = item.Price * 1.2m,
                            StockQuantity = 50,
                            VariantPropertiesJson = "{}"
                        }
                    }
                };
                products.Add(product);
            }

            context.Products.AddRange(products);
            await context.SaveChangesAsync();
        }

        private static string GenerateProductName(string category, int index)
        {
            return category switch
            {
                "Bất động sản" => $"Nhà đất vị trí đẹp {index}",
                "Xe cộ" => $"Xe máy Honda/Yamaha cũ {index}",
                "Thú cưng" => $"Chó/Mèo cảnh đáng yêu {index}",
                "Đồ điện tử" => $"Điện thoại iPhone/Samsung {index}",
                "Thời trang, Đồ dùng cá nhân" => $"Quần áo thời trang {index}",
                _ => $"Sản phẩm {category} {index}"
            };
        }

        private static readonly List<string> DefaultImages = new()
        {
            "https://images.unsplash.com/photo-1511556820780-d912e42b4980?w=500",
            "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=500",
            "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=500"
        };

        private static string GetRandomImageUrl(string category, Random random = null)
        {
            random ??= new Random();
            if (category != null && CategoryImages.TryGetValue(category, out var images) && images.Any())
            {
                return images[random.Next(images.Count)];
            }
            return DefaultImages[random.Next(DefaultImages.Count)];
        }
    }
}
