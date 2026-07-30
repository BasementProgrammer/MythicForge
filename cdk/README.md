# MythicForge CI/CD Pipeline (AWS CDK, C#)

This CDK app provisions a complete CI/CD pipeline for the **MythicForge** ASP.NET MVC 5
/ .NET Framework 4.8 application:

```
CodeCommit (MythicForge)  ->  CodeBuild (Windows, MSBuild)  ->  Elastic Beanstalk (Windows / IIS 10)
```

Because the app targets **.NET Framework 4.8**, both the build and the runtime run on
**Windows**: a Windows CodeBuild image compiles the app with MSBuild, and Elastic Beanstalk
hosts it on a Windows Server / IIS solution stack.

## What gets created

- **CodePipeline** (`MythicForge-pipeline`) with three stages:
  - **Source** – pulls from the existing CodeCommit repo `MythicForge` (branch `main`).
  - **Build** – a Windows CodeBuild project that runs `nuget restore` + `msbuild`, produces a
    web-deploy package (`MythicForge.zip`) and an `aws-windows-deployment-manifest.json`.
  - **Deploy** – deploys the bundle to Elastic Beanstalk.
- **Elastic Beanstalk** application (`MythicForge`) and a single-instance Windows/IIS
  environment (`MythicForge-env`).
- Supporting **IAM roles** (EB EC2 instance profile + EB service role) and the pipeline's
  artifact bucket / roles (created automatically by the CDK constructs).

## Prerequisites

- .NET SDK 8.0+ (this project targets `net10.0`; adjust `TargetFramework` if you use a
  different SDK).
- Node.js and the AWS CDK Toolkit (`npm install -g aws-cdk`).
- AWS credentials configured for the target account/region.
- The **MythicForge** CodeCommit repository must already exist in that account and contain the
  application source (with `MythicForge.sln` at the repo root).
- The account/region must be **bootstrapped** for CDK:
  ```bash
  cdk bootstrap
  ```

## Elastic Beanstalk solution stack (auto-resolved)

Windows solution-stack version strings are region-specific and AWS bumps the patch version
(e.g. `v2.23.2` → `v2.23.3`) regularly, so pinning one in code breaks deployments over time.

By default this stack **resolves the solution stack at deploy time**: a small custom resource
(an inline Python Lambda) calls `elasticbeanstalk:ListAvailableSolutionStacks` in your account
and picks the newest full **Windows Server 2022 / IIS 10.0** stack (Server Core is excluded).
Because it runs in-account at deploy time, it always finds a currently-valid stack and needs no
local credentials or manual version chasing.

To change the preferred Windows edition, edit `PreferredWindowsVersion` in `PipelineStack.cs`
(e.g. `"2019"` or `"2025"`).

If you'd rather pin an exact stack, override it and the custom resource is skipped entirely:

```bash
# List what's available in your region:
aws elasticbeanstalk list-available-solution-stacks \
  --query "SolutionStacks[?contains(@,'running IIS')]"

# Pin it:
cdk deploy -c solutionStack="64bit Windows Server 2022 v2.x.y running IIS 10.0"
```

## Deploy

From this `cdk/` directory:

```bash
dotnet build src/MythicForgePipeline/MythicForgePipeline.csproj
cdk synth
cdk deploy
```

After the stack is created, the pipeline runs automatically on the next commit to `main`. To
trigger it immediately you can release a change from the CodePipeline console, or push a commit
to the repository.

The stack outputs:
- `EbEnvironmentUrl` – the public URL of the Elastic Beanstalk environment.
- `RepositoryCloneUrlHttp` – the HTTPS clone URL of the source repository.

## Configuration knobs

Constants at the top of `src/MythicForgePipeline/PipelineStack.cs`:

| Constant | Default | Purpose |
| --- | --- | --- |
| `RepositoryName` | `MythicForge` | Existing CodeCommit repo to build from. |
| `SourceBranch` | `main` | Branch the pipeline watches. |
| `EbApplicationName` | `MythicForge` | Elastic Beanstalk application name. |
| `EbEnvironmentName` | `MythicForge-env` | Elastic Beanstalk environment name. |
| `DefaultSolutionStack` | Windows Server 2019 / IIS 10 | Overridable via `-c solutionStack=...`. |

The build steps live in the `BuildSpec` in the same file. The instance type
(`t3.medium`) and environment type (`SingleInstance`) are set in the EB `OptionSettings` — switch
`EnvironmentType` to `LoadBalanced` for a production-grade, auto-scaled setup.

## How the build produces a deployable bundle

The Windows CodeBuild image (`aws/codebuild/windows-base:2019-1.0`) ships MSBuild, NuGet, and the
.NET Framework tooling. The build:

1. `nuget restore MythicForge.sln`
2. `msbuild ... /p:DeployOnBuild=true /p:WebPublishMethod=Package` → `MythicForge.zip`
   (a Web Deploy / msdeploy package).
3. Writes `aws-windows-deployment-manifest.json`, which tells the EB Windows platform to deploy
   the package to the IIS `Default Web Site` at path `/`.

CodePipeline zips those two files into the artifact that the Elastic Beanstalk deploy action
publishes as a new application version.

## Clean up

```bash
cdk destroy
```

Note: the CodePipeline artifact S3 bucket may need to be emptied manually before it can be
removed.
