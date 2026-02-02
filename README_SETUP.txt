BANKLITE API - FILE PACK

1) Create an ASP.NET Core Web API project in Visual Studio named: BankLite.Api
2) Replace/add the files from this folder into your project root.
   - Program.cs, appsettings.json
   - Data/, Models/, DTOs/, Controllers/
3) Install NuGet packages:
   - Microsoft.EntityFrameworkCore.SqlServer
   - Microsoft.EntityFrameworkCore.Design
   - Microsoft.AspNetCore.Authentication.JwtBearer
   - BCrypt.Net-Next

4) Migration:
   Add-Migration InitialCreate
   Update-Database

5) Run and open Swagger. Test:
   POST /api/auth/register
   POST /api/auth/login
   Click Authorize -> 'Bearer <token>'
   POST /api/accounts (open)
   GET /api/accounts/my (list)
   POST /api/accounts/{id}/close

Note: Connection string assumes local SQL Server. If needed:
Server=localhost\SQLEXPRESS;Database=BankLiteDb;Trusted_Connection=True;TrustServerCertificate=True;
