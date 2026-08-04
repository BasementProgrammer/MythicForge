using System;
using System.Configuration;

namespace MythicForge.Services
{
    /// <summary>
    /// Reads the "DeploymentEnvironment" Web.config app setting and reports whether the
    /// application is running against AWS. Recognized values are "AWS" and "Local".
    /// When the value is anything other than "AWS" (case-insensitive) all AWS-native
    /// features (currently Amazon Bedrock image generation) are disabled so the
    /// application can run locally without an AWS account or credentials.
    /// (OpenTelemetry tracing is vendor-neutral and runs in every environment.)
    /// </summary>
    public static class DeploymentEnvironment
    {
        /// <summary>The environment name that enables AWS-native services.</summary>
        public const string Aws = "AWS";

        /// <summary>The environment name for running without any AWS dependency.</summary>
        public const string Local = "Local";

        /// <summary>
        /// The configured environment name from the "DeploymentEnvironment" app setting.
        /// Defaults to "AWS" when the setting is missing or blank, preserving the deployed
        /// (Elastic Beanstalk) behavior.
        /// </summary>
        public static string Current
        {
            get
            {
                var raw = ConfigurationManager.AppSettings["DeploymentEnvironment"];
                return string.IsNullOrWhiteSpace(raw) ? Aws : raw.Trim();
            }
        }

        /// <summary>
        /// True when the app is configured to run against AWS (the setting equals "AWS",
        /// case-insensitive). Any other value (e.g. "Local") returns false and turns off
        /// all AWS-specific features.
        /// </summary>
        public static bool IsAws
        {
            get { return Current.Equals(Aws, StringComparison.OrdinalIgnoreCase); }
        }
    }
}
