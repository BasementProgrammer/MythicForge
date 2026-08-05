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
AWS X-Ray integration). Each incoming HTTP request produces a trace, and outgoing HttpClient calls
appear as child spans. Traces are exported over **OTLP (HTTP/protobuf)**.

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

Instead, the collector **forwards traces off the box to AWS X-Ray**, which you view in the
**CloudWatch console** in the same region as the environment (see the steps below). This is the AWS
deployment path — the collector only runs on Elastic Beanstalk, where the app runs with
`DeploymentEnvironment=AWS`, so CloudWatch forwarding happens only in the AWS environment. The CDK
stack grants the EC2 instance role the `xray:Put*` permissions the collector needs, and the collector
picks up the region from the instance automatically.

To view your traces from the deployed app:

1. Open the **CloudWatch console** at <https://console.aws.amazon.com/cloudwatch/>.
2. **Set the region** (top-right region selector) to the same region your Elastic Beanstalk
   environment runs in — for example **Asia Pacific (Sydney) `ap-southeast-2`**. Traces are stored
   per-region: the collector sends them to the region of the EC2 instance, so if the console is in a
   different region you will see nothing.
3. In the left navigation pane, open **Application Signals (APM)** and choose **Traces** (or
   **Trace map** for the service graph). The console layout varies by region and rollout — on older
   layouts this same view appears as a top-level **X-Ray traces** entry. Both show the same X-Ray
   trace data.
4. Make sure the time range (top-right) covers when you generated traffic, then search/filter for the
   **MythicForge** service (the name from `OpenTelemetryServiceName`) and open a trace to see the
   request span and its child spans.

### Traces not showing up

If the region is correct but **Traces** / **Trace map** stay empty, the segments aren't reaching
X-Ray. Work through the pipeline hop by hop. Connect to the instance first (EC2 → the environment's
instance → **Connect → RDP**, or **Session Manager** if enabled):

1. **Generate traffic.** X-Ray only has data once requests hit the app — browse a few pages on the
   deployed site, and check the time range covers *after* that.
2. **Is the collector running?** In PowerShell on the instance: `Get-Service otelcol`. It should be
   `Running`. If it's missing or stopped, the `.ebextensions` install didn't complete — check the EB
   deploy logs.
3. **Is it accepting OTLP?** `Test-NetConnection -ComputerName localhost -Port 4318` should succeed.
4. **Watch the collector directly** — this is the fastest way to see the real error. Stop the
   service and run it in the foreground so its log prints to the console:

   ```powershell
   Stop-Service otelcol
   & 'C:\otelcol\otelcol-contrib.exe' --config 'C:\otelcol\config.yaml'
   ```

   Then generate more traffic and watch the output. An `AccessDenied` / `is not authorized to
   perform: xray:PutTraceSegments` line means the instance role is missing the X-Ray permissions
   (see below). No errors but no activity means spans aren't arriving from the app. Press `Ctrl+C`
   and `Start-Service otelcol` when done.
5. **IAM permissions.** The `awsxray` exporter needs `xray:Put*` on the instance role. These are
   granted by the CDK stack, but an environment **deployed before** those permissions were added
   won't have them — **redeploy the CDK stack** so the role update (and the collector config) apply.
   IAM changes propagate to the running instance within a few minutes; no instance replacement needed.
6. **Confirm spans leave the app.** If step 4 shows no incoming spans, add `debug` to the `traces`
   pipeline's `exporters` list in the collector config and re-run step 4 — `debug` prints every span
   the collector receives, isolating whether the problem is app→collector or collector→X-Ray.

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
