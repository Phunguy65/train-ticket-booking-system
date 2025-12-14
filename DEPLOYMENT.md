# Deployment Guide

This guide explains how to deploy the Train Ticket Booking System using the
automated CD pipeline.

## Table of Contents

*   [Overview](#overview)
*   [Release Process](#release-process)
*   [Installation](#installation)
*   [Docker Deployment](#docker-deployment)
*   [Manual Deployment](#manual-deployment)
*   [Troubleshooting](#troubleshooting)

## Overview

The system uses GitHub Actions for Continuous Deployment (CD) with the following
components:

*   **CI Pipeline** (`.github/workflows/ci.yml`) - Validates code on every push/PR
*   **CD Pipeline** (`.github/workflows/cd-release.yml`) - Creates releases with
  artifacts
*   **Database Migration** (`.github/workflows/database-migration.yml`) - Tests
  database changes

## Release Process

### 1. Automated Release (Recommended)

To create a new release, simply push a Git tag:

```bash
# Create a tag with version number
git tag v1.0.0

# Push the tag to trigger the CD pipeline
git push origin v1.0.0
```

**What happens automatically:**

1. ✅ CI pipeline validates the code
2. ✅ Builds backend, admin, and client applications
3. ✅ Creates self-contained Windows executables
4. ✅ Packages all artifacts as ZIP files
5. ✅ Creates a GitHub Release with release notes
6. ✅ Builds and pushes Docker image to ghcr.io

### 2. Manual Release Trigger

You can also trigger a release manually from GitHub:

1. Go to **Actions** tab in GitHub
2. Select **CD - Release** workflow
3. Click **Run workflow**
4. Enter the version number (e.g., `1.0.0`)
5. Click **Run workflow**

### Versioning Strategy

Follow **Semantic Versioning (SemVer)**:

*   **Major** (1.x.x) - Breaking changes
*   **Minor** (x.1.x) - New features, backward compatible
*   **Patch** (x.x.1) - Bug fixes

Examples:

*   `v1.0.0` - Initial release
*   `v1.0.1` - Bug fix
*   `v1.1.0` - New feature
*   `v2.0.0` - Breaking change

## Installation

### Prerequisites

*   **Windows 10/11** (x64)
*   **SQL Server 2022** (for backend)
*   **Docker** (optional, for containerized deployment)

### Download and Install

1. **Go to Releases Page**

    ```text
    https://github.com/YOUR_USERNAME/train-ticket-booking-system/releases
    ```

2. **Download Required Components**
    *   `backend-vX.X.X-win-x64.zip` - Backend server
    *   `admin-client-vX.X.X-win-x64.zip` - Admin application
    *   `customer-client-vX.X.X-win-x64.zip` - Customer application
    *   `database-migrations-vX.X.X.zip` - Database scripts

3. **Extract ZIP Files**

    ```powershell
    # Extract to desired locations
    Expand-Archive backend-v1.0.0-win-x64.zip -DestinationPath C:\TTBS\Backend
    Expand-Archive admin-client-v1.0.0-win-x64.zip -DestinationPath C:\TTBS\Admin
    Expand-Archive customer-client-v1.0.0-win-x64.zip -DestinationPath C:\TTBS\Client
    ```

4. **Setup Database**

    ```powershell
    cd database
    docker compose up -d ttbs-database
    docker compose up ttbs-flyway
    ```

5. **Configure Backend** Edit `appsettings.json` in backend folder:

    ```json
    {
     "ConnectionStrings": {
      "DefaultConnection": "Server=localhost,8666;Database=TrainTicketBooking;..."
     }
    }
    ```

6. **Run Applications**

    ```powershell
    # Start backend
    cd C:\TTBS\Backend
    .\backend.exe

    # Start admin (in another terminal)
    cd C:\TTBS\Admin
    .\admin.exe

    # Start client (in another terminal)
    cd C:\TTBS\Client
    .\client.exe
    ```

## Docker Deployment

### Pull Docker Image

```bash
# Pull latest version
docker pull ghcr.io/YOUR_USERNAME/train-ticket-booking-system/backend:latest

# Or specific version
docker pull ghcr.io/YOUR_USERNAME/train-ticket-booking-system/backend:1.0.0
```

### Run with Docker Compose

Create `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
    database:
        image: mcr.microsoft.com/mssql/server:2022-latest
        environment:
            - ACCEPT_EULA=Y
            - SA_PASSWORD=YourStrongPassword!
        ports:
            - '1433:1433'
        volumes:
            - db-data:/var/opt/mssql

    backend:
        image: ghcr.io/YOUR_USERNAME/train-ticket-booking-system/backend:latest
        environment:
            - ConnectionStrings__DefaultConnection=Server=database,1433;Database=TrainTicketBooking;User
              Id=sa;Password=YourStrongPassword!;TrustServerCertificate=True
            - TcpServer__Host=0.0.0.0
            - TcpServer__Port=5000
            - SignalR__Host=0.0.0.0
            - SignalR__Port=5001
        ports:
            - '5000:5000'
            - '5001:5001'
        depends_on:
            - database

volumes:
    db-data:
```

Run the stack:

```bash
docker compose -f docker-compose.prod.yml up -d
```

## Manual Deployment

### Build from Source

If you need to build manually:

```powershell
# Clone repository
git clone https://github.com/YOUR_USERNAME/train-ticket-booking-system.git
cd train-ticket-booking-system

# Restore dependencies
dotnet restore train-ticket-booking-system.slnx

# Build backend
dotnet publish backend/backend.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  --output ./publish/backend

# Build admin client
dotnet publish frontend/admin/admin.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  --output ./publish/admin

# Build customer client
dotnet publish frontend/client/client.csproj `
  --configuration Release `
  --runtime win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  --output ./publish/client
```

### Deploy to Windows Server

1. **Copy files to server**

    ```powershell
    # Using PowerShell remoting
    Copy-Item -Path .\publish\* -Destination \\server\C$\TTBS -Recurse
    ```

2. **Install as Windows Service** (optional)

    ```powershell
    # Using NSSM (Non-Sucking Service Manager)
    nssm install TTBSBackend "C:\TTBS\backend\backend.exe"
    nssm start TTBSBackend
    ```

## Troubleshooting

### Release Creation Failed

**Issue**: GitHub Actions workflow fails during release creation

**Solution**:

1. Check that you have proper permissions:
    *   Go to repository Settings → Actions → General
    *   Under "Workflow permissions", select "Read and write permissions"
2. Verify the tag format is correct: `vX.Y.Z` (e.g., `v1.0.0`)
3. Check workflow logs for specific errors

### Docker Build Failed

**Issue**: Docker image build fails

**Solution**:

1. Ensure Dockerfile is in the repository root
2. Check .dockerignore doesn't exclude necessary files
3. Verify .NET SDK version matches (9.0)
4. Check Docker build logs for specific errors

### Database Migration Failed

**Issue**: Flyway migrations fail during CI/CD

**Solution**:

1. Check migration file naming: `V1__description.sql`
2. Verify SQL syntax is correct for SQL Server 2022
3. Ensure no duplicate version numbers
4. Check Flyway logs: `docker logs ttbs-flyway`

### Application Won't Start

**Issue**: Published application fails to start

**Solution**:

1. Check appsettings.json configuration
2. Verify connection string to database
3. Ensure SQL Server is running and accessible
4. Check Windows Event Viewer for errors
5. Run with `--environment Development` for detailed logs

### CI Build Failures

**Issue**: CI pipeline fails on pull request

**Solution**:

1. Run locally first:

    ```powershell
    # Format check
    dotnet format train-ticket-booking-system.slnx --verify-no-changes

    # Linting
    pnpm biome check "**/*.{js,ts,jsx,tsx,json}"

    # Build
    dotnet build train-ticket-booking-system.slnx
    ```

2. Fix any reported issues before pushing

### Permission Denied on ghcr.io

**Issue**: Cannot push Docker image to GitHub Container Registry

**Solution**:

1. Enable GitHub Container Registry in repository settings
2. Make package public or authenticate properly
3. Check GitHub Actions has package write permission

## Environment Variables

### Backend Configuration

```bash
# Database
ConnectionStrings__DefaultConnection="Server=..."

# TCP Server
TcpServer__Host="0.0.0.0"
TcpServer__Port=5000

# SignalR
SignalR__Host="0.0.0.0"
SignalR__Port=5001

# Security
Security__SessionTimeout=30

# Booking
Booking__SeatHoldTimeoutMinutes=1
```

### Admin Client Configuration

```json
{
 "AppOptions": {
  "ServerHost": "localhost",
  "ServerPort": 5000,
  "SignalRUrl": "http://localhost:5001/bookingHub"
 }
}
```

## Monitoring & Logs

### Check Application Status

```powershell
# Backend
curl http://localhost:5001/health

# Check logs
Get-Content C:\TTBS\Backend\logs\*.log -Tail 50
```

### Docker Logs

```bash
# Backend logs
docker logs ttbs-backend -f

# Database logs
docker logs ttbs-database -f
```

## Rollback Procedure

If a deployment fails:

1. **Quick Rollback**

    ```powershell
    # Stop current version
    Stop-Process -Name backend -Force

    # Restore previous version
    Copy-Item C:\TTBS\Backup\backend\* C:\TTBS\Backend\ -Force

    # Restart
    Start-Process C:\TTBS\Backend\backend.exe
    ```

2. **Database Rollback**

    ```bash
    # Use Flyway undo (if available)
    flyway undo -url=jdbc:sqlserver://... -user=sa -password=...
    ```

3. **Git Tag Rollback**

    ```bash
    # Delete bad tag
    git tag -d v1.0.1
    git push origin :refs/tags/v1.0.1

    # Create new release from previous good version
    git tag v1.0.2
    git push origin v1.0.2
    ```

## Best Practices

1. **Always test in staging first**
2. **Backup database before migrations**
3. **Use semantic versioning**
4. **Document breaking changes in release notes**
5. **Monitor logs after deployment**
6. **Keep previous version as backup**
7. **Run health checks after deployment**

## Support

For issues or questions:

*   **GitHub Issues**:
  https://github.com/YOUR_USERNAME/train-ticket-booking-system/issues
*   **Documentation**: See [CLAUDE.md](./CLAUDE.md)
*   **Contributing**: See [CONTRIBUTING.md](./.github/CONTRIBUTING.md)
