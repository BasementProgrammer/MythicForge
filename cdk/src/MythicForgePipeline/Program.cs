using Amazon.CDK;

namespace MythicForgePipeline
{
    internal sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();

            new PipelineStack(app, "MythicForgePipelineStack", new StackProps
            {
                // Deploy into the account/region resolved from your current AWS credentials
                // (or CDK_DEFAULT_ACCOUNT / CDK_DEFAULT_REGION environment variables).
                Env = new Amazon.CDK.Environment
                {
                    Account = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_ACCOUNT"),
                    Region = System.Environment.GetEnvironmentVariable("CDK_DEFAULT_REGION")
                },
                Description = "CI/CD pipeline for the MythicForge ASP.NET Framework app (CodeCommit -> CodeBuild -> Elastic Beanstalk)."
            });

            app.Synth();
        }
    }
}
