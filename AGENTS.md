# AGENTS.md - Train Ticket Booking System

## Overview

**Project**: High-speed train ticket booking simulation system  
**Purpose**: Simulate train ticket booking with 10 seats, user
registration/login  
**Users**: Admin, Customer  
**Architecture**: Client-Server with TCP/Socket communication

### Technology Stack

*   **Backend**: .NET 9 Background Service, TCP/Socket server, Dapper ORM
*   **Database**: Microsoft SQL Server 2022
*   **Frontend**: Windows Forms 4.8.1 (Client), Avalonia UI (Admin), TCP/Socket
  communication
*   **Development Tools**: pnpm, Biome, Prettier, Husky, lint-staged

---

## Repository Structure

```powershell
train-ticket-booking-system/
├── backend/                    # .NET 9 Background Service (TCP Server)
│   ├── Program.cs             # Entry point
│   ├── Worker.cs              # Background service implementation
│   ├── backend.csproj         # .NET 9 project file
│   └── appsettings.json       # Configuration
├── frontend/
│   ├── admin/                 # Admin Avalonia app (.NET 9)
│   │   ├── Program.cs         # Entry point
│   │   ├── MainWindow.axaml   # Main window
│   │   └── admin.csproj       # Avalonia project
│   └── client/                # Customer WinForms app (.NET Framework 4.8.1)
│       ├── Program.cs         # Entry point
│       ├── Form1.cs           # Main form
│       └── client.csproj      # WinForms project
├── database/
│   ├── docker-compose.yml     # SQL Server 2022 container
│   └── sql/
│       └── schema.sql         # Database schema
└── train-ticket-booking-system.slnx  # Solution file
```

---

## Prerequisites

### Required Software

*   **.NET 9 SDK** - For backend service
*   **.NET Framework 4.8.1 Developer Pack** - For WinForms clients
*   **Avalonia UI** - For admin clients
*   **SQL Server 2022** - Database (via Docker or local installation)
*   **Docker Desktop** - For containerized SQL Server
*   **Node.js 18+** - For development tools (pnpm, linting)
*   **pnpm 10.23.0** - Package manager for dev dependencies
*   **Visual Studio 2022** or **Rider** - Recommended IDE

### Verify Installation

```powershell
# Check .NET versions
dotnet --list-sdks
dotnet --list-runtimes

# Check Docker
docker --version

# Check Node.js and pnpm
node --version
pnpm --version
```

---

## Setup Instructions

### 1. Install Dependencies

```powershell
# Install Node.js development dependencies
pnpm install

# Restore .NET dependencies
dotnet restore train-ticket-booking-system.slnx
```

### 2. Database Setup

#### Using Docker with Flyway (Recommended)

```powershell
# Navigate to database directory
cd database

# Start SQL Server container
docker-compose up -d ttbs-database

# Verify container is running
docker ps | Select-String "ttbs-database"

# Initialize database (creates TrainTicketBooking database)
.\init-database.ps1

# Run Flyway migrations
docker-compose up ttbs-flyway

# Or start all services together
docker-compose up -d
```

**Connection String**:
`Server=localhost,8666;Database=TrainTicketBooking;User Id=sa;Password=MyStr0ngP@ssw0rd!;TrustServerCertificate=True;`

**Migration Files Location**: `database/migrations/`

*   `V1__initial_schema.sql` - Core database schema
*   `V2__add_pagination_indexes.sql` - Performance indexes
*   `V3__add_cascade_delete.sql` - CASCADE DELETE constraints

### 3. Build Projects

```powershell
# Build entire solution
dotnet build train-ticket-booking-system.slnx --configuration Debug

# Or build individual projects
dotnet build backend/backend.csproj
dotnet build frontend/admin/admin.csproj
dotnet build frontend/client/client.csproj
```

---

## Running the System

### Start Backend Server

```powershell
cd backend
dotnet run
```

**Expected Output**: TCP server listening on configured port (default: 5000)

### Start Admin Application

```powershell
# Using dotnet
cd frontend/admin
dotnet run

# Or run compiled executable
.\frontend\admin\bin\Debug\admin.exe
```

### Start Client Application

```powershell
# Using dotnet
cd frontend/client
dotnet run

# Or run compiled executable
.\frontend\client\bin\Debug\client.exe
```

---

## Database Schema

### Tables

1. **User** - User accounts (Admin/Customer)
    *   UserId, Username, PasswordHash, FullName, Email, PhoneNumber, Role,
      CreatedAt, IsActive
2. **Train** - Train schedules
    *   TrainId, TrainNumber, TrainName, DepartureStation, ArrivalStation,
      DepartureTime, ArrivalTime, TotalSeats (default: 10), TicketPrice, Status
3. **Seat** - Seat availability per train
    *   SeatId, TrainId, SeatNumber, IsAvailable, Version (for optimistic locking)
4. **Booking** - Ticket bookings
    *   BookingId, UserId, TrainId, SeatId, BookingStatus, BookingDate,
      TotalAmount, PaymentStatus, CancelledAt
5. **AuditLog** - Transaction history
    *   LogId, UserId, Action, EntityType, EntityId, Details, CreatedAt

### Key Constraints

*   10 seats per train (enforced by default constraint)
*   Optimistic concurrency control on Seat table (Version column)
*   Role-based access: Admin, Customer
*   Booking statuses: Pending, Confirmed, Cancelled
*   Payment statuses: Pending, Paid, Refunded
*   **CASCADE DELETE enabled**: Deleting a Train automatically deletes all related
  Seats and Bookings

### Foreign Key Cascade Behavior

**Deletion Chain:**

```text
DELETE Train (TrainId)
    ↓ ON DELETE CASCADE
DELETE all Seats (TrainId = deleted train)
    ↓ ON DELETE CASCADE
DELETE all Bookings (SeatId = deleted seats)
```

**Foreign Key Constraints with CASCADE DELETE:**

*   `FK_Seat_TrainId_Train`: Seat.TrainId → Train.TrainId (ON DELETE CASCADE)
*   `FK_Booking_TrainId_Train`: Booking.TrainId → Train.TrainId (ON DELETE
  CASCADE)
*   `FK_Booking_SeatId_Seat`: Booking.SeatId → Seat.SeatId (ON DELETE CASCADE)

**⚠️ Important Notes:**

*   Deleting a train **permanently removes** all associated seats and bookings
*   Booking history is **not preserved** after train deletion
*   Use with caution in production environments
*   Consider implementing soft delete (status change) instead of hard delete for
  audit trail preservation
*   The `AuditLog` table records train deletion events for tracking purposes

---

## Development Workflow

### Code Quality Tools

```powershell
# Format code (runs automatically on save)
pnpm biome check --fix "**/*.{js,ts,jsx,tsx}"
pnpm prettier --write "**/*.{json,yaml,yml,sql}"

# Format C# code
dotnet format train-ticket-booking-system.slnx

# Lint Markdown
pnpm markdownlint-cli2 --fix "**/*.md"
```

### Git Hooks (Husky + lint-staged)

Pre-commit hooks automatically run:

*   Biome for JS/TS files
*   Prettier for JSON/YAML/SQL/Markdown
*   dotnet format for C# files
*   Markdownlint for Markdown files

### Commit Message Convention

Follow Conventional Commits:

```powershell
feat: add user authentication
fix: resolve seat booking race condition
docs: update AGENTS.md
chore: update dependencies
```

---

## Testing

### Backend Tests

```powershell
cd backend
dotnet test
```

### Frontend Tests

```powershell
# Admin app tests
cd frontend/admin
dotnet test

# Client app tests
cd frontend/client
dotnet test
```

---

## Architecture Guidelines

### Backend (TCP Server)

*   **Pattern**: Background Service with TCP listener
*   **ORM**: Dapper for database access
*   **Concurrency**: Handle multiple client connections
*   **Protocol**: Define custom TCP message protocol (JSON/Binary)
*   **Logging**: Use ILogger for structured logging

### Frontend (WinForms)

*   **Pattern**: Event-driven UI with TCP client
*   **Communication**: Async TCP socket operations
*   **State Management**: Local state per form
*   **Error Handling**: User-friendly error messages

### Database Access

*   **ORM**: Dapper (micro-ORM)
*   **Transactions**: Use for booking operations
*   **Concurrency**: Optimistic locking on Seat table
*   **Connection Pooling**: Enabled by default

---

## Common Tasks

### Add New Database Migration

```powershell
# Create new Flyway migration file
# Follow naming convention: V<VERSION>__<description>.sql
New-Item -Path "database\migrations\V4__your_migration_description.sql" -ItemType File

# Write your SQL migration
# Example: ALTER TABLE, CREATE INDEX, etc.

# Run Flyway migration
cd database
docker-compose up ttbs-flyway

# Verify migration was applied
docker exec -it ttbs-database /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "MyStr0ngP@ssw0rd!" -d TrainTicketBooking -Q "SELECT * FROM flyway_schema_history ORDER BY installed_rank" -C
```

### View Migration History

```powershell
# Navigate to database directory
cd database

# Check Flyway migration history
docker exec -it ttbs-database /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "MyStr0ngP@ssw0rd!" -d TrainTicketBooking -Q "SELECT installed_rank, version, description, script, installed_on, success FROM flyway_schema_history ORDER BY installed_rank" -C
```

### Add NuGet Package to Backend

```powershell
cd backend
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
```

### Add Reference to WinForms Project

```powershell
# Admin app
cd frontend/admin
dotnet add reference ..\..\backend\backend.csproj

# Client app
cd frontend/client
dotnet add reference ..\..\backend\backend.csproj
```

### Reset Database

```powershell
cd database

# Stop and remove container with volumes
docker-compose down -v

# Restart database container
docker-compose up -d ttbs-database

# Initialize database
.\init-database.ps1

# Run Flyway migrations
docker-compose up ttbs-flyway
```

---

## Coding Conventions

### C# Style

*   **Indentation**: Tabs (4 spaces)
*   **Naming**: PascalCase for public members, camelCase for private
*   **Async**: Use async/await for I/O operations
*   **Null Safety**: Enable nullable reference types
*   **Comments**: Explain "what" and "why", not "how"

### SQL Style

*   **Keywords**: UPPERCASE
*   **Identifiers**: PascalCase for tables/columns
*   **Formatting**: Use Prettier with prettier-plugin-sql

### JavaScript/TypeScript

*   **Formatter**: Biome
*   **Style**: Single quotes, no semicolons (enforced by Biome)

---

## Troubleshooting

### Backend Won't Start

```powershell
# Check port availability
netstat -ano | Select-String ":5000"

# Check logs
cd backend
dotnet run --verbosity detailed
```

### Database Connection Failed

```powershell
# Verify SQL Server is running
docker ps | Select-String "ttbs-database"

# Test connection
docker exec -it ttbs-database /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "MyStr0ngP@ssw0rd!" -C -Q "SELECT @@VERSION"
```

### WinForms Build Errors

```powershell
# Clean and rebuild
dotnet clean frontend/admin/admin.csproj
dotnet build frontend/admin/admin.csproj --no-incremental
```

---

## Environment Variables

### Backend (appsettings.json)

```json
{
 "ConnectionStrings": {
  "DefaultConnection": "Server=localhost,8666;Database=TrainTicketBooking;User Id=sa;Password=MyStr0ngP@ssw0rd!;TrustServerCertificate=True;"
 },
 "TcpServer": {
  "Port": 5000,
  "MaxConnections": 100
 }
}
```

### Database (docker-compose.yml)

*   `DB_PORT`: SQL Server port (default: 8666)
*   `SA_PASSWORD`: SA user password (default: MyStr0ngP@ssw0rd!)

---

## Security Notes

*   **Passwords**: Hash using BCrypt or PBKDF2
*   **SQL Injection**: Use parameterized queries (Dapper handles this)
*   **TCP Security**: Consider TLS for production
*   **Connection Strings**: Use User Secrets for sensitive data

---

## Performance Considerations

*   **Connection Pooling**: Enabled by default in SQL Server
*   **Async I/O**: Use async/await for all network operations
*   **Indexing**: Indexes created on frequently queried columns
*   **Optimistic Locking**: Version column prevents race conditions

---

## References

*   [.NET 9 Documentation](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
*   [Dapper Documentation](https://github.com/DapperLib/Dapper)
*   [Windows Forms .NET Framework](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
*   [Avalonia UI Framework](https://docs.avaloniaui.net/docs/basics/)
*   [SQL Server 2022 Docker](https://hub.docker.com/_/microsoft-mssql-server)

---

## Agent Instructions

### For Code Generation

1. **Backend**: Implement TCP server in Worker.cs, use Dapper for DB access
2. **Frontend**: Create forms for login, registration, booking in WinForms
3. **Database**: Follow existing schema, use transactions for bookings
4. **Testing**: Write unit tests for business logic, integration tests for DB

### For Debugging

1. Check backend logs in console output
2. Verify database connectivity using sqlcmd
3. Use Visual Studio debugger for WinForms apps
4. Monitor TCP traffic with Wireshark if needed

### For Deployment

1. Publish backend: `dotnet publish -c Release`
2. Publish WinForms: Build in Release mode
3. Package with database scripts
4. Document connection string configuration

---

**Last Updated**: 2025-11-27 **Maintainer**: Development Team
