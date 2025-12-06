# Hướng Dẫn Phát Triển - Hệ Thống Đặt Vé Tàu

## Mục Lục

*   [1. Giới Thiệu](#1-giới-thiệu)
*   [2. Yêu Cầu Phần Mềm](#2-yêu-cầu-phần-mềm)
*   [3. Cấu Trúc Thư Mục Dự Án](#3-cấu-trúc-thư-mục-dự-án)
*   [4. Hướng Dẫn Cài Đặt](#4-hướng-dẫn-cài-đặt)
*   [5. Chạy Ứng Dụng](#5-chạy-ứng-dụng)
*   [6. Cấu Hình](#6-cấu-hình)
*   [7. Quy Trình Phát Triển](#7-quy-trình-phát-triển)
*   [8. Xử Lý Lỗi Thường Gặp](#8-xử-lý-lỗi-thường-gặp)

---

## 1. Giới Thiệu

### 1.1. Mô Tả Dự Án

Hệ thống đặt vé tàu cao tốc mô phỏng với các tính năng:

*   Đăng ký và đăng nhập người dùng
*   Quản lý chuyến tàu với 10 ghế mỗi chuyến
*   Đặt vé và hủy vé
*   Quản trị hệ thống (dành cho quản trị viên)

### 1.2. Kiến Trúc Hệ Thống

```shell
┌─────────────────┐     ┌─────────────────┐
│  Ứng dụng       │     │  Ứng dụng       │
│  Quản trị       │     │  Khách hàng     │
│  (WinForms)     │     │  (WinForms)     │
└────────┬────────┘     └────────┬────────┘
         │                       │
         │    Giao tiếp TCP      │
         └───────────┬───────────┘
                     │
              ┌──────▼──────┐
              │  Máy chủ    │
              │  Backend    │
              │  (.NET 9)   │
              └──────┬──────┘
                     │
              ┌──────▼──────┐
              │  Cơ sở      │
              │  dữ liệu    │
              │  SQL Server │
              └─────────────┘
```

### 1.3. Công Nghệ Sử Dụng

| Thành phần          | Công nghệ               | Phiên bản            |
| ------------------- | ----------------------- | -------------------- |
| Máy chủ             | .NET Background Service | .NET 9.0             |
| Giao tiếp           | TCP/Socket              | -                    |
| ORM                 | Dapper                  | 2.1.66               |
| Cơ sở dữ liệu       | Microsoft SQL Server    | 2022                 |
| Ứng dụng quản trị   | Windows Forms           | .NET Framework 4.8.1 |
| Ứng dụng khách hàng | Windows Forms           | .NET Framework 4.8.1 |
| Quản lý gói         | pnpm                    | 10.23.0              |
| Kiểm tra mã nguồn   | Biome, Prettier         | 2.3.7, 3.6.2         |

---

## 2. Yêu Cầu Phần Mềm

### 2.1. Danh Sách Phần Mềm Cần Cài Đặt

#### Bắt buộc

| Phần mềm                            | Phiên bản   | Mục đích                      | Liên kết tải                                                                 |
| ----------------------------------- | ----------- | ----------------------------- | ---------------------------------------------------------------------------- |
| .NET 9 SDK                          | 9.0 trở lên | Biên dịch và chạy máy chủ     | [Tải .NET 9](https://dotnet.microsoft.com/download/dotnet/9.0)               |
| .NET Framework 4.8.1 Developer Pack | 4.8.1       | Biên dịch ứng dụng WinForms   | [Tải .NET Framework](https://dotnet.microsoft.com/download/dotnet-framework) |
| Docker Desktop                      | Mới nhất    | Chạy cơ sở dữ liệu SQL Server | [Tải Docker](https://www.docker.com/products/docker-desktop)                 |
| Node.js                             | 18 trở lên  | Chạy công cụ phát triển       | [Tải Node.js](https://nodejs.org/)                                           |
| pnpm                                | 10.23.0     | Quản lý gói Node.js           | [Tải pnpm](https://pnpm.io/installation)                                     |

#### Khuyến nghị

| Phần mềm           | Mục đích                       | Liên kết tải                                       |
| ------------------ | ------------------------------ | -------------------------------------------------- |
| Visual Studio 2022 | Môi trường phát triển tích hợp | [Tải VS 2022](https://visualstudio.microsoft.com/) |
| JetBrains Rider    | Môi trường phát triển thay thế | [Tải Rider](https://www.jetbrains.com/rider/)      |
| Git                | Quản lý phiên bản              | [Tải Git](https://git-scm.com/)                    |

### 2.2. Kiểm Tra Cài Đặt

Mở PowerShell và chạy các lệnh sau để kiểm tra:

```powershell
# Kiểm tra .NET SDK
dotnet --list-sdks

# Kết quả mong đợi: 9.0.xxx [đường dẫn]
```

```powershell
# Kiểm tra .NET Runtime
dotnet --list-runtimes

# Kết quả mong đợi: có Microsoft.NETCore.App 9.0.x
```

```powershell
# Kiểm tra Docker
docker --version

# Kết quả mong đợi: Docker version 2x.x.x
```

```powershell
# Kiểm tra Node.js
node --version

# Kết quả mong đợi: v18.x.x hoặc cao hơn
```

```powershell
# Kiểm tra pnpm
pnpm --version

# Kết quả mong đợi: 10.23.0 hoặc cao hơn
```

---

## 3. Cấu Trúc Thư Mục Dự Án

### 3.1. Tổng Quan

```powershell
train-ticket-booking-system/
├── backend/                    # Máy chủ .NET 9 (Dịch vụ nền)
├── frontend/
│   ├── admin/                  # Ứng dụng quản trị (WinForms)
│   ├── client/                 # Ứng dụng khách hàng (WinForms)
│   └── sdk-client/             # Thư viện giao tiếp TCP
├── database/                   # Cấu hình cơ sở dữ liệu
├── packages/                   # Gói NuGet dùng chung
├── train-ticket-booking-system.slnx  # Tệp giải pháp
├── package.json                # Cấu hình công cụ phát triển
└── DEVELOPMENT.vi.md           # Tài liệu này
```

### 3.2. Chi Tiết Thư Mục Backend

Máy chủ được tổ chức theo kiến trúc 4 tầng:

```powershell
backend/
├── Program.cs                  # Điểm khởi đầu, cấu hình DI
├── Worker.cs                   # Dịch vụ nền chính
├── appsettings.json            # Cấu hình ứng dụng
├── Business/                   # TẦNG NGHIỆP VỤ
│   ├── Models/                 # Các lớp mô hình dữ liệu
│   │   ├── User.cs            # Người dùng
│   │   ├── Train.cs           # Chuyến tàu
│   │   ├── Seat.cs            # Ghế ngồi
│   │   ├── Booking.cs         # Đặt vé
│   │   └── AuditLog.cs        # Nhật ký hoạt động
│   └── Services/               # Các dịch vụ xử lý nghiệp vụ
│       ├── AuthenticationService.cs
│       ├── TrainService.cs
│       ├── BookingService.cs
│       ├── UserService.cs
│       └── AuditService.cs
├── DataAccess/                 # TẦNG TRUY CẬP DỮ LIỆU
│   ├── DbContext/              # Ngữ cảnh cơ sở dữ liệu (Dapper)
│   ├── Repositories/           # Các kho lưu trữ (Repository Pattern)
│   └── UnitOfWork/             # Mẫu Unit of Work
├── Infrastructure/             # TẦNG HẠ TẦNG
│   └── Security/               # Bảo mật
│       ├── PasswordHasher.cs   # Mã hóa mật khẩu (BCrypt)
│       └── SessionManager.cs   # Quản lý phiên đăng nhập
└── Presentation/               # TẦNG TRÌNH BÀY
    ├── TcpServer.cs            # Máy chủ TCP
    ├── ClientHandler.cs        # Xử lý kết nối máy khách
    ├── Protocol/               # Giao thức truyền thông
    │   ├── Request.cs          # Định dạng yêu cầu
    │   └── Response.cs         # Định dạng phản hồi
    └── Handlers/               # Bộ xử lý yêu cầu
        ├── AuthHandler.cs
        ├── TrainHandler.cs
        ├── BookingHandler.cs
        ├── UserHandler.cs
        └── AuditHandler.cs
```

### 3.3. Chi Tiết Thư Mục Frontend

#### Ứng dụng Quản trị (admin)

```powershell
frontend/admin/
├── Program.cs                  # Điểm khởi đầu
├── admin.csproj                # Tệp dự án
├── Forms/                      # Các biểu mẫu giao diện
│   ├── Authentication/         # Đăng nhập
│   ├── Main/                   # Màn hình chính
│   ├── TrainManagement/        # Quản lý chuyến tàu
│   ├── BookingManagement/      # Quản lý đặt vé
│   ├── UserManagement/         # Quản lý người dùng
│   ├── AuditLog/               # Xem nhật ký hoạt động
│   └── Statistics/             # Thống kê
├── Controls/                   # Điều khiển tùy chỉnh
├── Helpers/                    # Lớp tiện ích
└── Models/                     # Mô hình hiển thị
```

#### Ứng dụng Khách hàng (client)

```powershell
frontend/client/
├── Program.cs                  # Điểm khởi đầu
├── client.csproj               # Tệp dự án
├── Forms/                      # Các biểu mẫu giao diện
│   ├── Authentication/         # Đăng nhập, đăng ký
│   ├── Main/                   # Màn hình chính
│   ├── TrainSearch/            # Tìm kiếm chuyến tàu
│   ├── Booking/                # Đặt vé
│   └── Profile/                # Thông tin cá nhân
├── Controls/                   # Điều khiển tùy chỉnh
├── Helpers/                    # Lớp tiện ích
└── Models/                     # Mô hình hiển thị
```

#### Thư viện SDK (sdk-client)

```powershell
frontend/sdk-client/
├── ApiClient.cs                # Máy khách API chính
├── TcpClientManager.cs         # Quản lý kết nối TCP
├── Protocol/                   # Giao thức
│   ├── DTOs.cs                 # Đối tượng truyền dữ liệu
│   ├── Request.cs              # Định dạng yêu cầu
│   └── Response.cs             # Định dạng phản hồi
├── Services/                   # Dịch vụ gọi API
│   ├── AuthenticationService.cs
│   ├── TrainService.cs
│   ├── BookingService.cs
│   ├── UserService.cs
│   └── AuditService.cs
└── Exceptions/                 # Ngoại lệ tùy chỉnh
    └── ApiException.cs
```

### 3.4. Chi Tiết Thư Mục Database

```powershell
database/
├── docker-compose.yml          # Cấu hình Docker cho SQL Server và Flyway
├── init-database.ps1           # Tập lệnh khởi tạo cơ sở dữ liệu
└── migrations/                 # Tập lệnh di chuyển Flyway
    ├── V1__initial_schema.sql  # Lược đồ cơ sở dữ liệu ban đầu
    ├── V2__add_pagination_indexes.sql  # Chỉ mục tối ưu phân trang
    └── V3__add_cascade_delete.sql      # Ràng buộc CASCADE DELETE
```

### 3.5. Các Tệp Cấu Hình Gốc

| Tệp                     | Mục đích                                   |
| ----------------------- | ------------------------------------------ |
| `package.json`          | Cấu hình công cụ phát triển Node.js        |
| `pnpm-workspace.yaml`   | Cấu hình không gian làm việc pnpm          |
| `biome.json`            | Cấu hình kiểm tra mã JavaScript/TypeScript |
| `prettier.config.cjs`   | Cấu hình định dạng mã                      |
| `commitlint.config.ts`  | Cấu hình quy ước thông điệp commit         |
| `lint-staged.config.js` | Cấu hình kiểm tra mã khi commit            |
| `.husky/`               | Git Hooks tự động                          |

---

## 4. Hướng Dẫn Cài Đặt

### 4.1. Sao Chép Mã Nguồn

```powershell
# Sao chép kho mã nguồn
git clone https://github.com/Phunguy65/train-ticket-booking-system.git

# Di chuyển vào thư mục dự án
cd train-ticket-booking-system
```

### 4.2. Cài Đặt Phụ Thuộc Node.js

```powershell
# Cài đặt các gói phát triển
pnpm install
```

Lệnh này sẽ cài đặt:

*   Biome (kiểm tra mã nguồn)
*   Prettier (định dạng mã)
*   Husky (Git Hooks)
*   lint-staged (kiểm tra tệp đã thay đổi)
*   commitlint (kiểm tra thông điệp commit)

### 4.3. Khôi Phục Gói NuGet

```powershell
# Khôi phục tất cả gói NuGet cho giải pháp
dotnet restore train-ticket-booking-system.slnx
```

### 4.4. Thiết Lập Cơ Sở Dữ Liệu

#### Bước 1: Khởi động Docker Desktop

Đảm bảo Docker Desktop đang chạy trước khi thực hiện các bước tiếp theo.

#### Bước 2: Khởi động container SQL Server

```powershell
# Di chuyển vào thư mục database
cd database

# Khởi động SQL Server
docker-compose up -d

# Quay lại thư mục gốc
cd ..
```

#### Bước 3: Đợi SQL Server khởi động

```powershell
# Đợi 30 giây để SQL Server khởi động hoàn tất
Start-Sleep -Seconds 30
```

#### Bước 4: Kiểm tra container đang chạy

```powershell
# Kiểm tra trạng thái container
docker ps | Select-String "ttbs-database"
```

Kết quả mong đợi:

```shell
xxxxxxxxxxxx   mcr.microsoft.com/mssql/server:2022-latest   ...   Up   0.0.0.0:8666->1433/tcp   ttbs-database
```

#### Bước 5: Áp dụng lược đồ cơ sở dữ liệu (nếu cần)

```powershell
# Khởi tạo cơ sở dữ liệu
.\init-database.ps1

# Chạy di chuyển Flyway
docker-compose up ttbs-flyway
```

### 4.5. Biên Dịch Dự Án

```powershell
# Biên dịch toàn bộ giải pháp
dotnet build train-ticket-booking-system.slnx --configuration Debug
```

Kết quả mong đợi:

```shell
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

---

## 5. Chạy Ứng Dụng

### 5.1. Khởi Động Máy Chủ Backend

**Quan trọng**: Máy chủ backend phải được khởi động **trước** khi chạy các ứng
dụng frontend.

```powershell
# Di chuyển vào thư mục backend
cd backend

# Chạy máy chủ
dotnet run
```

Kết quả mong đợi:

```shell
info: TcpServer[0]
      Máy chủ TCP đang lắng nghe tại 127.0.0.1:5000
```

Giữ cửa sổ terminal này mở và mở terminal mới cho các bước tiếp theo.

### 5.2. Khởi Động Ứng Dụng Quản Trị

Mở terminal mới:

```powershell
# Di chuyển vào thư mục admin
cd frontend\admin

# Chạy ứng dụng
dotnet run
```

Hoặc chạy tệp thực thi:

```powershell
.\frontend\admin\bin\Debug\admin.exe
```

### 5.3. Khởi Động Ứng Dụng Khách Hàng

Mở terminal mới:

```powershell
# Di chuyển vào thư mục client
cd frontend\client

# Chạy ứng dụng
dotnet run
```

Hoặc chạy tệp thực thi:

```powershell
.\frontend\client\bin\Debug\client.exe
```

### 5.4. Thứ Tự Khởi Động Khuyến Nghị

```shell
1. Docker Desktop (đảm bảo đang chạy)
2. Container SQL Server (docker-compose up -d)
3. Máy chủ Backend (dotnet run trong thư mục backend)
4. Ứng dụng Frontend (admin hoặc client)
```

---

## 6. Cấu Hình

### 6.1. Cấu Hình Máy Chủ TCP

Tệp: `backend/appsettings.json`

```json
{
 "TcpServer": {
  "Host": "127.0.0.1", // Địa chỉ IP lắng nghe
  "Port": 5000, // Cổng lắng nghe
  "MaxConnections": 100, // Số kết nối tối đa
  "ConnectionTimeout": 30, // Thời gian chờ kết nối (giây)
  "HeartbeatInterval": 60 // Khoảng cách gửi heartbeat (giây)
 }
}
```

### 6.2. Cấu Hình Bảo Mật

```json
{
 "Security": {
  "SessionTimeout": 30, // Thời gian phiên hết hạn (phút)
  "MaxLoginAttempts": 5, // Số lần đăng nhập sai tối đa
  "LockoutDuration": 15 // Thời gian khóa tài khoản (phút)
 }
}
```

### 6.3. Cấu Hình Đặt Vé

```json
{
 "Booking": {
  "CancellationDeadlineHours": 24 // Hạn hủy vé trước giờ khởi hành
 }
}
```

### 6.4. Cấu Hình Cơ Sở Dữ Liệu

#### Chuỗi kết nối

Tệp: `backend/appsettings.json`

```json
{
 "ConnectionStrings": {
  "DefaultConnection": "Server=localhost,8666;Database=TrainTicketBooking;User Id=sa;Password=My$tr0ngP@ssw0rd!;TrustServerCertificate=True;Min Pool Size=5;Max Pool Size=100;"
 }
}
```

#### Giải thích các thành phần

| Thành phần             | Giá trị            | Mô tả                           |
| ---------------------- | ------------------ | ------------------------------- |
| Server                 | localhost,8666     | Địa chỉ và cổng SQL Server      |
| Database               | TrainTicketBooking | Tên cơ sở dữ liệu               |
| User Id                | sa                 | Tên người dùng                  |
| Password               | My$tr0ngP@ssw0rd!  | Mật khẩu                        |
| TrustServerCertificate | True               | Bỏ qua xác thực chứng chỉ SSL   |
| Min Pool Size          | 5                  | Số kết nối tối thiểu trong pool |
| Max Pool Size          | 100                | Số kết nối tối đa trong pool    |

### 6.5. Cấu Hình Docker

Tệp: `database/docker-compose.yml`

```yaml
services:
    ttbs-database:
        image: mcr.microsoft.com/mssql/server:2022-latest
        container_name: ttbs-database
        environment:
            - ACCEPT_EULA=Y # Chấp nhận điều khoản sử dụng
            - SA_PASSWORD=My$tr0ngP@ssw0rd! # Mật khẩu SA
        ports:
            - '${DB_PORT:-8666}:1433' # Ánh xạ cổng (mặc định 8666)
```

#### Thay đổi cổng cơ sở dữ liệu

Đặt biến môi trường trước khi chạy:

```powershell
$env:DB_PORT = "1433"
docker-compose up -d
```

---

## 7. Quy Trình Phát Triển

### 7.1. Git Hooks Tự Động

Dự án sử dụng Husky và lint-staged để tự động kiểm tra mã nguồn trước khi
commit.

#### Các kiểm tra được thực hiện

| Loại tệp         | Công cụ       | Hành động                   |
| ---------------- | ------------- | --------------------------- |
| `*.js, *.ts`     | Biome         | Kiểm tra và sửa lỗi         |
| `*.json, *.yaml` | Prettier      | Định dạng lại               |
| `*.md`           | markdownlint  | Kiểm tra định dạng Markdown |
| `*.sql`          | Prettier      | Định dạng lại               |
| `*.cs, *.csproj` | dotnet format | Định dạng theo quy chuẩn C# |

### 7.2. Quy Ước Thông Điệp Commit

Dự án tuân theo [Conventional Commits](https://www.conventionalcommits.org/).

#### Cấu trúc thông điệp

```shell
<loại>(<phạm vi tùy chọn>): <mô tả>

[nội dung tùy chọn]

[chân trang tùy chọn]
```

#### Các loại commit

| Loại       | Mô tả                                      | Ví dụ                                  |
| ---------- | ------------------------------------------ | -------------------------------------- |
| `feat`     | Tính năng mới                              | `feat: thêm chức năng đặt vé`          |
| `fix`      | Sửa lỗi                                    | `fix: sửa lỗi hiển thị danh sách ghế`  |
| `docs`     | Thay đổi tài liệu                          | `docs: cập nhật hướng dẫn cài đặt`     |
| `style`    | Thay đổi định dạng (không ảnh hưởng logic) | `style: định dạng lại mã nguồn`        |
| `refactor` | Tái cấu trúc mã (không thêm/sửa tính năng) | `refactor: tách lớp BookingService`    |
| `test`     | Thêm hoặc sửa test                         | `test: thêm unit test cho AuthService` |
| `chore`    | Công việc bảo trì                          | `chore: cập nhật phụ thuộc`            |

#### Quy tắc

*   Tiêu đề không quá **72 ký tự**
*   Mỗi dòng nội dung không quá **80 ký tự**
*   Viết bằng tiếng Việt hoặc tiếng Anh (nhất quán trong dự án)

### 7.3. Định Dạng Mã Nguồn

#### Định dạng tất cả mã nguồn

```powershell
# Định dạng JavaScript/TypeScript
pnpm biome check --fix .

# Định dạng JSON, YAML, SQL
pnpm prettier --write .

# Định dạng Markdown
pnpm markdownlint-cli2 --fix "**/*.md"

# Định dạng C#
dotnet format train-ticket-booking-system.slnx
```

#### Định dạng một tệp cụ thể

```powershell
# Định dạng tệp C#
dotnet format train-ticket-booking-system.slnx --include backend\Program.cs

# Định dạng tệp SQL
pnpm prettier --write database\migrations\V1__initial_schema.sql
```

### 7.4. Quy Tắc Viết Mã Csharp

#### Đặt tên

| Loại                  | Quy tắc     | Ví dụ                        |
| --------------------- | ----------- | ---------------------------- |
| Lớp, Giao diện        | PascalCase  | `UserService`, `IRepository` |
| Phương thức công khai | PascalCase  | `GetUserById()`              |
| Thuộc tính công khai  | PascalCase  | `UserId`, `FullName`         |
| Biến cục bộ           | camelCase   | `userId`, `bookingList`      |
| Trường riêng tư       | \_camelCase | `_userRepository`            |
| Hằng số               | UPPER_CASE  | `MAX_CONNECTIONS`            |

#### Async/Await

*   Sử dụng `async/await` cho tất cả thao tác I/O
*   Thêm hậu tố `Async` cho phương thức bất đồng bộ

```csharp
// Đúng
public async Task<User> GetUserByIdAsync(int id)
{
    return await _repository.FindByIdAsync(id);
}

// Sai
public User GetUserById(int id)
{
    return _repository.FindByIdAsync(id).Result;
}
```

#### Null Safety

*   Bật nullable reference types
*   Sử dụng toán tử null-conditional (`?.`) và null-coalescing (`??`)

```csharp
// Trong tệp .csproj
<Nullable>enable</Nullable>

// Trong mã nguồn
string? userName = user?.Name ?? "Khách";
```

---

## 8. Xử Lý Lỗi Thường Gặp

### 8.1. Không Thể Kết Nối Cơ Sở Dữ Liệu

#### Triệu chứng

```csharp
SqlException: A network-related or instance-specific error occurred
```

#### Kiểm tra và xử lý

```powershell
# 1. Kiểm tra container đang chạy
docker ps | Select-String "ttbs-database"

# 2. Nếu không thấy, khởi động lại
cd database
docker-compose up -d
cd ..

# 3. Đợi SQL Server khởi động
Start-Sleep -Seconds 30

# 4. Kiểm tra kết nối
docker exec -it ttbs-database /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "My`$tr0ngP@ssw0rd!" -C -Q "SELECT @@VERSION"
```

### 8.2. Cổng Đã Được Sử Dụng

#### Triệu chứng

```shell
AddressAlreadyInUseException: Cổng 5000 đã được sử dụng
```

#### Kiểm tra và xử lý

```powershell
# 1. Tìm tiến trình đang dùng cổng 5000
netstat -ano | Select-String ":5000"

# 2. Tìm tên tiến trình theo PID
Get-Process -Id <PID>

# 3. Dừng tiến trình (nếu cần)
Stop-Process -Id <PID> -Force
```

Hoặc đổi cổng trong `appsettings.json`:

```json
{
 "TcpServer": {
  "Port": 5001
 }
}
```

### 8.3. Lỗi Biên Dịch WinForms

#### Triệu chứng

```csharp
error MSB4019: The imported project ... was not found
```

#### Xử lý

```powershell
# 1. Xóa thư mục bin và obj
Remove-Item -Recurse -Force frontend\admin\bin, frontend\admin\obj
Remove-Item -Recurse -Force frontend\client\bin, frontend\client\obj

# 2. Khôi phục và biên dịch lại
dotnet restore train-ticket-booking-system.slnx
dotnet build train-ticket-booking-system.slnx --no-incremental
```

### 8.4. Git Hooks Không Chạy

#### Triệu chứng

Commit được thực hiện mà không có kiểm tra mã nguồn.

#### Xử lý

```powershell
# Cài đặt lại Husky
pnpm install
pnpm prepare
```

### 8.5. Lỗi Kết Nối TCP từ Frontend

#### Triệu chứng

```csharp
SocketException: Không thể kết nối đến máy chủ
```

#### Kiểm tra

1. Đảm bảo máy chủ backend đang chạy
2. Kiểm tra cấu hình IP và cổng trong ứng dụng frontend
3. Kiểm tra tường lửa Windows

```powershell
# Cho phép cổng 5000 qua tường lửa
New-NetFirewallRule -DisplayName "TCP 5000" -Direction Inbound -Protocol TCP -LocalPort 5000 -Action Allow
```

### 8.6. Đặt Lại Cơ Sở Dữ Liệu

Khi cần xóa toàn bộ dữ liệu và bắt đầu lại:

```powershell
cd database

# Dừng và xóa container cùng dữ liệu
docker-compose down -v

# Khởi động lại container cơ sở dữ liệu
docker-compose up -d ttbs-database

# Khởi tạo cơ sở dữ liệu
.\init-database.ps1

# Chạy di chuyển Flyway
docker-compose up ttbs-flyway

cd ..
```

---

## Phụ Lục

### A. Lược Đồ Cơ Sở Dữ Liệu

#### Bảng User (Người dùng)

| Cột          | Kiểu dữ liệu  | Mô tả                    |
| ------------ | ------------- | ------------------------ |
| UserId       | INT           | Khóa chính, tự tăng      |
| Username     | NVARCHAR(50)  | Tên đăng nhập (duy nhất) |
| PasswordHash | NVARCHAR(255) | Mật khẩu đã mã hóa       |
| FullName     | NVARCHAR(100) | Họ và tên                |
| Email        | NVARCHAR(100) | Địa chỉ email            |
| PhoneNumber  | NVARCHAR(20)  | Số điện thoại            |
| Role         | NVARCHAR(20)  | Vai trò (Admin/Customer) |
| CreatedAt    | DATETIME      | Thời điểm tạo            |
| IsActive     | BIT           | Trạng thái hoạt động     |

#### Bảng Train (Chuyến tàu)

| Cột              | Kiểu dữ liệu  | Mô tả                      |
| ---------------- | ------------- | -------------------------- |
| TrainId          | INT           | Khóa chính, tự tăng        |
| TrainNumber      | NVARCHAR(20)  | Số hiệu tàu                |
| TrainName        | NVARCHAR(100) | Tên chuyến                 |
| DepartureStation | NVARCHAR(100) | Ga đi                      |
| ArrivalStation   | NVARCHAR(100) | Ga đến                     |
| DepartureTime    | DATETIME      | Thời gian khởi hành        |
| ArrivalTime      | DATETIME      | Thời gian đến              |
| TotalSeats       | INT           | Tổng số ghế (mặc định: 10) |
| TicketPrice      | DECIMAL(18,2) | Giá vé                     |
| Status           | NVARCHAR(20)  | Trạng thái                 |

#### Bảng Seat (Ghế ngồi)

| Cột         | Kiểu dữ liệu | Mô tả                     |
| ----------- | ------------ | ------------------------- |
| SeatId      | INT          | Khóa chính, tự tăng       |
| TrainId     | INT          | Khóa ngoại đến Train      |
| SeatNumber  | INT          | Số ghế (1-10)             |
| IsAvailable | BIT          | Ghế còn trống             |
| Version     | INT          | Phiên bản (khóa lạc quan) |

#### Bảng Booking (Đặt vé)

| Cột           | Kiểu dữ liệu  | Mô tả                                    |
| ------------- | ------------- | ---------------------------------------- |
| BookingId     | INT           | Khóa chính, tự tăng                      |
| UserId        | INT           | Khóa ngoại đến User                      |
| TrainId       | INT           | Khóa ngoại đến Train                     |
| SeatId        | INT           | Khóa ngoại đến Seat                      |
| BookingStatus | NVARCHAR(20)  | Trạng thái (Pending/Confirmed/Cancelled) |
| BookingDate   | DATETIME      | Thời điểm đặt                            |
| TotalAmount   | DECIMAL(18,2) | Tổng tiền                                |
| PaymentStatus | NVARCHAR(20)  | Trạng thái thanh toán                    |
| CancelledAt   | DATETIME      | Thời điểm hủy (nếu có)                   |

#### Bảng AuditLog (Nhật ký hoạt động)

| Cột        | Kiểu dữ liệu  | Mô tả               |
| ---------- | ------------- | ------------------- |
| LogId      | INT           | Khóa chính, tự tăng |
| UserId     | INT           | Người thực hiện     |
| Action     | NVARCHAR(50)  | Hành động           |
| EntityType | NVARCHAR(50)  | Loại đối tượng      |
| EntityId   | INT           | ID đối tượng        |
| Details    | NVARCHAR(MAX) | Chi tiết            |
| CreatedAt  | DATETIME      | Thời điểm           |

### B. Danh Sách Gói NuGet

#### Backend (.NET 9)

| Gói                          | Phiên bản | Mục đích              |
| ---------------------------- | --------- | --------------------- |
| BCrypt.Net-Next              | 4.0.3     | Mã hóa mật khẩu       |
| Dapper                       | 2.1.66    | Micro ORM             |
| Microsoft.Data.SqlClient     | 5.2.2     | Kết nối SQL Server    |
| Microsoft.Extensions.Hosting | 9.0.11    | Framework dịch vụ nền |
| Newtonsoft.Json              | 13.0.3    | Xử lý JSON            |

#### SDK-Client (.NET Framework 4.8.1)

| Gói             | Phiên bản | Mục đích   |
| --------------- | --------- | ---------- |
| Newtonsoft.Json | 13.0.3    | Xử lý JSON |

---

**Cập nhật lần cuối**: 30 tháng 11, 2025  
**Người bảo trì**: Nhóm phát triển
