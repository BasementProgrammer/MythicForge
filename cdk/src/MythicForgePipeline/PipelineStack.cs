using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.CodeBuild;
using Amazon.CDK.AWS.CodePipeline;
using Amazon.CDK.AWS.CodePipeline.Actions;
using Amazon.CDK.AWS.CodeStarConnections;
using Amazon.CDK.AWS.ElasticBeanstalk;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.CustomResources;
using Constructs;
using StageProps = Amazon.CDK.AWS.CodePipeline.StageProps;
using Code = Amazon.CDK.AWS.Lambda.Code;

namespace MythicForgePipeline
{
    public class PipelineStack : Stack
    {
        // ------------------------------------------------------------------
        // Tweak these to match your environment.
        // ------------------------------------------------------------------

        // GitHub repository owner and name.
        private const string GitHubOwner = "BasementProgrammer";
        private const string GitHubRepo = "MythicForge";

        // Branch the pipeline watches.
        private const string SourceBranch = "main";

        // Repository layout. The source repo root contains everything (this cdk/
        // project, the solution file and the README); the deployable ASP.NET app
        // lives in the "MythicForge" subfolder. Paths below are relative to the
        // repo root (which is CODEBUILD_SRC_DIR when CodeBuild checks the repo out).
        private const string AppFolder = "MythicForge";
        private const string SolutionFile = "MythicForge.sln";
        private const string ProjectFile = AppFolder + "/MythicForge.csproj";

        // Web Deploy package produced by msdeploy and referenced by the EB manifest.
        private const string PackageFile = "MythicForge.zip";

        // Elastic Beanstalk application / environment names.
        private const string EbApplicationName = "MythicForge";
        private const string EbEnvironmentName = "MythicForge-env";

        // Preferred Windows Server version (full edition, not Server Core) for the
        // "running IIS 10.0" platform that hosts the .NET Framework 4.8 app.
        private const string PreferredWindowsVersion = "2022";

        internal PipelineStack(Construct scope, string id, IStackProps props = null)
            : base(scope, id, props)
        {
            // The EB Windows/IIS solution stack version string is region-specific and
            // AWS bumps the patch version regularly, so pinning it in code breaks
            // deployments over time. By default we resolve the newest matching stack at
            // deploy time with a small custom resource. You can still pin it explicitly:
            //   cdk deploy -c solutionStack="64bit Windows Server 2022 v2.x.y running IIS 10.0"
            var solutionStackOverride = (string)Node.TryGetContext("solutionStack");
            var solutionStack = string.IsNullOrEmpty(solutionStackOverride)
                ? ResolveSolutionStackAtDeploy()
                : solutionStackOverride;

            // --------------------------------------------------------------
            // Source: GitHub via AWS CodeStar Connections.
            // After first deploy, confirm the connection in the AWS Console
            // under Developer Tools > Settings > Connections.
            // --------------------------------------------------------------
            var connection = new CfnConnection(this, "GitHubConnection", new CfnConnectionProps
            {
                ConnectionName = "MythicForge-GitHub",
                ProviderType = "GitHub"
            });

            // --------------------------------------------------------------
            // Elastic Beanstalk application + Windows environment.
            // --------------------------------------------------------------
            var ebApp = new CfnApplication(this, "EbApplication", new CfnApplicationProps
            {
                ApplicationName = EbApplicationName
            });

            // EC2 instance profile used by the EB hosts.
            var instanceRole = new Role(this, "EbInstanceRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("ec2.amazonaws.com"),
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("AWSElasticBeanstalkWebTier"),
                    ManagedPolicy.FromAwsManagedPolicyName("AWSElasticBeanstalkWorkerTier"),
                    ManagedPolicy.FromAwsManagedPolicyName("AWSElasticBeanstalkMulticontainerDocker")
                }
            });

            // Let the on-instance OpenTelemetry Collector forward traces to AWS X-Ray
            // (surfaced in the CloudWatch console). These are the same permissions the old
            // X-Ray daemon used; X-Ray does not support resource-level scoping for them.
            instanceRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[]
                {
                    "xray:PutTraceSegments",
                    "xray:PutTelemetryRecords",
                    "xray:GetSamplingRules",
                    "xray:GetSamplingTargets",
                    "xray:GetSamplingStatisticSummaries"
                },
                Resources = new[] { "*" }
            }));

            var instanceProfile = new CfnInstanceProfile(this, "EbInstanceProfile", new CfnInstanceProfileProps
            {
                Roles = new[] { instanceRole.RoleName }
            });

            // Service role that lets Elastic Beanstalk manage the environment.
            var serviceRole = new Role(this, "EbServiceRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("elasticbeanstalk.amazonaws.com"),
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSElasticBeanstalkEnhancedHealth"),
                    // NOTE: this policy lives at the IAM root path, not under service-role/.
                    ManagedPolicy.FromAwsManagedPolicyName("AWSElasticBeanstalkManagedUpdatesCustomerRolePolicy")
                }
            });

            var ebEnv = new CfnEnvironment(this, "EbEnvironment", new CfnEnvironmentProps
            {
                ApplicationName = EbApplicationName,
                EnvironmentName = EbEnvironmentName,
                SolutionStackName = solutionStack,
                OptionSettings = new[]
                {
                    new CfnEnvironment.OptionSettingProperty
                    {
                        Namespace = "aws:autoscaling:launchconfiguration",
                        OptionName = "IamInstanceProfile",
                        Value = instanceProfile.Ref
                    },
                    new CfnEnvironment.OptionSettingProperty
                    {
                        Namespace = "aws:autoscaling:launchconfiguration",
                        OptionName = "InstanceType",
                        // Windows instances need a bit of headroom; t3.medium is a sane default.
                        Value = "t3.medium"
                    },
                    new CfnEnvironment.OptionSettingProperty
                    {
                        Namespace = "aws:elasticbeanstalk:environment",
                        OptionName = "ServiceRole",
                        Value = serviceRole.RoleName
                    },
                    new CfnEnvironment.OptionSettingProperty
                    {
                        Namespace = "aws:elasticbeanstalk:environment",
                        // SingleInstance keeps the sample cheap (no load balancer). Switch to
                        // "LoadBalanced" for production.
                        OptionName = "EnvironmentType",
                        Value = "SingleInstance"
                    },
                    // OpenTelemetry replaced AWS X-Ray. The app exports OTLP traces to the
                    // OpenTelemetry Collector installed on the instance by
                    // .ebextensions/02-install-otel-collector.config, which listens on
                    // localhost:4318 (OTLP/HTTP). The OTLP exporter appends the /v1/traces
                    // path to this base endpoint.
                    new CfnEnvironment.OptionSettingProperty
                    {
                        Namespace = "aws:elasticbeanstalk:application:environment",
                        OptionName = "OTEL_EXPORTER_OTLP_ENDPOINT",
                        Value = "http://localhost:4318"
                    }
                }
            });

            // Make sure the application exists before the environment is created.
            ebEnv.AddResourceDependency(ebApp);

            // --------------------------------------------------------------
            // Build: restore + msbuild on a Windows CodeBuild image, producing
            // a web-deploy package plus the EB deployment manifest.
            // --------------------------------------------------------------
            var buildProject = new PipelineProject(this, "BuildProject", new PipelineProjectProps
            {
                ProjectName = "MythicForge-build",
                Environment = new BuildEnvironment
                {
                    // Windows base image ships MSBuild, NuGet and the .NET Framework
                    // developer tools required to compile the ASP.NET MVC app.
                    // Note: the 2019 container is not offered in every region (e.g. ap-southeast-2),
                    // so we use the more broadly available Windows Server 2022 image.
                    BuildImage = WindowsBuildImage.WIN_SERVER_CORE_2022_BASE_3_0,
                    // Windows containers require MEDIUM or larger (SMALL is unsupported).
                    ComputeType = ComputeType.MEDIUM
                },
                BuildSpec = BuildSpec.FromObject(new Dictionary<string, object>
                {
                    ["version"] = "0.2",
                    ["phases"] = new Dictionary<string, object>
                    {
                        ["install"] = new Dictionary<string, object>
                        {
                            // The Windows Server 2022 CodeBuild image has VS Build Tools but not
                            // the .NET Framework 4.8 targeting pack, so MSBuild can't find the v4.8
                            // reference assemblies. Install it with Chocolatey (preinstalled on the
                            // image). Choco treats reboot-required exit code 3010 as success.
            ["commands"] = new[]
                            {
                                // .NET Framework 4.8 targeting pack (for MSBuild to find v4.8
                                // reference assemblies) and Web Deploy (for msdeploy.exe, used to
                                // build the deployment package). Both preinstalled tools are absent
                                // from the image. Choco treats reboot code 3010 as success.
                                "choco install netfx-4.8-devpack -y --no-progress",
                                "choco install webdeploy -y --no-progress"
                            }
                        },
                        ["build"] = new Dictionary<string, object>
                        {
                            // All paths are relative to the repo root (CODEBUILD_SRC_DIR).
                            // The solution sits at the root and references the app project in
                            // the "MythicForge" subfolder.
                            //
                            // The CodeBuild Windows image ships VS Build Tools WITHOUT the web
                            // workload, so Microsoft.WebApplication.targets (the Package/Publish
                            // targets) is missing. We restore those targets from the
                            // MSBuild.Microsoft.VisualStudio.Web.targets NuGet package and point
                            // VSToolsPath at it, so MSBuild's Package target runs exactly like
                            // Visual Studio and emits a proper Web Deploy package (with the
                            // 'IIS Web Application Name' parameter the EB manifest sets).
                            //
                            // MSBuild is also not on PATH, so we locate it with vswhere.
                            ["commands"] = new[]
                            {
                                $"nuget restore {SolutionFile}",
                                "nuget install MSBuild.Microsoft.VisualStudio.Web.targets " +
                                "-Version 14.0.0.3 -OutputDirectory build-tools -ExcludeVersion",
                                "$vswhere = \"${env:ProgramFiles(x86)}\\Microsoft Visual Studio\\Installer\\vswhere.exe\"; " +
                                "$msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild " +
                                "-find MSBuild\\**\\Bin\\MSBuild.exe | Select-Object -First 1; " +
                                "if (-not $msbuild) { throw 'MSBuild not found via vswhere' }; " +
                                "$vstools = \"$env:CODEBUILD_SRC_DIR\\build-tools\\MSBuild.Microsoft.VisualStudio.Web.targets\\tools\\VSToolsPath\"; " +
                                "if (-not (Test-Path \"$vstools\\WebApplications\\Microsoft.WebApplication.targets\")) " +
                                "{ throw 'Web targets NuGet package not restored' }; " +
                                "Write-Host \"Using MSBuild at $msbuild; VSToolsPath=$vstools\"; " +
                                $"& $msbuild {ProjectFile} /p:Configuration=Release /p:VisualStudioVersion=16.0 " +
                                "/p:VSToolsPath=\"$vstools\" /p:DeployOnBuild=true /p:WebPublishMethod=Package " +
                                $"/p:PackageAsSingleFile=true /p:PackageLocation=\"$env:CODEBUILD_SRC_DIR\\{PackageFile}\"; " +
                                "if ($LASTEXITCODE -ne 0) { throw 'MSBuild package failed' }",
                                // Write the EB Windows deployment manifest that deploys the package
                                // to the root of the default IIS site.
                                "Set-Content -Path $env:CODEBUILD_SRC_DIR\\aws-windows-deployment-manifest.json " +
                                "-Value '{ \"manifestVersion\": 1, \"deployments\": { \"msDeploy\": [ " +
                                $"{{ \"name\": \"MythicForge\", \"parameters\": {{ \"appBundle\": \"{PackageFile}\", " +
                                "\"iisPath\": \"/\", \"iisWebSite\": \"Default Web Site\" } } ] } }'; " +
                                $"if (-not (Test-Path \"$env:CODEBUILD_SRC_DIR\\{PackageFile}\")) {{ throw 'Web deploy package was not produced' }}"
                            }
                        }
                    },
                    // The msdeploy package, manifest and .ebextensions (config that installs
                    // LocalDB on the instance) are zipped by CodePipeline into the bundle handed
                    // to the Elastic Beanstalk deploy action. .ebextensions must sit at the bundle
                    // root alongside the manifest, so it is copied straight from the source root.
                    ["artifacts"] = new Dictionary<string, object>
                    {
                        ["files"] = new[]
                        {
                            PackageFile,
                            "aws-windows-deployment-manifest.json",
                            ".ebextensions/**/*"
                        }
                    }
                })
            });

            // --------------------------------------------------------------
            // Pipeline: Source -> Build -> Deploy.
            // --------------------------------------------------------------
            var sourceOutput = new Artifact_("SourceOutput");
            var buildOutput = new Artifact_("BuildOutput");

            new Pipeline(this, "Pipeline", new PipelineProps
            {
                PipelineName = "MythicForge-pipeline",
                PipelineType = PipelineType.V2,
                Stages = new[]
                {
                    new StageProps
                    {
                        StageName = "Source",
                        Actions = new[]
                        {
                            new CodeStarConnectionsSourceAction(new CodeStarConnectionsSourceActionProps
                            {
                                ActionName = "GitHub_Source",
                                Owner = GitHubOwner,
                                Repo = GitHubRepo,
                                Branch = SourceBranch,
                                ConnectionArn = connection.AttrConnectionArn,
                                Output = sourceOutput
                            })
                        }
                    },
                    new StageProps
                    {
                        StageName = "Build",
                        Actions = new[]
                        {
                            new CodeBuildAction(new CodeBuildActionProps
                            {
                                ActionName = "MSBuild",
                                Project = buildProject,
                                Input = sourceOutput,
                                Outputs = new[] { buildOutput }
                            })
                        }
                    },
                    new StageProps
                    {
                        StageName = "Deploy",
                        Actions = new[]
                        {
                            new ElasticBeanstalkDeployAction(new ElasticBeanstalkDeployActionProps
                            {
                                ActionName = "Deploy_To_ElasticBeanstalk",
                                ApplicationName = EbApplicationName,
                                EnvironmentName = EbEnvironmentName,
                                Input = buildOutput
                            })
                        }
                    }
                }
            });

            // --------------------------------------------------------------
            // Handy outputs.
            // --------------------------------------------------------------
            new CfnOutput(this, "EbEnvironmentUrl", new CfnOutputProps
            {
                Value = ebEnv.AttrEndpointUrl,
                Description = "Public endpoint of the Elastic Beanstalk environment."
            });

            new CfnOutput(this, "GitHubRepositoryUrl", new CfnOutputProps
            {
                Value = $"https://github.com/{GitHubOwner}/{GitHubRepo}",
                Description = "Source GitHub repository URL."
            });

            new CfnOutput(this, "CodeStarConnectionArn", new CfnOutputProps
            {
                Value = connection.AttrConnectionArn,
                Description = "CodeStar Connection ARN (must be confirmed in the console after first deploy)."
            });
        }

        /// <summary>
        /// Creates a custom resource that, at deploy time, asks Elastic Beanstalk for the
        /// newest available Windows / IIS 10.0 solution stack (preferring the full
        /// Windows Server <see cref="PreferredWindowsVersion"/> edition, not Server Core)
        /// and returns its exact name. Running in-account at deploy time means it always
        /// picks a currently-valid stack without depending on local credentials or a
        /// hard-coded version string.
        /// </summary>
        private string ResolveSolutionStackAtDeploy()
        {
            // Inline handler run by the custom-resource provider. boto3 ships in the
            // Lambda Python runtime, so no bundling/assets are required.
            var handlerCode = string.Join("\n", new[]
            {
                "import re",
                "import boto3",
                "",
                "def handler(event, context):",
                "    if event['RequestType'] == 'Delete':",
                "        return {'PhysicalResourceId': event.get('PhysicalResourceId', 'eb-solution-stack')}",
                "    preferred = event['ResourceProperties']['PreferredWindowsVersion']",
                "    eb = boto3.client('elasticbeanstalk')",
                "    stacks = eb.list_available_solution_stacks()['SolutionStacks']",
                "    def ver(s):",
                "        m = re.search(r'v(\\d+(?:\\.\\d+)+)', s)",
                "        return tuple(int(x) for x in m.group(1).split('.')) if m else (0,)",
                "    candidates = [s for s in stacks if 'running IIS 10.0' in s and 'Windows Server Core' not in s]",
                "    pref = [s for s in candidates if ('Windows Server ' + preferred + ' ') in s]",
                "    ordered = sorted(pref or candidates, key=ver, reverse=True)",
                "    if not ordered:",
                "        raise Exception('No Windows/IIS 10.0 Elastic Beanstalk solution stack found in this region')",
                "    return {'PhysicalResourceId': 'eb-solution-stack', 'Data': {'SolutionStackName': ordered[0]}}"
            });

            var lookupFn = new Function(this, "SolutionStackLookupFn", new FunctionProps
            {
                Runtime = Runtime.PYTHON_3_12,
                Handler = "index.handler",
                Code = Code.FromInline(handlerCode),
                Timeout = Duration.Seconds(30)
            });

            lookupFn.AddToRolePolicy(new PolicyStatement(new PolicyStatementProps
            {
                Actions = new[] { "elasticbeanstalk:ListAvailableSolutionStacks" },
                Resources = new[] { "*" }
            }));

            var provider = new Provider(this, "SolutionStackProvider", new ProviderProps
            {
                OnEventHandler = lookupFn
            });

            var lookup = new CustomResource(this, "SolutionStackLookup", new CustomResourceProps
            {
                ServiceToken = provider.ServiceToken,
                Properties = new Dictionary<string, object>
                {
                    ["PreferredWindowsVersion"] = PreferredWindowsVersion
                }
            });

            return lookup.GetAttString("SolutionStackName");
        }
    }
}
