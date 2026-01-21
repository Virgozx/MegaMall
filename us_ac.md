

---
# MỤC LỤC
1. Quy ước đặt mã US  
2. Persona mô tả 3 role  
3. User Stories theo module  
4. Acceptance Criteria siêu chi tiết  
5. Scenarios Gherkin mở rộng  
6. Error Cases (Negative AC)  
7. Data Constraints cho mỗi US  
8. Dependencies  
9. Definition of Done cho từng nhóm US  
10. Test Matrix mapping

---
# 1. Quy ước mã US
- US-xxx: User Story  
- AC-xxx: Acceptance Criteria  
- NEG-xxx: Negative scenarios  
- GHERKIN: Mô phỏng hành vi  

---
# 2. Persona (chi tiết)
## Buyer
- Không phải chuyên IT, thao tác chủ yếu trên mobile  
- Muốn tìm kiếm nhanh, xem giá, đánh giá, mua hàng nhanh  
- Quan tâm tốc độ, rõ ràng của thông tin

## Seller
- Quan tâm dashboard, dễ đăng sản phẩm  
- Đăng nhiều sản phẩm liên tục  
- Cần theo dõi đơn và xử lý order chính xác

## Admin
- Có quyền cao nhất  
- Quản lý seller, sản phẩm, báo cáo, tranh chấp  
- Mức độ rủi ro cao nếu sai quy trình

---
# 3. Danh sách User Stories (Full System)
## Nhóm A — Authentication & User Management
- **US-A01** Đăng ký Buyer  
- **US-A02** Đăng ký Seller  
- **US-A03** Đăng nhập  
- **US-A04** Xác thực 2 bước (optional)  
- **US-A05** Admin xem danh sách user

## Nhóm B — Seller Onboarding
- **US-B01** Seller gửi request trở thành người bán  
- **US-B02** Admin duyệt seller  
- **US-B03** Seller cập nhật Shop Profile

## Nhóm C — Product Management
- **US-C01** Seller tạo sản phẩm mới  
- **US-C02** Seller chỉnh sửa sản phẩm  
- **US-C03** Seller bật/tắt trạng thái hiển thị  
- **US-C04** Seller upload ảnh sản phẩm  
- **US-C05** Admin ẩn sản phẩm vi phạm

## Nhóm D — Search & Browsing
- **US-D01** Buyer tìm kiếm từ khóa  
- **US-D02** Lọc theo category  
- **US-D03** Lọc theo giá  
- **US-D04** Lọc theo thuộc tính (color/size)  
- **US-D05** Sắp xếp kết quả (sort)

## Nhóm E — Cart & Order
- **US-E01** Thêm vào giỏ hàng  
- **US-E02** Xem giỏ hàng  
- **US-E03** Checkout  
- **US-E04** Nhận payment callback  
- **US-E05** Seller xử lý đơn  
- **US-E06** Buyer xác nhận nhận hàng

## Nhóm F — Reviews
- **US-F01** Buyer tạo đánh giá  
- **US-F02** Admin ẩn review vi phạm

## Nhóm G — Admin Moderation
- **US-G01** Admin xem seller pending  
- **US-G02** Admin khóa tài khoản  
- **US-G03** Admin xử lý tranh chấp

---
# 4. Acceptance Criteria (Chi tiết cực sâu)

## US-A01 — Đăng ký Buyer
### AC-A01-01 — Đăng ký thành công
- Email hợp lệ
- Mật khẩu theo policy (8 ký tự, hoa, thường, số, ký tự đặc biệt)
- DisplayName không rỗng
- Tạo User Role=Buyer, EmailConfirmed=false
- Hệ thống queue email Confirm

### AC-A01-02 — Trùng email
- Email đã tồn tại → trả lỗi EMAIL_TAKEN

### AC-A01-03 — Dữ liệu thiếu
- Nếu thiếu email/password/displayName → lỗi VALIDATION_FAILED

### AC-A01-04 — Format email sai
- Lỗi INVALID_EMAIL

---
## US-A02 — Đăng ký Seller
### AC-A02-01 — Thành công
- Tạo user role=Seller
- ShopProfile status=Pending
- Admin dashboard hiển thị seller pending

### AC-A02-02 — Trùng ShopName
- ShopName phải unique → SHOPNAME_TAKEN

---
## US-B02 — Admin duyệt Seller
### AC-B02-01 — Duyệt thành công
- Trạng thái chuyển Pending → Approved
- Ghi log: SellerApproved
- Queue email thông báo

### AC-B02-02 — Từ chối
- Trạng thái thành Rejected
- Lưu RejectReason

### AC-B02-03 — Không có quyền
- Non-admin truy cập → Forbidden

---
## US-C01 — Seller tạo sản phẩm
### AC-C01-01 — Thành công
- Seller phải approved
- Title 5–256 chars  
- Stock >= 0  
- Price > 0  
- Category tồn tại  
- IsPublished default=false

### AC-C01-02 — Upload ảnh hợp lệ
- Tối đa 5 ảnh  
- Định dạng PNG/JPEG/WEBP  
- Kích thước <= 5MB/ảnh

### AC-C01-03 — Sai định dạng ảnh
- Lỗi INVALID_IMAGE_TYPE

### AC-C01-04 — Seller chưa được duyệt
- Lỗi SELLER_NOT_APPROVED

---
## US-D01 — Search sản phẩm
### AC-D01-01 — Tìm kiếm theo từ khóa
- Query không phân biệt hoa thường
- Accent-insensitive (tiếng Việt)
- Full-text matching: Title > Desc > Category

### AC-D01-02 — Pagination
- pageSize <= 100
- Nếu vượt → force về 100

### AC-D01-03 — Không có kết quả
- Trả về empty array nhưng status 200

---
## US-E03 — Checkout
### AC-E03-01 — Thành công
- Giỏ hàng không rỗng
- Tính totalAmount = sum(qty × unitPrice)
- Tạo Order trạng thái PendingPayment
- Lưu snapshot giá (OrderItem)

### AC-E03-02 — Sản phẩm hết hàng
- Trả lỗi OUT_OF_STOCK

### AC-E03-03 — Buyer chưa xác thực email
- Trả lỗi EMAIL_NOT_CONFIRMED

---
## US-E04 — Payment Callback
### AC-E04-01 — Thành công
- status=success → Order chuyển Paid → Processing

### AC-E04-02 — Payment failed
- Order giữ trạng thái PendingPayment

### AC-E04-03 — Replay callback
- Callback lặp lại không được đổi trạng thái Order nếu đã Paid

---
## US-E05 — Seller xử lý đơn
### AC-E05-01 — Chuyển sang Shipped
- Seller phải đúng SellerId của Order
- TrackingNumber required

### AC-E05-02 — Chuyển sang Delivered
- Chỉ hệ thống hoặc shipper có quyền

---
## US-F01 — Review
### AC-F01-01 — Tạo review hợp lệ
- OrderItem.task Completed  
- Rating 1–5  
- Comment length <= 2000

### AC-F01-02 — Không mua → không review
- Lỗi NOT_PURCHASED

---
# 5. Gherkin Scenarios (Mở rộng sâu)

## Seller tạo sản phẩm
```
Scenario: Seller tạo sản phẩm hợp lệ
  Given Seller đã được Admin phê duyệt
  And Seller đang ở trang tạo sản phẩm
  When Seller nhập Title="Áo thun tay lỡ" Price=120000 Stock=50 Category=2
  And Seller upload 3 ảnh hợp lệ
  Then hệ thống tạo Product mới trạng thái IsPublished=false
  And hiển thị thông báo "Tạo sản phẩm thành công"
```

## Thanh toán thành công
```
Scenario: Buyer checkout và thanh toán thành công
  Given Buyer có 2 sản phẩm trong giỏ
  When Buyer bấm Checkout
  Then hệ thống tạo Order trạng thái PendingPayment
  When PaymentGateway callback status="success"
  Then Order chuyển sang Paid rồi Processing
```

## Review
```
Scenario: Buyer review sau khi nhận hàng
  Given Order trạng thái Completed
  When Buyer nhập Rating=5 Comment="Sản phẩm tuyệt vời"
  Then hệ thống lưu review và cập nhật rating trung bình Seller
```

---
# 6. Negative Scenarios (Lỗi mô tả chi tiết)
- Buyer thêm vào giỏ nhưng sản phẩm bị deactive → error PRODUCT_NOT_ACTIVE
- Seller chỉnh sửa sản phẩm bị Admin khóa → error PRODUCT_BANNED
- Buyer tìm kiếm với pageSize=10000 → force 100
- Payment callback không có orderId → error INVALID_CALLBACK
- Seller cố ship đơn không thuộc mình → error UNAUTHORIZED_SELLER

---
# 7. Data Constraints (chi tiết cho từng trường)
### Email
- Max 256  
- RFC5322  
- Không cho phép disposable email (optional rule)

### Price
- 0 < price ≤ 10,000,000,000

### Rating
- Integer 1–5

### Comment
- Max 2000 chars  
- HTML không được phép (sanitize)

### ShopName
- Unique
- Max 128
- Không chứa ký tự đặc biệt (!@#$...) ngoại trừ khoảng trắng và dấu tiếng Việt

---
# 8. Dependencies
- US-A01 phải hoàn thành trước mọi US của Buyer
- US-A02 + US-B02 mới cho phép Seller tạo product
- US-E03 (Checkout) phụ thuộc US-E01 (Cart)
- Review phụ thuộc E06 (Buyer confirmed)

---
# 9. Definition of Done (DoD)
### Với mỗi User Story:
- AC đầy đủ được đáp ứng  
- Negative case được mô tả  
- Có Gherkin scenario  
- Có data constraints  
- Có mô tả hiển thị UI/UX (dưới dạng văn bản)  
- Không cần code

---
# 10. Test Matrix mapping (rất chi tiết)
| Module | US | Positive Tests | Negative Tests |
|--------|-------|-------------------|------------------|
| Auth | A01 | Valid register | Invalid email, weak password |
| Seller | B02 | Approve seller | Wrong role, invalid id |
| Product | C01 | Valid create | Too many images, invalid mime |
| Search | D01 | Keyword search | No result, invalid page |
| Cart | E01 | Add valid | Out of stock |
| Order | E03 | Normal checkout | Email not confirmed |
| Payment | E04 | Success callback | Replay callback |
| Review | F01 | Valid review | Not purchased |
| Admin | G03 | Resolve dispute | Unauthorized user |

---
**KẾT THÚC FILE US-AC.md**

