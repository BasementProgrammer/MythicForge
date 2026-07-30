# Mythic Forge

A sample ecommerce web application built with **C#**, **ASP.NET MVC 5**, on **.NET Framework 4.8**.
Customers build and buy custom mystical creatures (Dragons, Unicorns, Giants, Pixies), choosing
options such as wings, horns and colors, with the price calculated from their selections.

It uses **Entity Framework 6 (Code First)** against a **local SQL Server LocalDB** database file
stored inside the project, so the whole sample is self-contained. The database is
**dropped, recreated, and reseeded on every application start**, giving a clean, predictable
experience each run.

## Features

- **Data-driven creature builder** — creature types, option categories (e.g. Wings, Horns),
  options and colors all live in the database. Add a creature or option by adding data, no code.
- **Server-side pricing** — the price is computed from the base price plus the selected color
  and option modifiers, and is never trusted from the client.
- **Shopping cart** — session-based, so shoppers can build a cart before logging in.
- **Checkout** — captures shipping details and persists an order.
- **Login / registration** — Forms Authentication with salted PBKDF2 password hashing (no
  third-party auth packages).
- **Order history** — each customer can view their past orders.

## Demo login

The database is reseeded on every run with a demo account:

- **Email:** demo@example.com
- **Password:** Password123!

You can also register a new account, but note it is wiped on the next application start.

## Requirements

These are Windows-only technologies. To build and run the app you need:

- Windows
- Visual Studio 2019 or 2022 with the **ASP.NET and web development** workload
- **.NET Framework 4.8** developer pack
- **SQL Server Express LocalDB** (installed with Visual Studio by default)

## How to run

1. Open `MythicForge.sln` in Visual Studio.
2. Restore NuGet packages (Visual Studio does this automatically on first build).
3. Press **F5** (or **Ctrl+F5**) to run under IIS Express.
4. Browse the creatures, build one, add it to your cart, log in with the demo account and check out.

## How the "clean every run" behavior works

- `Data/SampleDbContext.cs` — the Entity Framework `DbContext`.
- `Data/SampleDbInitializer.cs` — inherits `DropCreateDatabaseAlways<SampleDbContext>` and seeds
  the demo user, colors and the full creature catalog.
- `Global.asax.cs` — on `Application_Start` it registers the initializer and forces initialization,
  so the database is rebuilt and reseeded every time the app starts.
- `Web.config` — the `SampleDbContext` connection string points at `|DataDirectory|\SampleDb.mdf`,
  which resolves to the `App_Data` folder.

## Project layout

```
MythicForge.sln
MythicForge/
  App_Data/                  Local .mdf database file (created at runtime)
  App_Start/                 Route, filter and bundle configuration
  Content/                   CSS
  Controllers/               Home, Creatures, Cart, Checkout, Account, Orders
  Data/                      DbContext and drop/create/seed initializer
  Models/                    Catalog, User, Order, and the session CartLine
  Services/                  PasswordHasher, PricingService, CartService
  ViewModels/                View-specific models
  Views/                     Razor views
  Global.asax(.cs)           App startup + database initialization
  Web.config                 App config, connection string, EF + Forms auth
  packages.config            NuGet dependencies
```

## Data model

- `CreatureType` → has many `OptionCategory` → each has many `CreatureOption` (with a price modifier)
- `Color` — shared across all creatures
- `User` — customer account (PBKDF2 hashed password)
- `Order` → has many `OrderItem` — line items snapshot the chosen configuration as text

## Notes

- Client-side libraries (jQuery, Bootstrap) come from NuGet. On a fresh checkout their content
  files under `Scripts/` and `Content/` may not be present until the packages are installed; the
  bundles are configured so this does not break the app.
- `DropCreateDatabaseAlways` wipes all data (including registered accounts) on every start by
  design — that matches the "clean experience" goal.
