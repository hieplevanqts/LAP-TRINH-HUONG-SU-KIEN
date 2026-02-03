# Hotel Management (C# WinForms + SQL Server)

## Noi dung
- Phan tich nghiep vu: `docs/01_phan_tich_nghiep_vu.md`
- Use case chinh: `docs/02_use_case.md`
- Phan quyen: `docs/03_phan_quyen.md`
- Script CSDL: `database/hotel_management.sql`
- Source code: `src/`

## Cach khoi tao du an (tren Windows co .NET SDK)
1. Tao solution va add projects:
   - dotnet new sln -n HotelManager
   - dotnet sln HotelManager.sln add src/HotelManager.Models/HotelManager.Models.csproj
   - dotnet sln HotelManager.sln add src/HotelManager.DAL/HotelManager.DAL.csproj
   - dotnet sln HotelManager.sln add src/HotelManager.BLL/HotelManager.BLL.csproj
   - dotnet sln HotelManager.sln add src/HotelManager.WinForms/HotelManager.WinForms.csproj
2. Chay script CSDL trong SQL Server:
   - database/hotel_management.sql
3. Cap nhat chuoi ket noi trong `src/HotelManager.WinForms/App.config`.
4. Chay WinForms:
   - dotnet run --project src/HotelManager.WinForms/HotelManager.WinForms.csproj

## Ghi chu
- WinForms chi chay tren Windows.
- DAL dung `System.Configuration.ConfigurationManager` de doc chuoi ket noi.
