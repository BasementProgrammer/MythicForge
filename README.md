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

## Observability (OpenTelemetry)

The app is instrumented with **OpenTelemetry** distributed tracing (this replaced the earlier
AWS X-Ray integration). Each incoming HTTP request produces a trace, and outgoing calls — including
Amazon Bedrock image generation, which goes over HttpClient — appear as child spans. Traces are
exported over **OTLP (HTTP/protobuf)**.

Setup lives in `MythicForge/Services/OpenTelemetryConfig.cs`, is started in `Global.asax.cs`
(`Application_Start`), and the request-tracing HTTP module is registered in `Web.config` under
`<system.webServer>/<modules>`. Unlike the old X-Ray integration, tracing runs in **every**
environment (it is not gated by `DeploymentEnvironment`).

### Where traces go

The exporter sends to the first endpoint it finds:

1. The `OpenTelemetryOtlpEndpoint` app setting in `Web.config`, or
2. the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable, or
3. the OTLP default (`http://localhost:4318`) if neither is set.

The service name reported on spans comes from the `OpenTelemetryServiceName` app setting
(default `MythicForge`).

### View traces locally

The quickest way to see traces on your machine is to run a local backend that speaks OTLP and has
a UI. Jaeger all-in-one works well:

1. Start Jaeger (requires Docker), which accepts OTLP directly and serves a UI on port 16686:

   ```
   docker run --rm -p 16686:16686 -p 4318:4318 jaegertracing/all-in-one:latest
   ```

2. Point the app at it by setting the endpoint in `MythicForge/Web.config`:

   ```xml
   <add key="OpenTelemetryOtlpEndpoint" value="http://localhost:4318" />
   ```

3. Run the app (F5), click around the site to generate some requests, then open the Jaeger UI at
   <http://localhost:16686>, choose the **MythicForge** service, and search for traces.

If you would rather just confirm spans are being produced without a UI, run the OpenTelemetry
Collector locally with a `debug` exporter (same config shipped to the server, see below) and watch
its console output.

### View traces on the deployed (Elastic Beanstalk) instance

The pipeline installs an **OpenTelemetry Collector** on the EB instance as a Windows service
(`otelcol`) via `.ebextensions/02-install-otel-collector.config`. The app is pointed at it with
`OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4318` (set by the CDK stack). Here `localhost` refers
to the **EB instance itself** — the app and the collector share the host, so this is only the
internal hop from the app to the collector. It is **not** something you can browse to: the instance
has no inbound trace port open, and in a load-balanced setup you don't have a stable address for it
at all.

Instead, the collector **forwards traces off the box to AWS X-Ray**, which surfaces them in the
**CloudWatch console** under **Application monitoring → X-Ray traces** (Application Signals). This is
the AWS deployment path — the collector only runs on Elastic Beanstalk, where the app runs with
`DeploymentEnvironment=AWS`, so CloudWatch forwarding happens only in the AWS environment. The CDK
stack grants the EC2 instance role the `xray:Put*` permissions the collector needs, and the collector
picks up the region from the instance automatically.

To view your traces from the deployed app:

1. Open the **AWS Console → CloudWatch** in the same region as the environment.
2. Go to **Application monitoring → Traces** (X-Ray).
3. Filter by the **MythicForge** service (the service name from `OpenTelemetryServiceName`) and open
   a trace to see the request span with its child spans (including Amazon Bedrock calls).

**Using a different backend instead of CloudWatch.** Edit the collector config — preferably the
`files:` block in `.ebextensions/02-install-otel-collector.config` so it survives redeploys — and
change the `traces` pipeline's `exporters` list. For example, add an `otlphttp` exporter pointed at a
managed/SaaS vendor or a self-hosted Jaeger/Tempo you can reach, then restart the `otelcol` service.
You can also add `debug` to the exporters list to have spans written to the collector's own log for
troubleshooting.

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
