using System;
using System.Configuration;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace MythicForge.Services
{
    /// <summary>
    /// Configures OpenTelemetry distributed tracing for the application. This replaces the
    /// previous AWS X-Ray integration with a vendor-neutral pipeline:
    ///
    ///   * Incoming ASP.NET requests are traced by the TelemetryHttpModule registered in
    ///     Web.config (&lt;system.webServer&gt;/&lt;modules&gt;) together with
    ///     <c>AddAspNetInstrumentation()</c> here.
    ///   * Outgoing HTTP calls made via HttpClient are traced by
    ///     <c>AddHttpClientInstrumentation()</c>.
    ///   * Spans are exported over OTLP to a collector (e.g. the OpenTelemetry Collector,
    ///     or any OTLP-compatible backend).
    ///
    /// Unlike X-Ray this is not AWS-specific, so it is initialized in every environment
    /// (AWS and Local). When no OTLP endpoint is reachable the exporter simply fails to
    /// send and logs internally; it does not affect request handling.
    /// </summary>
    public static class OpenTelemetryConfig
    {
        private static TracerProvider _tracerProvider;

        /// <summary>
        /// Logical service name reported on every span. Reads the "OpenTelemetryServiceName"
        /// app setting, defaulting to "MythicForge".
        /// </summary>
        public static string ServiceName
        {
            get
            {
                var raw = ConfigurationManager.AppSettings["OpenTelemetryServiceName"];
                return string.IsNullOrWhiteSpace(raw) ? "MythicForge" : raw.Trim();
            }
        }

        /// <summary>
        /// Optional OTLP collector endpoint (e.g. "http://localhost:4318" for HTTP/protobuf).
        /// Read from the
        /// "OpenTelemetryOtlpEndpoint" app setting. When unset, the OTLP exporter's own
        /// default endpoint / the OTEL_EXPORTER_OTLP_ENDPOINT environment variable is used.
        /// </summary>
        private static string OtlpEndpoint
        {
            get
            {
                var raw = ConfigurationManager.AppSettings["OpenTelemetryOtlpEndpoint"];
                return string.IsNullOrWhiteSpace(raw) ? null : raw.Trim();
            }
        }

        /// <summary>
        /// Builds and starts the global tracer provider. Idempotent — safe to call once
        /// from Application_Start.
        /// </summary>
        public static void Initialize()
        {
            if (_tracerProvider != null)
            {
                return;
            }

            var builder = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(ServiceName))
                .AddAspNetInstrumentation()
                .AddHttpClientInstrumentation();

            var endpoint = OtlpEndpoint;
            builder.AddOtlpExporter(options =>
            {
                // On .NET Framework 4.8 use OTLP over HTTP/protobuf (port 4318). The gRPC
                // transport relies on HTTP/2, which the framework's HttpClient does not
                // support reliably.
                options.Protocol = OtlpExportProtocol.HttpProtobuf;

                if (!string.IsNullOrEmpty(endpoint))
                {
                    options.Endpoint = new Uri(endpoint);
                }
            });

            _tracerProvider = builder.Build();
        }

        /// <summary>
        /// Flushes and disposes the tracer provider. Call from Application_End so buffered
        /// spans are exported on shutdown.
        /// </summary>
        public static void Shutdown()
        {
            _tracerProvider?.Dispose();
            _tracerProvider = null;
        }
    }
}
