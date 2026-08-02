# TODO: TimekeepingSystem - Kế hoạch & trạng thái

## Project hoàn thiện (đã test OK)
- [x] 1. Tạo solution `.slnx`
- [x] 2. Tạo project MVC + package EF Core Sqlite
- [x] 3. Models, ViewModels, Controllers, Views
- [x] 4. Program.cs, appsettings.json, site.css
- [x] 5. Build & test (0 lỗi, 17/17 test PASS)

## Yêu cầu mới: 5 công nhân + bảng chấm công 2 tháng
- [x] 1. Thêm 2 công nhân (worker4 = Phạm Thị D, worker5 = Hoàng Văn E) vào seed AppDbContext
- [x] 2. Tạo Models/SeedData.cs (đảm bảo 5 CN + sinh chấm công tháng trước & hiện tại)
- [x] 3. Cập nhật Program.cs gọi SeedData.Initialize
- [x] 4. Build & test

## Kết quả test
- Build: 0 lỗi, 0 cảnh báo
- Login Admin (admin/admin123) -> OK
- 6 users: 1 Admin + 5 công nhân (worker1-5)
- 140 bản ghi chấm công: Tháng trước (2026-07): 135 bản, Tháng hiện tại (2026-08): 5 bản
- Mỗi công nhân: 28 bản ghi chấm công
- Phân bố: Present 99, Late 15, Absent 15, HalfDay 11

