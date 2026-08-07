# Blood Donation Management System

An ASP.NET Core MVC web application for managing blood donors, donation records, and
staff accounts for a blood bank. Built as an AIUB lab task.

---

## Features

### Dashboard (`/Home/Index`)
- Total donors, total donations, total blood collected (mL), distinct blood groups
- Donations in the last 30 days
- Donor count breakdown per blood group
- 5 most recent donations
- Blood groups that currently have **no** registered donors

### Donor Management (`/Donor`)
- Full CRUD (Create, Read, Edit, Delete)
- Search and filter by name, blood group, and city
- Paginated listing
- **Recent Donors** — donors who donated most recently
- **Donation Count** — number of donations per donor

### Donation Management (`/Donation`)
- Full CRUD
- Filter by donor, date range (from/to), and camp name
- Paginated listing
- **Total Blood Collected** report
- A donor's `LastDonationDate` is recalculated automatically when their donations change

### User Management (`/User`) — Admin only
- Create, edit, and delete staff/admin accounts
- Passwords hashed with BCrypt
- Accounts can be deactivated (`IsActive`) instead of deleted

### Security
- Cookie-based authentication (8-hour sliding expiration)
- Role-based authorization — `Admin` and `Staff`
- Antiforgery token validation applied globally to every POST
- Custom `403` (Access Denied) and `404` error pages

---

## Roles & Permissions

| Area | Admin | Staff |
| --- | :---: | :---: |
| Dashboard | ✅ | ✅ |
| Donors (CRUD + reports) | ✅ | ✅ |
| Donations (CRUD + reports) | ✅ | ✅ |
| User Management | ✅ | ❌ |

---

## Tech Stack

- **.NET 10** / ASP.NET Core MVC
- **Entity Framework Core 10** (SQL Server provider, database-first)
- **SQL Server** (Express or LocalDB)
- **BCrypt.Net-Next 4.2.0** for password hashing
- **Bootstrap 5** + jQuery Validation

---

## Project Structure

```
BloodDonation/
├── Controllers/          # Account, Home, Donor, Donation, User, Error
├── EF/
│   ├── BloodBankDbContext.cs
│   └── Tables/           # Donor, Donation, User, Role entities
├── Models/               # ViewModels (LoginVM, DashboardVM, PagerVM, ...)
├── Views/                # Razor views + shared layout, alerts, pagination
├── wwwroot/              # CSS, JS, Bootstrap, jQuery
└── Program.cs            # DI, auth, routing configuration
```

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express, Developer, or LocalDB)
- Visual Studio 2022+ or VS Code (optional)

### 1. Clone the repository

```bash
git clone <repository-url>
cd BloodDonation
```

### 2. Create the database

Run the script in [`db/schema.sql`](db/schema.sql) against your SQL Server instance —
it creates the `BloodBankDB` database, all four tables, and seeds the demo accounts:

```bash
sqlcmd -S .\SQLEXPRESS -E -i db/schema.sql
```

Or simply open `db/schema.sql` in SQL Server Management Studio and execute it.

### 3. Update the connection string

Edit `BloodDonation/appsettings.json` and point `DbConn` at your own SQL Server instance:

```json
{
  "ConnectionStrings": {
    "DbConn": "Server=.\\SQLEXPRESS; initial catalog=BloodBankDB; TrustServerCertificate=true; Integrated Security=true;"
  }
}
```

### 4. Run the application

```bash
cd BloodDonation
dotnet run
```

Then browse to the URL printed in the console (typically `https://localhost:7xxx`).
You will be redirected to the login page.

---

## Demo Credentials

The seed script creates two accounts:

| Role | Username | Password | Access |
| --- | --- | --- | --- |
| **Admin** | `admin` | `Admin@123` | Everything, including User Management |
| **Staff** | `staff` | `Staff@123` | Dashboard, Donors, Donations |

> ⚠️ These are demo accounts for local development only. Change the passwords (or delete
> the accounts) before deploying anywhere real.

---

## Notes

- The project uses a **database-first** EF Core workflow — the entity classes in
  `EF/Tables/` were scaffolded from an existing schema. There are no EF migrations;
  schema changes are made in SQL and re-scaffolded.
- The default admin account must be seeded directly in SQL, since creating users through
  the UI requires already being logged in as an Admin.
