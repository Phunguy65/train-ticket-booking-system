# 📘 Tài Liệu Tích Hợp Backend (TCP RPC)

Tài liệu hướng dẫn gọi API cho Frontend WinForms.

---

## 🔐 Authentication Service (Xác thực)
**Yêu cầu trước khi sử dụng:**

```csharp
using sdk_client;
using sdk_client.Services;
using sdk_client.Protocol;
using sdk_client.Exceptions; // Để bắt ApiException

var apiClient = new ApiClient("127.0.0.1", 5000);
var authService = new AuthenticationService(apiClient);
```
### 1. Đăng ký tài khoản
* **Hàm RPC:** `RegisterAsync`
* **Dùng khi nào:** Khi người dùng bấm nút "Đăng ký" trên màn hình tạo tài khoản.
* **Input:**
```json
{
  "Username": "user01",      // string (Bắt buộc, duy nhất)
  "Password": "password123",  // string (Bắt buộc)
  "FullName": "Nguyen Van A", // string (Bắt buộc)
  "Email": "a@example.com",   // string (Bắt buộc)
  "PhoneNumber": "0901234567" // string (Tùy chọn)
}
```
* **Output:**
```json
{
  "UserId": 45,             // int (ID của User vừa được tạo)
  "Message": "Registration successful" 
}
```
* **Cách dùng:**
```csharp
try
{
    // Ghi chú: Service nhận pass thường, Server lo việc Hash
    var username = "tester_" + Guid.NewGuid().ToString().Substring(0, 4);

    // 1. Gọi API qua Service (Service tự động tạo RegisterRequest DTO)
    var response = await authService.RegisterAsync(
        username,
        "SecureP@ss1",
        "Nguyen Van B",
        "b@example.com"
    );

    // 2. Thông báo thành công (response.Success == true)
    // Server trả về Data chứa UserId
    dynamic data = response.Data;
    MessageBox.Show($"Đăng ký thành công! User ID: {data.UserId}");
}
catch (ApiException apiEx)
{
    // Xử lý lỗi nghiệp vụ (VD: Username đã tồn tại, Email không hợp lệ)
    MessageBox.Show($"Đăng ký thất bại: {apiEx.Message}");
}
catch (Exception ex)
{
    MessageBox.Show($"Lỗi hệ thống không mong muốn: {ex.Message}");
}
```
### 2. Đăng nhập
* **Hàm RPC:** `LoginAsync`
* **Dùng khi nào:** Khi người dùng nhập User/Pass và bấm nút "Đăng nhập".
* **Input:**
```json
{
  "Username": "user01",
  "Password": "password123"
}
```
* **Output:**
```json
{
  "SessionToken": "eyJhbGciOiJIUzI1NiI...", // Token dùng cho các request sau
  "UserId": 45,
  "Username": "user01",
  "Role": "Customer"                       // Hoặc "Admin"
}
```
* **Cách dùng:**

```csharp
try
{
    // 1. Gọi API qua Service. Hàm này trả về LoginResponse DTO.
    var loginRes = await authService.LoginAsync("user01", "SecureP@ss1");

    // LƯU Ý: Nếu thành công, Service đã TỰ ĐỘNG gán SessionToken 
    // vào apiClient.SessionToken để dùng cho các request sau.
    
    MessageBox.Show($"Đăng nhập thành công! Role: {loginRes.Role}");
    
    // TODO: Chuyển sang màn hình chính
}
catch (ApiException apiEx)
{
    // Xử lý lỗi xác thực (VD: Sai User/Pass, tài khoản bị khóa)
    MessageBox.Show($"Đăng nhập thất bại: {apiEx.Message}");
}
catch (Exception ex)
{
    MessageBox.Show($"Lỗi kết nối hoặc hệ thống: {ex.Message}");
}
```

### 3. Đăng xuất
* **Hàm RPC:** `LogoutAsync`
* **Dùng khi nào:** Khi người dùng bấm nút "Đăng xuất" hoặc muốn thoát tài khoản.
* **Input:** k có
* **Output:**
```json
{
  "Message": "Logged out successfully."
}
```
* **Cách dùng:**

```csharp
try
{
    // 1. Gọi API qua Service.
    var response = await authService.LogoutAsync();

    // LƯU Ý: Nếu thành công, Service đã TỰ ĐỘNG xóa token khỏi apiClient.SessionToken.
    
    MessageBox.Show("Đã đăng xuất thành công!");
    
    // TODO: Chuyển về màn hình Login
}
catch (ApiException apiEx)
{
    // Lỗi xảy ra nếu token hết hạn (mặc dù Server đã cố gắng logout nó)
    MessageBox.Show($"Đăng xuất không hoàn tất: {apiEx.Message}");
}
```
## 👤 User Service (Quản lý người dùng)

**Yêu cầu trước khi sử dụng:**

```csharp
using sdk_client;
using sdk_client.Services;
using sdk_client.Protocol;
using sdk_client.Exceptions;

// Khởi tạo (đảm bảo apiClient đã Login thành công)
var apiClient = new ApiClient("127.0.0.1", 5000); // Đảm bảo đã login bằng tài khoản Admin cho các hàm Admin
var userService = new UserService(apiClient);
```

### 1. Lấy danh sách người dùng (Admin)
* **Hàm RPC:** `GetAllUsersAsync`
* **Dùng khi nào:** Hiển thị danh sách user cho Admin quản lý.
* **Input:** Server nhận tham số phân trang, nếu không gửi sẽ trả về toàn bộ danh sách (không khuyến khích).
```json
{
  "PageNumber": 1,    // int (Trang cần lấy)
  "PageSize": 20      // int (Số lượng dòng/trang)
}
```
* **Output:**
```json
{
  "TotalRecords": 150,
  "Items": [
    {
      "UserId": 105,
      "Username": "user01",
      "FullName": "Nguyen Van A",
      "Email": "a@example.com",
      "Role": "Customer",
      "IsActive": true
    }
  ]
}
```
* **Cách dùng:**
```csharp
try
{
    // 1. Gọi API qua Service, truyền tham số phân trang
    // Hàm này trả về object (Data) của Response.
    var pagedUsers = await userService.GetAllUsersAsync(pageNumber: 1, pageSize: 20);

    // 2. Chuyển đổi và hiển thị (Dùng Newtonsoft.Json)
    var usersList = JsonConvert.DeserializeObject<PagedResult<UserDTO>>(
        JsonConvert.SerializeObject(pagedUsers)
    );
    
    Console.WriteLine($"Tổng số User: {usersList.TotalRecords}");
    dgvUsers.DataSource = usersList.Items; 
}
catch (ApiException apiEx)
{
    // Xử lý lỗi: "Admin access required" hoặc "Session token is required"
    MessageBox.Show($"Lỗi truy cập danh sách: {apiEx.Message}"); 
}
catch (Exception ex)
{
    MessageBox.Show($"Lỗi hệ thống: {ex.Message}");
}
```
### 2. Cập nhật thông tin cá nhân
* **Hàm RPC:** `UpdateUserProfileAsync`
* **Dùng khi nào:** User sửa thông tin trong phần "Hồ sơ của tôi".
* **Input:** (Sử dụng `UpdateUserRequest` DTO) Gửi các trường muốn cập nhật (các trường null sẽ không được thay đổi).
```json
{
  "FullName": "Ten Moi Da Sua", 
  "Email": "email_moi@test.com",
  "PhoneNumber": "0987654321"
}
```
* **Output:**
```json
{
  "Message": "User profile updated successfully"
}
```
* **Cách dùng:**
```csharp
try
{
    // 1. Lấy dữ liệu từ form
    string newFullName = txtFullName.Text;
    string newEmail = txtEmail.Text;

    // 2. Gọi API qua Service (chỉ gửi những trường cần cập nhật)
    var response = await userService.UpdateUserProfileAsync(
        fullName: newFullName, 
        email: newEmail
        // Không truyền PhoneNumber nếu không sửa
    );

    // 3. Thông báo thành công
    MessageBox.Show("Cập nhật thông tin cá nhân thành công!");
    // Cần reload lại thông tin User trong Client nếu cần.
}
catch (ApiException apiEx)
{
    // Xử lý lỗi nghiệp vụ (VD: Email đã tồn tại, Token hết hạn)
    MessageBox.Show($"Cập nhật thất bại: {apiEx.Message}");
}
catch (Exception ex)
{
    MessageBox.Show($"Lỗi hệ thống: {ex.Message}");
}
```

### 3. Khóa / Mở khóa tài khoản (Admin)
* **Hàm RPC:** `LockUnlockUserAsync`
* **Dùng khi nào:** Admin bấm nút "Khóa" hoặc "Mở khóa" trên danh sách user.
* **Input:** (Sử dụng `LockUnlockUserRequest` DTO) Gửi ID User và trạng thái mới:
```json
{
  "UserId": 105,     // int (Bắt buộc)
  "IsActive": false  // boolean. false = Khóa (Lock), true = Mở lại (Unlock)
}
```
* **Output:**
```json
{
  "Message": "User account status updated"
}
```
* **Cách dùng:**
```csharp
try
{
    int userIdToLock = 105;
    bool shouldLock = true; // Muốn khóa tài khoản

    // 1. Xác nhận và gán trạng thái
    bool newIsActiveStatus = !shouldLock; // false nếu muốn khóa

    // 2. Gọi API qua Service
    var response = await userService.LockUnlockUserAsync(userIdToLock, newIsActiveStatus);

    // 3. Thông báo và cập nhật UI
    MessageBox.Show($"Đã khóa tài khoản ID {userIdToLock} thành công.");
    // dgvUsers.RefreshRow(userIdToLock);
}
catch (ApiException apiEx)
{
    // Xử lý lỗi (VD: Admin access required, UserId không tồn tại)
    MessageBox.Show($"Thất bại khi khóa tài khoản: {apiEx.Message}");
}
catch (Exception ex)
{
    MessageBox.Show($"Lỗi hệ thống: {ex.Message}");
}
```

## 🚂 Train Service (Quản lý Tàu)

**Yêu cầu trước khi sử dụng:**
Đảm bảo bạn đã khởi tạo `ApiClient` và `TrainService` trước khi gọi hàm:

```csharp
using sdk_client;
using sdk_client.Services;
using sdk_client.Protocol;

var apiClient = new ApiClient("127.0.0.1", 5000);
var trainService = new TrainService(apiClient);
```

### 1. Tìm kiếm tàu
* **Hàm RPC:** `SearchTrainsAsync`
* **Dùng khi nào:** Người dùng chọn Ga đi, Ga đến, Ngày đi và bấm "Tìm kiếm".
* **Input:** Server nhận một đối tượng JSON `SearchTrainRequest` (kèm phân trang):
  ```json
  {
      "DepartureStation": "Ha Noi",   // string (Optional)
      "ArrivalStation": "Sai Gon",    // string (Optional)
      "DepartureDate": "2024-05-20",  // DateTime (Optional)
      "PageNumber": 1,                // int (Optional)
      "PageSize": 20                  // int (Optional)
  }
* **Output:** Server trả về danh sách các chuyến tàu thỏa mãn điều kiện:
```json
[
    {
        "TrainId": 101,
        "TrainNumber": "SE1",
        "TrainName": "Thong Nhat Express",
        "DepartureStation": "Ha Noi",
        "ArrivalStation": "Sai Gon",
        "DepartureTime": "2024-05-20T06:00:00",
        "ArrivalTime": "2024-05-21T18:30:00",
        "TotalSeats": 200,
        "TicketPrice": 950000.0,
        "Status": "Active"
    },
    {
        "TrainId": 102,
        // ... thông tin tàu tiếp theo
    }
]
```
* **Cách dùng:**

```csharp
try 
{
    // 1. Khởi tạo Service (thường đã được tiêm hoặc khởi tạo từ trước)
    // var trainService = new TrainService(apiClient);

    // 2. Gọi hàm tìm kiếm (Sử dụng hàm của Service, KHÔNG gọi trực tiếp client)
    // Ví dụ: Tìm tàu Hà Nội -> Sài Gòn, ngày hiện tại, Trang 1, 20 kết quả.
    var resultData = await trainService.SearchTrainsAsync(
        departureStation: "Ha Noi", 
        arrivalStation: "Sai Gon", 
        departureDate: DateTime.Now, 
        pageNumber: 1, 
        pageSize: 20
    );

    // 3. Hiển thị kết quả
    // Lưu ý: TrainService.SearchTrainsAsync trả về trực tiếp Data (object),
    // ApiClient sẽ ném lỗi (Exception) nếu server trả về Success = false.
    
    if (resultData != null)
    {
        // Gán dữ liệu vào GridView
        dgvTrains.DataSource = resultData;
        
        // Nếu cần ép kiểu sang List để xử lý logic:
        // var trainList = JsonConvert.DeserializeObject<List<Train>>(resultData.ToString());
    }
    else 
    {
         MessageBox.Show("Không tìm thấy chuyến tàu nào phù hợp.");
    }
}
catch (Exception ex)
{
    // Xử lý lỗi (Ví dụ: Mất kết nối, hoặc lỗi logic từ server trả về)
    MessageBox.Show($"Lỗi tìm kiếm: {ex.Message}");
}
```

### 2. Lấy danh sách tất cả tàu
* **Hàm RPC:** `GetAllTrainsAsync`
* **Dùng khi nào:** Hiển thị danh sách toàn bộ tàu cho Admin quản lý (có phân trang).
* **Input:**
  tham số phân trang (Optional):
  ```json
  {
      "PageNumber": 1,  // int (Optional) - Trang số 1
      "PageSize": 50    // int (Optional) - 50 dòng/trang
  }
* **Output:** danh sách tàu (hoặc đối tượng phân trang `PagedResult` nếu input có phân trang):
    ```json
    [
        {
            "TrainId": 1,
            "TrainNumber": "SE1",
            "TrainName": "Thong Nhat Express",
            "DepartureStation": "Ha Noi",
            "ArrivalStation": "Sai Gon",
            "DepartureTime": "2024-01-01T06:00:00",
            "TotalSeats": 100,
            "TicketPrice": 500000.0,
            "Status": "Active"
        },
        // ... các tàu khác
    ]
    ```
* **Cách dùng:**

    ```csharp
    try 
    {
        // 1. Gọi API qua Service
        // Lấy trang 1, mỗi trang 50 dòng
        var data = await trainService.GetAllTrainsAsync(1, 50);

        // 2. Hiển thị kết quả
        // Vì ApiClient sẽ ném lỗi nếu thất bại, nên nếu chạy đến đây nghĩa là Success
        if (data != null)
        {
            dgvAllTrains.DataSource = data;
            
            // Nếu data trả về dạng PagedResult (có TotalCount), bạn có thể cần ép kiểu:
            // var pagedData = JObject.FromObject(data).ToObject<PagedResult<Train>>();
        }
        else 
        {
            MessageBox.Show("Không có dữ liệu tàu.");
        }
    }
    catch (Exception ex)
    {
        // Xử lý khi lỗi kết nối hoặc Server trả về Success = false
        MessageBox.Show($"Lỗi tải danh sách: {ex.Message}");
    }
    ```

### 3. Lấy chi tiết tàu

* **Hàm RPC:** `GetTrainByIdAsync`
* **Dùng khi nào:** Khi người dùng click vào một dòng tàu để xem chi tiết hoặc Admin muốn lấy dữ liệu cũ lên form để sửa.
* **Input:** `trainId` (int): ID duy nhất của chuyến tàu.

* **Output:**
  Đối tượng chứa thông tin chi tiết:
  ```json
  {
      "TrainId": 123,
      "TrainNumber": "SE1",
      "TrainName": "Thong Nhat Express",
      "DepartureStation": "Ha Noi",
      "ArrivalStation": "Sai Gon",
      "DepartureTime": "2024-01-01T06:00:00",
      "ArrivalTime": "2024-01-02T18:00:00",
      "TotalSeats": 100,
      "TicketPrice": 500000.0,
      "Status": "Active"
  }
* **Cách dùng:**
    ```csharp
    try 
    {
        int trainId = 123; // ID lấy từ dòng được chọn trong GridView
        
        // 1. Gọi API qua Service
        var trainInfo = await trainService.GetTrainByIdAsync(trainId);

        // 2. Map dữ liệu vào UI
        if (trainInfo != null)
        {
            // Vì dữ liệu trả về là JObject/Object, cần convert hoặc ép kiểu
            // Cách 1: Dùng dynamic (nhanh nhưng không có gợi ý code)
            dynamic train = trainInfo;
            txtTrainName.Text = train.TrainName;
            txtTrainNumber.Text = train.TrainNumber;
            
            // Cách 2: Deserialize ra Model (Khuyên dùng)
            // var trainModel = JsonConvert.DeserializeObject<Train>(trainInfo.ToString());
            // txtTrainName.Text = trainModel.TrainName;
        }
        else 
        {
            MessageBox.Show("Không tìm thấy thông tin tàu.");
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Lỗi tải chi tiết: {ex.Message}");
    }
    ```

### 4. Tạo tàu mới (Admin)
* **Hàm RPC:** `CreateTrainAsync`
* **Dùng khi nào:** Admin nhập thông tin tàu mới và bấm "Lưu".
* **Input:**
  Server nhận object `CreateTrainRequest` (không bao gồm ID vì ID tự sinh):

  ```json
  {
      "TrainNumber": "SE1",           // string (Bắt buộc)
      "TrainName": "Thong Nhat",      // string (Bắt buộc)
      "DepartureStation": "Ha Noi",   // string
      "ArrivalStation": "Sai Gon",    // string
      "DepartureTime": "2024-01-01T06:00:00", // DateTime
      "ArrivalTime": "2024-01-02T18:00:00",   // DateTime
      "TotalSeats": 100,              // int
      "TicketPrice": 500000.0         // decimal/double
  }
* **Output:** Server trả về ID của tàu vừa tạo và thông báo:
  ```json
  {
    "TrainId": 55,                 // ID của tàu mới
    "Message": "Create success"    // Thông báo từ Backend
}
  ```
* **Cách dùng:**

    ```csharp
    try 
    {
        // 1. Tạo dữ liệu từ Form nhập liệu
        // Sử dụng class CreateTrainRequest để đảm bảo đúng kiểu dữ liệu
        var newTrainRequest = new CreateTrainRequest 
        { 
            // cái này tùy ô nhập mọi người tên là gì, ở dưới đây chỉ tham khảo
            TrainNumber = txtTrainNumber.Text,
            TrainName = txtTrainName.Text,
            DepartureStation = txtDepartStation.Text,
            ArrivalStation = txtArrivalStation.Text,
            DepartureTime = dtpDepart.Value,
            ArrivalTime = dtpArrive.Value,
            TotalSeats = int.Parse(txtSeats.Text),
            TicketPrice = decimal.Parse(txtPrice.Text)
        };

        // 2. Gọi API qua Service
        // Hàm này trả về object Response chứa Success và Data
        var response = await trainService.CreateTrainAsync(newTrainRequest);

        // 3. Kiểm tra kết quả
        // Lưu ý: Nếu có lỗi (Success = false), ApiClient thường sẽ ném Exception
        // nên nếu code chạy đến dòng này nghĩa là đã thành công.
        
        MessageBox.Show("Thêm tàu thành công!");
        
        // Nếu cần lấy ID tàu vừa tạo để xử lý tiếp:
        // dynamic data = response.Data;
        // int newId = data.TrainId;
    }
    catch (Exception ex)
    {
        // Xử lý lỗi (Validation sai, trùng mã tàu, hoặc lỗi server)
        MessageBox.Show($"Thêm thất bại: {ex.Message}");
    }
    ```

### 5. Cập nhật thông tin tàu (Admin)
* **Hàm RPC:** `UpdateTrainAsync`
* **Dùng khi nào:** Admin sửa thông tin tàu (giờ chạy, giá vé...) và bấm "Cập nhật".
* **Input:**
  Server nhận object `UpdateTrainRequest` (Bắt buộc phải có `TrainId` để định danh):
  ```json
  {
      "TrainId": 123,                 // int (Bắt buộc)
      "TrainNumber": "SE1-Updated",   // string
      "TrainName": "Thong Nhat New",  // string
      "DepartureStation": "Ha Noi",   // string
      "ArrivalStation": "Da Nang",    // string
      "DepartureTime": "2024-01-01T08:00:00", // DateTime
      "ArrivalTime": "2024-01-02T12:00:00",   // DateTime
      "TotalSeats": 120,              // int
      "TicketPrice": 550000.0,        // decimal
      "Status": "Active"              // string (Active/Cancelled/Delayed)
  }
  ```
* **Output:**
    ```json
    {
        "Message": "Update success"    // Thông báo từ Backend
    }
    ```

* **Cách dùng:**

    ```csharp
    try 
    {
        // 1. Tạo dữ liệu cập nhật
        // Sử dụng class UpdateTrainRequest để đảm bảo type-safe
        var updateData = new UpdateTrainRequest 
        { 
            TrainId = 123, // ID của tàu đang sửa (lấy từ biến lưu trữ hoặc hidden field)
            TrainNumber = txtTrainNumber.Text,
            TrainName = txtTrainName.Text,
            DepartureStation = txtDepartStation.Text,
            ArrivalStation = txtArrivalStation.Text,
            DepartureTime = dtpDepart.Value,
            ArrivalTime = dtpArrive.Value,
            TotalSeats = int.Parse(txtSeats.Text),
            TicketPrice = decimal.Parse(txtPrice.Text),
            Status = cboStatus.SelectedItem.ToString() // VD: "Active"
        };

        // 2. Gọi API qua Service
        // Hàm này trả về object Response (gồm Success và Data)
        var response = await trainService.UpdateTrainAsync(updateData);

        // 3. Thông báo kết quả
        // ApiClient sẽ ném Exception nếu server trả về lỗi (Success = false)
        MessageBox.Show("Cập nhật thông tin tàu thành công!");
    }
    catch (Exception ex)
    {
        // Xử lý lỗi (VD: ID không tồn tại, dữ liệu không hợp lệ)
        MessageBox.Show($"Cập nhật thất bại: {ex.Message}");
    }
    ```
### 6. Cập nhật trạng thái tàu (Nhanh)
* **Hàm RPC:** `UpdateTrainStatusAsync`
* **Dùng khi nào:** Admin muốn thay đổi nhanh trạng thái (Hủy chuyến, Hoãn, Bảo trì...) mà không cần sửa đổi các thông tin khác (giờ chạy, tên tàu...).
* **Input:**
  Server nhận ID và Status mới:
  ```json
  {
      "TrainId": 123,         // int (Bắt buộc)
      "Status": "Cancelled"   // string (VD: Active, Cancelled, Delayed)
  }
  ```
* **Output:**
    ```json
    {
        "Message": "Update status success"
    }
    ```

* **Cách dùng:**

    ```csharp
    try 
    {
        // 1. Chuẩn bị dữ liệu
        int trainId = 123; 
        string newStatus = "Cancelled"; // Lấy từ Dropdown hoặc nút bấm

        // 2. Gọi API qua Service
        // Hàm này nhận trực tiếp 2 tham số, không cần tạo object request phức tạp
        var response = await trainService.UpdateTrainStatusAsync(trainId, newStatus);

        // 3. Thông báo kết quả
        // Nếu không có Exception nghĩa là Success = true
        MessageBox.Show($"Đã đổi trạng thái tàu {trainId} sang {newStatus}.");
    }
    catch (Exception ex)
    {
        // Xử lý lỗi (VD: Tàu không tồn tại)
        MessageBox.Show($"Lỗi cập nhật trạng thái: {ex.Message}");
    }
    ```
### 7. Xóa tàu (Admin)
* **Hàm RPC:** `DeleteTrainAsync`
* **Dùng khi nào:** Admin bấm nút "Xóa" trên danh sách tàu để loại bỏ một chuyến tàu khỏi hệ thống.
* **Input:**
  Server nhận ID của tàu cần xóa:
  ```json
  {
      "TrainId": 123      // int (Bắt buộc)
  }
  ```
* **Output:**
    ```json
    {
        "Message": "Delete success"
    }   
    ```
* **Cách dùng:**

    ```csharp
    try 
    {
        int trainId = 123; // ID lấy từ dòng đang chọn

        // 1. Xác nhận trước khi xóa (Best Practice)
        var confirm = MessageBox.Show(
            "Bạn có chắc chắn muốn xóa chuyến tàu này không?", 
            "Xác nhận xóa", 
            MessageBoxButtons.YesNo, 
            MessageBoxIcon.Warning
        );

        if (confirm == DialogResult.Yes)
        {
            // 2. Gọi API qua Service
            // Hàm DeleteTrainAsync trả về object Response
            var response = await trainService.DeleteTrainAsync(trainId);

            // 3. Thông báo thành công
            // Nếu không có Exception nghĩa là Success = true
            MessageBox.Show("Đã xóa tàu thành công!");

            // 4. Cập nhật lại giao diện (Load lại danh sách)
            // await LoadTrainList(); 
        }
    }
    catch (Exception ex)
    {
        // Xử lý lỗi (VD: Tàu đang có vé đã đặt, hoặc lỗi server)
        MessageBox.Show($"Không thể xóa: {ex.Message}");
    }
    ```

## 🎫 Booking Service (Đặt & Quản lý Vé)

Module chịu trách nhiệm hiển thị sơ đồ ghế, xử lý giao dịch đặt vé, hủy vé và xem lịch sử.

**Yêu cầu trước khi sử dụng:**

```csharp
using sdk_client;
using sdk_client.Services;
using sdk_client.Protocol;

// Khởi tạo (đảm bảo apiClient đã Login thành công)
var bookingService = new BookingService(apiClient);
```

### 1. Lấy sơ đồ ghế (Get Seat Map)
* **Hàm RPC:** `GetSeatMapAsync`
* **Dùng khi nào:** Lấy danh sách trạng thái ghế của một chuyến tàu cụ thể để hiển thị lên giao diện chọn chỗ.
* **Input:** Server nhận `TrainId` để truy vấn:

    ```json
    {
        "TrainId": 105   // int (Bắt buộc)
    }
    ```
* **Output:** Server trả về danh sách các chuyến tàu thỏa mãn điều kiện:
    ```json
    [
        {
            "SeatId": 1,
            "SeatNumber": "A1",
            "CarriageNumber": 1,
            "IsAvailable": true,    // true: Trống, false: Đã đặt
            "Price": 500000
        },
        {
            "SeatId": 2,
            "SeatNumber": "A2",
            "CarriageNumber": 1,
            "IsAvailable": false,
            "Price": 500000
        }
    ]
    ```
* **Cách dùng:**

    ```csharp
    try 
    {
        int trainId = 105;
        // Hàm này trả về object (Data), không phải Response wrapper
        var data = await bookingService.GetSeatMapAsync(trainId);

        // Convert data sang JArray hoặc List<Seat> để render UI
        var seatList = JsonConvert.DeserializeObject<List<SeatDTO>>(JsonConvert.SerializeObject(data));

        foreach(var seat in seatList) {
            Console.WriteLine($"Ghế {seat.SeatNumber}: {(seat.IsAvailable ? "Trống" : "Đã đặt")}");
        }
    }
    catch (Exception ex) 
    {
        Console.WriteLine($"Lỗi tải sơ đồ ghế: {ex.Message}");
    }
    ```

### 2. Đặt vé (Book Ticket)
* **Hàm RPC:** `BookTicketAsync`
* **Dùng khi nào:** Thực hiện đặt một ghế cụ thể cho User đang đăng nhập.
* **Input:**
  tham số phân trang (Optional):
  ```json
    {
        "TrainId": 105,      // int
        "SeatId": 1,         // int
        "SessionToken": "..." // String (Tự động inject bởi ApiClient)
    }
  ```
* **Output:** danh sách tàu (hoặc đối tượng phân trang `PagedResult` nếu input có phân trang):
    ```json
    {
        "BookingId": 8892,
        "Message": "Booking successful"
    }
    ```
* **Cách dùng:**

    ```csharp
    try 
    {
        // Gọi API
        var response = await bookingService.BookTicketAsync(trainId, seatId);

        // Kiểm tra success (Mặc dù ApiClient sẽ throw nếu false, nhưng check cho rõ luồng)
        if (response.Success) 
        {
            // Lấy BookingId từ Data trả về
            dynamic resData = response.Data;
            int bookingId = resData.BookingId;

            MessageBox.Show($"Đặt vé thành công! Mã vé: {bookingId}");
        }
    }
    catch (ApiException apiEx)
    {
        // Ví dụ: "Ghế đã có người đặt", "Phiên đăng nhập hết hạn"
        MessageBox.Show($"Lỗi đặt vé: {apiEx.Message}");
    }
    catch (Exception ex)
    {
        MessageBox.Show($"Lỗi hệ thống: {ex.Message}");
    }
    ```

### 3. Hủy vé (Cancel Booking)

* **Hàm RPC:** `CancelBookingAsync`
* **Dùng khi nào:** Hủy một vé đã đặt. Admin có thể hủy vé của bất kỳ ai, User chỉ hủy được vé của mình.
* **Input:**
    ```json
    {
        "BookingId": 8892,    // int
        "SessionToken": "..."
    }
    ```
* **Output:**
  Đối tượng chứa thông tin chi tiết:

    ```json
    {
        "Message": "Booking cancelled successfully"
    }
    ```
* **Cách dùng:**
    ```csharp
    try 
    {
        var confirm = MessageBox.Show("Bạn muốn hủy vé này?", "Xác nhận", MessageBoxButtons.YesNo);
        if (confirm == DialogResult.Yes) 
        {
            var response = await bookingService.CancelBookingAsync(bookingId);
            MessageBox.Show("Đã hủy vé thành công.");

            // Reload lại danh sách lịch sử
            await LoadHistory();
        }
    }
    catch (Exception ex) 
    {
        MessageBox.Show($"Không thể hủy vé: {ex.Message}");
    }
    ```

### 4. Lịch sử đặt vé (Get History)
* **Hàm RPC:** `GetBookingHistoryAsync`
* **Dùng khi nào:** Lấy danh sách các vé mà User hiện tại đã đặt.
* **Input:** ...
* **Output:** Server trả về ID của tàu vừa tạo và thông báo:
  ```json
    [
        {
            "BookingId": 8892,
            "TrainNumber": "SE1",
            "SeatNumber": "A1",
            "BookingDate": "2023-10-25T10:00:00",
            "Status": "Active" // hoặc "Cancelled"
        },
        ...
    ]
  ```
* **Cách dùng:**

    ```csharp
    try 
    {
        var data = await bookingService.GetBookingHistoryAsync();

        // Hiển thị lên DataGridView hoặc List
        dataGridViewHistory.DataSource = data; 
    }
    catch (Exception ex) 
    {
        Console.WriteLine($"Lỗi tải lịch sử: {ex.Message}");
    }
    ```

### 5. Quản lý toàn bộ vé (Admin - Get All Bookings)
* **Hàm RPC:** `GetAllBookingsAsync`
* **Dùng khi nào:** Lấy danh sách tất cả booking trên hệ thống. Hỗ trợ phân trang. Chỉ Admin mới gọi được.
* **Input:** Có thể gửi kèm tham số phân trang (Optional):
  ```json
    {
        "PageNumber": 1,      // int (Tùy chọn)
        "PageSize": 20,       // int (Tùy chọn)
    }
  ```
* **Output:**
    ```json
    {
        "TotalRecords": 150,
        "Items": [
            { "BookingId": 1, "Username": "user1", "Status": "Active", ... },
            { "BookingId": 2, "Username": "user2", "Status": "Cancelled", ... }
        ]
    }
    ```
    (Lưu ý: Nếu không phân trang, output sẽ là một mảng JSON phẳng) 

* **Cách dùng:**

    ```csharp
    try 
    {
        // Cách 1: Lấy tất cả (Cẩn thận nếu dữ liệu lớn)
        // var allData = await bookingService.GetAllBookingsAsync();

        // Cách 2: Phân trang (Trang 1, 50 dòng)
        var pagedData = await bookingService.GetAllBookingsAsync(1, 50);

        Console.WriteLine("Tải dữ liệu quản trị thành công");
    }
    catch (ApiException ex)
    {
        if (ex.Message.Contains("Admin access required")) 
        {
            MessageBox.Show("Bạn không có quyền truy cập chức năng này.");
        }
    }
    ```
