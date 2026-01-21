---
# 1. Mục tiêu dự án
- Xây dựng nền tảng marketplace mô phỏng Shopee.
- 3 role: **Admin**, **Seller**, **Buyer**.
- Yêu cầu: linh hoạt mở rộng, modular, rõ trách nhiệm lớp, có thể triển khai cloud.
- Dùng kiến trúc MVC chuẩn ASP.NET Core nhưng tài liệu này mô tả **từ góc nhìn kiến trúc**, không phải code.

---
# 2. Kiến trúc hệ thống
## 2.1 Kiến trúc lớp
- **Presentation Layer (MVC)**: Razor Views + Controllers + ViewModels
- **Application Layer (Services)**: xử lý logic nghiệp vụ, gồm các service như ProductService, OrderService, AuthService.
- **Domain Layer**: mô hình domain thuần (Entities + Rules)
- **Infrastructure Layer**: Database, Email sender, Payment adapter, File storage
- **Cross-cutting Layer**: Logging, Validation, Mapping

## 2.2 Sơ đồ module (mô tả)
- **Module Users** → quản lý identity, roles, seller profile.
- **Module Products** → danh mục sản phẩm, attributes, images, search, filters.
- **Module Orders** → cart, order, state machine, shipping.
- **Module Payments** → mô phỏng payment gateway, callback.
- **Module Reviews** → rating, comments, moderation.
- **Module Chat** → Buyer/Seller chat, lưu tin nhắn.
- **Module Admin** → phê duyệt seller, quản trị nội dung, thống kê.

---
# 3. Thiết kế Domain chi tiết
## 3.1 Quy tắc đặt tên
- Entities PascalCase
- Thuộc tính viết PascalCase
- Enum đặt hậu tố *Status*, *Role*, *Type*
- Tên quan hệ rõ ràng: `BuyerId`, `SellerId`

## 3.2 Danh sách Entities (mở rộng sâu)
### User
- Vai trò: Admin, Seller, Buyer
- Thuộc tính:
  - Email, Username, PhoneNumber
  - EmailConfirmed, IsSellerApproved
  - Security fields (không mô tả code)
- Ràng buộc:
  - Email unique
  - Seller phải được Admin duyệt mới đăng sản phẩm

### SellerProfile
- ShopName (duy nhất), ShopDescription
- Trạng thái: Pending, Approved, Rejected, Suspended
- RatingAverage tự động cập nhật từ Review
- Quan hệ 1-N với Product

### Category (đa cấp)
- Danh mục cha-con không giới hạn
- Có `DisplayOrder` để sắp xếp menu
- Slug unique

### Product (mở rộng 50+ thuộc tính nếu cần)
- Cần mô tả rõ loại dữ liệu cho từng trường:
  - Title: text
  - Description: rich-text, có quy tắc sanitize
  - Pricing: price, original price, discount logic
  - Visibility: IsActive, IsPublished
  - StockQuantity: Số lượng khả dụng
  - SEO fields: MetaTitle, MetaDescription, MetaKeywords
- Ràng buộc:
  - Seller phải được duyệt
  - Category phải tồn tại
  - Không xóa cứng (soft delete)

### ProductAttributes
- Kiểu dữ liệu key/value linh hoạt
- Lưu dưới dạng JSON hoặc bảng quan hệ
- Dùng để filter (color, size, material...)

### ProductImage
- Lưu thứ tự, đánh dấu ảnh chính
- Chuẩn đặt tên file: `sellerId/productId/timestamp_hash.ext`

### Cart & CartItem
- Một Buyer có 1 cart
- Cart không lưu giá động — giá snapshot lưu trong OrderItem

### Order & OrderItem
- OrderNumber chuẩn hóa: `PREFIX-YYYYMMDD-Sequence`
- OrderItem lưu snapshot giá, tên sản phẩm, SKU tại thời điểm đặt
- Order state machine (chi tiết ở phần sau)

### Review
- Rating (1–5)
- Comment text
- Moderation fields: IsVisible, IsFlagged, RejectReason
- Review chỉ hợp lệ nếu Buyer mua sản phẩm

---
# 4. Quy trình nghiệp vụ (Business Flows)
## 4.1 Seller đăng ký → được duyệt → tạo sản phẩm
1. Seller đăng ký → trạng thái SellerProfile = Pending
2. Admin duyệt → SellerProfile = Approved
3. Seller vào Dashboard → tạo sản phẩm
4. Sản phẩm IsPublished=false cho đến khi seller bật

## 4.2 Buyer tìm kiếm & mua hàng
1. Buyer search theo từ khóa
2. Hệ thống áp dụng full-text + scoring
3. Buyer xem chi tiết sản phẩm
4. Thêm vào giỏ → Giỏ cập nhật số lượng
5. Checkout → tạo Order (PendingPayment)
6. PaymentCallback → Order chuyển sang Processing
7. Seller ship → Shipped
8. Buyer confirm → Completed
9. Buyer Review sản phẩm

## 4.3 Admin quản lý tranh chấp
- Buyer/Seller mở ticket
- Admin xem lịch sử giao dịch + logs
- Quyết định refund, penalty seller

---
# 5. Order State Machine (chi tiết đặc tả chuyển trạng thái)
### Trạng thái chính:
- Created
- PendingPayment
- Paid
- Processing
- Shipped
- Delivered
- Completed
- Cancelled
- Refunded

### Chuyển trạng thái hợp lệ:
| From | To | Điều kiện |
|------|------|-----------|
| Created | PendingPayment | Buyer checkout |
| PendingPayment | Paid | Payment gateway notify success |
| PendingPayment | Cancelled | Buyer hủy trước khi thanh toán |
| Paid | Processing | Seller xác nhận |
| Processing | Shipped | Seller ship + nhập tracking number |
| Shipped | Delivered | Bên vận chuyển báo giao thành công |
| Delivered | Completed | Buyer confirm |
| Paid/Processing | Cancelled | Admin can force cancel |
| Completed | Refunded | Admin manual action |

### Log trạng thái
- Mỗi lần chuyển tạo record history: From, To, Actor, Timestamp, Notes

---
# 6. Search Engine (mô tả logic)
### Input xử lý
- Query text
- Bộ lọc category
- Thanh price slider
- Thuộc tính (color, size…)
- Sorting: relevance, newest, price ASC/DESC, popularity

### Logic tính relevance
- Weight factors:
  - Title match = 0.6
  - Description match = 0.2
  - Category match = 0.1
  - Seller rating = 0.1

### Data indexing
- Full-text index cho Title + Description
- Index composite: (CategoryId, Price)
- Index popularity: (SoldCount)

---
# 7. Non-functional Requirements (chi tiết)
## 7.1 Performance
- Tải trang sản phẩm < **500ms** ở 90th percentile
- Search < **700ms** cho dataset 1 triệu sản phẩm
- Sử dụng caching theo lớp:
  - Product detail cache: 3 phút
  - Category tree: 30 phút

## 7.2 Scalability
- Web server stateless
- Session store Redis
- CDN cho images
- Queue system cho email, notification

## 7.3 Reliability
- Retry policy cho email + payment callback
- Circuit breaker cho external services

## 7.4 Security
- Anti-forgery token trong form
- HTML sanitize cho mô tả sản phẩm
- Rate limit login
- Lockout 5 attempts
- Validation toàn bộ input user

---
# 8. File Storage Specification
- Folder structure chuẩn:
```
/uploads/
  /sellerId/
    /productId/
      main.jpg
      img_1.webp
      img_2.webp
```
- Quy định kích thước ảnh:
  - Thumbnail: 64×64
  - Small: 320×320
  - Large: 1024×1024

- Chính sách loại file: chỉ PNG, JPEG, WEBP

---
# 9. Event System (mô tả không code)
## Events chính
- UserRegistered
- SellerApproved
- ProductCreated
- OrderPlaced
- PaymentSuccess
- OrderShipped
- ReviewSubmitted

## Payload chuẩn (mô tả)
Ví dụ OrderPlaced event:
```
event: OrderPlaced
payload:
  orderId
  buyerId
  items[] (productId, qty, price)
  totalAmount
  timestamp
```

---
# 10. Logging Specification
Ghi log theo dạng structured:
- RequestId
- Actor (UserId)
- Action
- Entity affected
- Previous state / Next state
- Metadata: IP, device

Ví dụ action log: "ORDER_STATUS_CHANGED"

---
# 11. Monitoring + Metrics
- Số lượng đơn mỗi giờ
- Tỉ lệ thanh toán thành công
- Tỉ lệ seller bị suspended
- Số review bị flag
- P95 response time

---
# 12. Test Matrix cấp hệ thống
Danh sách các nhóm test cần có nhưng không sinh code:
- Authentication
- Authorization
- Seller onboarding
- Product creation
- Search engine accuracy
- Checkout flow
- Payment simulation
- Order state transitions
- Review rules
- Admin moderation
- Stress test concurrency
- XSS + injection tests

---
# 13. Checklist bắt buộc để AI agent triển khai dự án
- Tạo đầy đủ folder theo spec
- Sinh tất cả đối tượng domain đúng thuộc tính
- Ánh xạ tất cả quan hệ như mô tả
- Tạo các tài liệu swagger mô tả endpoint (nhưng không viết code)
- Tạo validators dựa trên rules của spec
- Tạo tài liệu seed data theo cấu trúc mô tả
- Không được tạo bất kỳ dòng code logic ứng dụng

---
**Kết thúc SPEC.md**