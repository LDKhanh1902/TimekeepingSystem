# TODO: TimekeepingSystem - Fix lỗi timeout khi deploy Render

## Vấn đề đã xác định
1. Project KHÔNG có thư mục `Migrations/` → `Migrate()` không tạo được bảng trong Supabase
2. Thiếu `UseForwardedHeaders` → nguy cơ redirect loop HTTP↔HTTPS khi chạy sau proxy của Render
3. Dockerfile hardcode cổng 10000, không theo biến `$PORT` của Render
4. Khởi động app chậm do retry kết nối DB nhiều lần (5 lần × 10s) khi DB lỗi
5. `CookieSecurePolicy.Always` chặn session cookie khi chạy qua HTTP

## Đã sửa (Phần 1 - Fix timeout deploy)
- [x] 1. Thêm package `Microsoft.EntityFrameworkCore.Design` vào csproj
- [x] 2. Cài `dotnet-ef` global tool (bản 8.x tương thích EF Core 8)
- [x] 3. Tạo migration `InitialCreate` (tạo bảng Users, Shifts, Attendances + seed data)
- [x] 4. Sửa `Program.cs`:
      - Thêm `UseForwardedHeaders` (fix redirect loop khi sau proxy)
      - Bọc `Migrate()` trong try-catch ghi log rõ ràng (không crash app)
      - Giảm retry (3 lần × 5s) để fail nhanh, tránh timeout khởi động
      - Đổi `CookieSecurePolicy.Always` → `SameAsRequest` (cookie hoạt động cả HTTP local & HTTPS Render)
- [x] 5. Sửa `Dockerfile`: lắng nghe theo biến `$PORT` của Render (mặc định 10000)
- [x] 6. Thêm `.dockerignore` trong thư mục project (tránh copy bin/obj → build nhanh hơn)
- [x] 7. Build Release thành công (0 lỗi, 0 warning)

## Đã sửa (Phần 2 - Tối ưu truy vấn DB)
- [x] 1. Chuyển NON-SARGABLE → SARGABLE query:
      - Trước: `a.Date.Year == y && a.Date.Month == m` → EF dịch thành `date_part(...)` → KHÔNG dùng được index
      - Sau: `a.Date >= startDate && a.Date < endDate` → dùng được index `(UserId, Date)` và `Date`
      - Áp dụng cho: AdminController (WorkerAttendance, MonthlyAttendance, SalarySlip), WorkerController (Dashboard, Attendance)
- [x] 2. Thêm `AsNoTracking()` cho tất cả query GET chỉ đọc:
      - Giảm RAM/CPU do không bật change tracker
      - Áp dụng cho: AuthController (Login), AdminController, WorkerController
- [x] 3. Thêm index DB (migration `AddIndexes`):
      - `IX_Users_Role_IsActive` trên `(Role, IsActive)` → tăng tốc query lọc theo vai trò
      - `IX_Attendances_Date` trên `Date` → tăng tốc query theo ngày/tháng
- [x] 4. Build Release thành công (0 lỗi, 0 warning)

## Ghi chú
- Đã thêm `AppDbContextFactory.cs` (design-time factory) để `dotnet ef` tạo migration
  mà không cần kết nối Supabase trong lúc build.
- Migration `InitialCreate` tạo đầy đủ bảng + seed 3 ca + 6 users (1 Admin + 5 Worker).
- Migration `AddIndexes` thêm 2 index tối ưu truy vấn.
- Khi deploy lên Render: push code (bao gồm thư mục `Migrations/`) lên GitHub
  → Render sẽ build Dockerfile mới → chạy `Migrate()` tự tạo bảng + index trong Supabase.

