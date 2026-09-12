# WareStockApi

Backend API ของ WareStock สร้างด้วย .NET 10 ตามแนวทาง Clean Architecture

> โปรเจคนี้เป็น backend ให้กับหน้าเว็บ [WareStockWeb](../WareStockWeb) — ต้องรันโปรเจคนี้ก่อนแล้วค่อยรันฝั่งเว็บ

## Tech Stack / Libraries หลัก

- **Framework:** .NET 10, โครงสร้างแบบ Clean Architecture (`Domain` → `Application` → `Infrastructure` → `Web`)
- **Orchestration:** .NET Aspire (`AppHost`) — สร้างและจัดการ dependency (เช่น SQL Server container) ให้อัตโนมัติตอน dev
- **Database:** Entity Framework Core + SQL Server
- **Auth:** ASP.NET Core Identity + JWT Bearer Authentication
- **อื่นๆ:** MediatR (CQRS), AutoMapper, FluentValidation, Ardalis.GuardClauses
- **API Docs:** Scalar (เอกสาร API แบบ interactive ที่ path `/scalar`)

## สิ่งที่ต้องติดตั้งก่อน (Prerequisites)

- [.NET SDK 10.0.201](https://dotnet.microsoft.com/) ขึ้นไป (ดูเวอร์ชันที่ต้องใช้ได้ใน `global.json`)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) — ต้องเปิดทิ้งไว้ เพราะ Aspire จะสร้าง SQL Server container ให้อัตโนมัติ

## Build

```bash
dotnet build
```

## วิธีรัน

1. เปิด Docker Desktop ทิ้งไว้ (Aspire ต้องใช้สร้าง SQL Server container)

2. รัน Backend API — เลือกวิธีใดวิธีหนึ่ง:

   - **ผ่าน AppHost (แนะนำ)**

     ```bash
     dotnet run --project .\src\AppHost
     ```

     Aspire dashboard จะเปิดขึ้นมาอัตโนมัติ แสดง URL และ log ของแต่ละ service ที่รันอยู่แบบ real-time

   - **หรือรัน Web API แบบ standalone** (ไม่ผ่าน Aspire dashboard)

     ```bash
     cd src/Web
     dotnet run
     ```

   ทั้งสองวิธีพอร์ตของ Web API ถูก fix ไว้แล้วที่ `http://localhost:5164` (หรือ `https://localhost:7145`) เสมอ (ดูใน `src/AppHost/Program.cs` และ `src/Web/Properties/launchSettings.json`)

3. ตรวจสอบว่า API พร้อมใช้งาน — เปิด `http://localhost:5164/scalar` ต้องขึ้นหน้าเอกสาร API (Scalar) โดยไม่ต้องเปิด Aspire dashboard มาหา URL ก่อน

4. รันฝั่งเว็บ ([WareStockWeb](../WareStockWeb)) ต่อ — ค่า default ของ `VITE_API_URL` (`http://localhost:5164/v1`) ตรงกับพอร์ตนี้อยู่แล้ว ไม่ต้องแก้ ดูขั้นตอนรันฝั่งเว็บได้ในไฟล์ README ของโปรเจคนั้น

## Config ที่เกี่ยวข้อง

ค่าใน `src/Web/appsettings.json` มีค่า default สำหรับ dev มาให้แล้ว ปกติไม่ต้องแก้:

- `ConnectionStrings:WareStockApiDb` — connection string ของฐานข้อมูล (ใช้เมื่อรัน `src/Web` แบบ standalone; ถ้ารันผ่าน AppHost ค่านี้จะถูก Aspire override ให้ชี้ไปที่ container ที่สร้างขึ้นแทน)
- `Jwt:Issuer`, `Jwt:Audience`, `Jwt:Secret`, `Jwt:AccessTokenMinutes`, `Jwt:RefreshTokenDays` — ค่า dev-only ใส่มาให้แล้ว ใช้ได้ทันที (ห้ามใช้ค่านี้บน production)

## Database

โดย default `AppHost` จะสร้าง SQL Server **container** ให้อัตโนมัติตอนสตาร์ท (ต้องเปิด Docker ไว้) — ไม่ต้อง setup อะไรเพิ่ม และฐานข้อมูลจะถูก drop/recreate/reseed ใหม่ทุกครั้งที่รันในโหมด Development (ดูที่ `ApplicationDbContextInitialiser`)

ถ้ามี SQL Server ของตัวเองอยู่แล้ว (ติดตั้งในเครื่อง, container เดิม, หรือ Azure SQL) และต้องการใช้แทน ให้ตั้งค่าผ่าน .NET user-secrets (เก็บเฉพาะเครื่องตัวเอง ไม่ถูก commit ขึ้น git):

```bash
cd src/AppHost
dotnet user-secrets set "ConnectionStrings:WareStockApiDb" "Server=localhost;Database=WareStockApiDb;User Id=sa;Password=<your-password>;TrustServerCertificate=True"
```

จากนั้นแก้ `src/AppHost/Program.cs` ให้อ้างอิง connection string แทนการสร้าง container:

```csharp
// แทนที่: builder.AddAzureSqlServer(...).RunAsContainer(...).AddDatabase(...)
var databaseServer = builder.AddConnectionString(Services.Database);
```

นักพัฒนาแต่ละคนที่ต้องการใช้ SQL Server ของตัวเองต้องรันคำสั่ง `dotnet user-secrets set` ข้างต้นด้วยข้อมูลของตัวเอง (เก็บแยกเฉพาะเครื่อง ไม่แชร์ผ่าน git)

## บัญชีทดสอบที่ seed มาให้อัตโนมัติ

ในโหมด Development ฐานข้อมูลจะถูก reseed ด้วยข้อมูลตัวอย่างทุกครั้งที่สตาร์ท พร้อมบัญชีสำหรับล็อกอินทันที:

| Field    | Value                     |
| -------- | ------------------------- |
| Email    | `administrator@localhost` |
| Password | `Administrator1!`         |

เรียก `POST /v1/auth/login` ด้วยข้อมูลนี้ (หรือกดปุ่ม **Authorize** ใน Scalar ที่ `/scalar`) เพื่อรับ bearer token นอกจากนี้ยังมี demo user เพิ่มอีก 2 คน (`jane.doe`, `john.smith` รหัสผ่าน `Password1!`) เพื่อให้หน้า list, dashboard และ chat มีข้อมูลตัวอย่างให้ใช้งานทันที

## Code Style & Formatting

โปรเจคใช้ [EditorConfig](https://editorconfig.org/) เพื่อรักษารูปแบบโค้ดให้เหมือนกันในทุก editor/IDE — ดูรายละเอียดได้ที่ไฟล์ `.editorconfig`

## Code Scaffolding

โปรเจคมีเครื่องมือช่วย generate command/query ใหม่ (เริ่มจากโฟลเดอร์ `src/Application/`):

สร้าง command ใหม่:

```bash
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

สร้าง query ใหม่:

```bash
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

ถ้าเจอ error *"No templates or subcommands found matching: 'ca-usecase'."* ให้ติดตั้ง template แล้วลองใหม่:

```bash
dotnet new install Clean.Architecture.Solution.Template::10.8.0
```

## Test

โปรเจคมี unit, integration และ functional tests

```bash
dotnet test
```

## Help

ดูข้อมูลเพิ่มเติมเกี่ยวกับ template ที่ใช้สร้างโปรเจคนี้ได้ที่ [project website](https://cleanarchitecture.jasontaylor.dev)
