# Nhật ký hội thoại (chi tiết)

> Dự án: `BAI_TAP_LON` – Quản lý khách sạn (WinForms + SQL Server)
>  
> Thời gian: Từ lúc bắt đầu tới hiện tại.

## 1) Khởi động dự án và CSDL
1. Người dùng hỏi cách chạy dự án trong VS Code.
2. Mình kiểm tra dự án và đọc `README.md`.
3. Hướng dẫn tạo solution, chạy Docker SQL Server, chạy script CSDL và cập nhật connection string.
4. Người dùng chạy `sqlcmd` trong container, gặp lỗi:
1. Sai path `sqlcmd` → sửa sang `/opt/mssql-tools18/bin/sqlcmd`.
2. Lỗi TLS self‑signed → thêm `-C` (TrustServerCertificate).
5. Chạy script CSDL thành công, kiểm tra bảng đã tạo.

## 2) Sửa lỗi build .NET
1. Lỗi thiếu `System.Data.SqlClient` → thêm package vào `HotelManager.DAL.csproj`.
2. Lỗi thiếu `ConfigurationManager` → thêm `System.Configuration.ConfigurationManager`.
3. Lỗi restore do không có `.sln` → hướng dẫn restore theo `csproj`.

## 3) Nâng target .NET
1. App báo yêu cầu .NET Desktop Runtime.
2. Mình đổi `TargetFramework` sang `net10.0`/`net10.0-windows` toàn bộ project.

## 4) Tính năng Phòng, Loại phòng, Khách hàng
1. Thêm màn hình quản lý phòng (CRUD) + dịch vụ BLL.
2. Thêm màn hình quản lý loại phòng (CRUD) + dịch vụ BLL.
3. Thêm màn hình quản lý khách hàng (CRUD) + dịch vụ BLL.
4. Nối menu vào các màn hình.
5. Thêm kiểm tra lựa chọn và thông báo lỗi thân thiện.

## 5) Chức năng Đặt phòng và Thanh toán
1. Thêm màn hình “Đặt phòng” có:
1. Chọn khách hàng, ngày, số người.
2. Chọn phòng trống (có toggle hiển thị tất cả phòng).
2. Tạo booking + booking rooms + cập nhật trạng thái phòng.
3. Thêm màn hình “Thanh toán”:
1. Tính tiền từ booking rooms.
2. Tạo invoice + payment.
3. Trả phòng (Rooms → Available) và cập nhật BookingRooms.

## 6) Lịch sử, hóa đơn, báo cáo
1. Thêm Lịch sử đặt phòng (lọc theo ngày, trạng thái).
2. Thêm Hóa đơn (lọc theo ngày).
3. Thêm Báo cáo doanh thu theo ngày/tháng.
4. Thêm popup Chi tiết hóa đơn và nút In hóa đơn.

## 7) Giao diện & Guna UI2
1. Tích hợp Guna.UI2 cho nhiều form:
1. Khách hàng, phòng, loại phòng.
2. Đặt phòng, thanh toán.
3. Hóa đơn, lịch sử, báo cáo, chi tiết hóa đơn.
2. Cải thiện responsive bằng Dock/Fill, TableLayout.
3. Tùy biến MenuStrip để đồng bộ theme.

## 8) Việt hóa giao diện
1. Đổi nhãn, button, message box từ không dấu → có dấu.
2. Đổi tiếng Anh sang tiếng Việt ở UI.
3. Việt hóa tiêu đề cột trong các bảng.
4. Việt hóa trạng thái phòng, trạng thái hóa đơn, phương thức thanh toán.
5. Sửa lỗi font bị mã hóa sai trong một số file.

## 9) Dịch vụ và cộng tiền dịch vụ
1. Thêm màn hình quản lý dịch vụ:
1. Tên dịch vụ, giá, kích hoạt.
2. CRUD đầy đủ.
2. Khi đặt phòng:
1. Cho chọn dịch vụ + số lượng.
2. Lưu ServiceUsages.
3. Khi thanh toán:
1. Cộng cả tiền dịch vụ vào tổng.

## 10) Cài đặt thông tin chuyển khoản + QR
1. Thêm menu “Cài đặt”.
2. Tạo SettingsForm lưu:
1. Ngân hàng, số TK, chủ TK, ảnh QR.
3. Ban đầu lưu appSettings gây lỗi “locked”.
4. Đổi sang lưu JSON trong AppData.
5. Hiển thị QR trong popup riêng (PaymentInfoForm).
6. Thêm hiển thị đường dẫn ảnh QR và trạng thái lỗi.

## 11) DatePicker đẹp hơn
1. Chuẩn hóa DatePicker Guna2:
1. Bo góc, viền, màu chữ.
2. Định dạng `dd/MM/yyyy`.
2. Thử tùy biến calendar dropdown:
1. Bị lỗi do Guna2 không hỗ trợ Calendar*.
2. Rollback các thuộc tính Calendar*.

## 12) Tối ưu bố cục menu/tabs
1. Chuyển một số menu sang tabs.
2. Loại bỏ menu trùng với tab (khách hàng, thanh toán, đặt phòng, báo cáo).
3. Thêm tab Lịch sử đặt phòng.

## 13) Tự động cập nhật dữ liệu
1. Khi thanh toán xong:
1. Tự refresh Lịch sử đặt phòng, Hóa đơn, Báo cáo.

## 14) Đóng gói cài đặt (LocalDB)
1. Chọn LocalDB: `(localdb)\MSSQLLocalDB`.
2. Tự khởi tạo DB:
1. Chạy `hotel_management.sql` khi app khởi động.
3. Copy script SQL vào output.
4. Thêm installer Inno Setup (`installer/HotelManager.iss`).
5. Hướng dẫn build publish + compile installer.

## 15) Các lỗi và xử lý chính
1. Lỗi `sqlcmd` path và TLS self‑signed → sửa path và thêm `-C`.
2. Lỗi thiếu package .NET → thêm `System.Data.SqlClient`, `ConfigurationManager`.
3. Lỗi appsettings bị khóa → chuyển sang JSON settings.
4. Lỗi Guna2 Calendar* → bỏ thuộc tính không hỗ trợ.
5. Lỗi mã hóa tiếng Việt → chỉnh lại text trực tiếp.

## 16) Các file chính đã thêm/sửa (tóm tắt)
1. BLL: `RoomService.cs`, `RoomTypeService.cs`, `CustomerService.cs`, `PaymentService.cs`, `InvoiceService.cs`, `ReportService.cs`, `ServiceService.cs`.
2. WinForms: `RoomsForm.cs`, `RoomTypesForm.cs`, `CustomersForm.cs`, `BookingsForm.cs`, `PaymentsForm.cs`, `PaymentInfoForm.cs`, `InvoicesForm.cs`, `InvoiceDetailsForm.cs`, `BookingHistoryForm.cs`, `ReportsForm.cs`, `ServicesForm.cs`, `SettingsForm.cs`, `DbInitializer.cs`.
3. Cấu hình: `HotelManager.WinForms.csproj` (copy SQL script), `Db.cs` (override connection string).
4. Installer: `installer/HotelManager.iss`.

## 17) Trạng thái hiện tại
1. App chạy ổn định trên .NET 10.
2. UI đã Việt hóa, có Guna UI2.
3. Tính năng: đặt phòng, thanh toán, hóa đơn, báo cáo, dịch vụ, lịch sử, in hóa đơn.
4. Cài đặt chuyển khoản + QR hoạt động qua popup.
5. LocalDB tự tạo CSDL khi chạy lần đầu.
