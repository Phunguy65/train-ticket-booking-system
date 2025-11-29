#!/bin/bash

# Start SQL Server dưới dạng background
/opt/mssql/bin/sqlserver &
pid=$!

# Hàm kiểm tra xem SQL Server đã sẵn sàng chưa
function wait_for_mssql() {
    echo "Đang chờ SQL Server khởi động..."
    # Thử login mỗi giây, tối đa 60 lần
    for i in {1..60}; do
        /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -Q "SELECT 1" > /dev/null 2>&1
        if [ $? -eq 0 ]; then
            echo "SQL Server đã sẵn sàng!"
            return 0
        fi
        sleep 1
    done
    echo "Lỗi: SQL Server không khởi động kịp thời gian."
    return 1
}

# Đợi server lên
wait_for_mssql

# Chạy lần lượt các file .sql tìm thấy trong thư mục /docker-entrypoint-initdb.d/
# 'sort' đảm bảo thứ tự 01, 02, 03...
echo "Bắt đầu chạy các script khởi tạo..."

for f in $(find /docker-entrypoint-initdb.d -name "*.sql" | sort); do
    echo "Đang thực thi file: $f"
    /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$SA_PASSWORD" -d TrainTicketBooking -i "$f"
    
    # Kiểm tra lỗi (optional)
    if [ $? -eq 0 ]; then
        echo "  -> Thành công: $f"
    else
        echo "  -> THẤT BẠI: $f"
    fi
done

echo "Hoàn tất khởi tạo. Container đang chạy..."
wait $pid
