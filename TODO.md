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
- [x] 1. Chuyển NON-SARGABLE → SARGABLE query
- [x] 2. Thêm `AsNoTracking()` cho tất cả query GET chỉ đọc
- [x] 3. Thêm index DB (migration `AddIndexes`)
- [x] 4. Build Release thành công (0 lỗi, 0 warning)

## Đang thực hiện (Phần 3 - Fix Data Protection key + Anti-forgery)
Vấn đề:
- `CryptographicException: The key {..} was not found in the key ring` → Data Protection keys không được persist,
  Render có filesystem tạm → restart là mất key → session cookie/anti-forgery cookie không giải mã được → mất dữ liệu đăng nhập.

### Kế hoạch
- [x] 1. Thêm package `Microsoft.AspNetCore.DataProtection.EntityFrameworkCore` vào csproj
- [x] 2. Thêm `DbSet<DataProtectionKey>` vào `AppDbContext`
- [x] 3. Cấu hình `.PersistKeysToDbContext<AppDbContext>()` trong `Program.cs`
- [x] 4. Thêm `@Html.AntiForgeryToken()` vào form Login (`Login.cshtml`)
- [x] 5. Thêm `[ValidateAntiForgeryToken]` cho action Login POST (`AuthController.cs`)
- [x] 6. Tạo migration `DataProtectionKeys` (bảng `DataProtectionKeys` trong Supabase)
- [x] 7. Build Release kiểm tra 0 lỗi

