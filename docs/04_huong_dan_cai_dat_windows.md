# Huong dan cai dat moi truong va chay ung dung (Windows + SQL Server Docker)

## Yeu cau
- Windows 10/11 64-bit.
- Docker Desktop (WSL2 backend).
- .NET SDK 8.
- Visual Studio 2022 (Workload: Desktop development with .NET).
- Azure Data Studio hoac SSMS de quan ly SQL Server.

## Buoc 1: Cai dat Docker Desktop
1. Cai Docker Desktop va bat WSL2.
2. Khoi dong Docker Desktop.
3. Kiem tra Docker hoat dong:

```bash
docker version
```

## Buoc 2: Tao va chay SQL Server bang docker-compose
1. Tao file `docker-compose.yml` o thu muc goc du an voi noi dung:

```yaml
version: "3.9"
services:
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: hotel_sqlserver
    ports:
      - "1433:1433"
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: "YourStrong!Passw0rd"
    volumes:
      - sqlserver_data:/var/opt/mssql

volumes:
  sqlserver_data:
```

2. Chay SQL Server:

```bash
docker-compose up -d
```

3. Kiem tra container:

```bash
docker ps
```

## Buoc 3: Tao CSDL va bang
1. Mo Azure Data Studio hoac SSMS.
2. Ket noi toi SQL Server:
   - Server: `localhost,1433`
   - Login: `sa`
   - Password: `YourStrong!Passw0rd`
3. Mo file `database/hotel_management.sql` va chay script.

## Buoc 4: Cau hinh ket noi trong WinForms
Sua `src/HotelManager.WinForms/App.config`:

```xml
<add name="HotelDb"
     connectionString="Server=localhost,1433;Database=HotelManagement;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;"
     providerName="System.Data.SqlClient" />
```

## Buoc 5: Tao solution va chay ung dung
1. Tao solution va add projects:

```bash
dotnet new sln -n HotelManager
dotnet sln HotelManager.sln add src/HotelManager.Models/HotelManager.Models.csproj
dotnet sln HotelManager.sln add src/HotelManager.DAL/HotelManager.DAL.csproj
dotnet sln HotelManager.sln add src/HotelManager.BLL/HotelManager.BLL.csproj
dotnet sln HotelManager.sln add src/HotelManager.WinForms/HotelManager.WinForms.csproj
```

2. Chay WinForms:

```bash
dotnet run --project src/HotelManager.WinForms/HotelManager.WinForms.csproj
```

## Loi thuong gap
- Sai password SQL Server: dam bao `MSSQL_SA_PASSWORD` dung.
- Khong ket noi duoc: kiem tra Docker Desktop dang chay va port 1433 khong bi chiem.
- WinForms chi chay tren Windows.
