<#
.Synopsis
This script builds and packages specified projects and optionally publishes them to both a local and a remote NuGet feed.

.Description
Builds each project in the $targetProjects list, packages them, and publishes to a local directory specified by the NUGET_FEED_LOCAL environment variable. If the UploadToRemote parameter is set, it will also push the latest version of the package and its symbols to the remote NuGet repository.

.Parameter BuildConfig
Indicates whether the build config should be set to Debug or Release. The default is Release.

.Parameter UploadToRemote
Indicates whether to upload the packages to the remote NuGet repository. The default is False.

.Notes
Needed environment variables:
- NUGET_FEED_LOCAL: The path to the local NuGet feed where the packages will be published.
- NUGET_API_KEY: API key for the NuGet repository
- ANDROID_HOME: The path to the Android SDK. This is required for Android projects.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$BuildConfig = "Release",

    [Parameter()]
    [bool]$UploadToRemote = $false
)

$ErrorActionPreference = "Stop"
$LogDirectory = "$PSScriptRoot\package-logs"

# Directory path of local NuGet feed (publishes to a local directory)
$LocalRepositoryPath = [Environment]::GetEnvironmentVariable("DEV_LOCAL_NUGET_FEED_URL", "Process")

# Url of the remote NuGet repository
$NugetRepositoryUrl = [Environment]::GetEnvironmentVariable("PERSONAL_NUGET_FEED_URL", "Process")

# Remote NuGet repository API key
$NugetRepositoryApiKey = [Environment]::GetEnvironmentVariable("PERSONAL_NUGET_FEED_PAT", "Process")

# Path to the Android SDK (needed for building Avalonia projects)
$AndroidSdkPath = [Environment]::GetEnvironmentVariable("ANDROID_HOME", "Process")


if (-not $LocalRepositoryPath)
{
    throw "Publish path for local NuGet feed not set in NUGET_FEED_LOCAL"
}

if (-not $AndroidSdkPath)
{
    throw "Android SDK path not set in ANDROID_HOME"
}

if ($UploadToRemote -and [string]::IsNullOrEmpty($NugetRepositoryApiKey))
{
    throw "API key for NuGet is not set in the environment variables. Please set the NUGET_API_KEY environment variable."
}

# Add your libraries here
$targetProjects = @(
    "Six.Library.Path.A",
    "Six.Library.Path.B"
)

# Create the log directory.
if (-not (Test-Path -Path $LogDirectory))
{
    New-Item -ItemType Directory -Path $LogDirectory | Out-Null
}

Write-Host "----------------------------------------------------------------------------------" -ForegroundColor Yellow
Write-Host "BUILDING AND PACKAGING PROJECTS" -ForegroundColor Green
Write-Host "----------------------------------------------------------------------------------" -ForegroundColor Yellow

foreach ($project in $targetProjects)
{
    # Search for the project file recursively
    $projectPath = Get-ChildItem -Path "$PSScriptRoot\libs" -Recurse -Filter "$project.csproj" | Select-Object -First 1 -ExpandProperty FullName

    if (-not $projectPath)
    {
        Write-Warning "Project file for $project not found. Skipping..."
        continue
    }

    $androidSpecificArgs = if ($project -like "*Android*")
    {
        "-p:AndroidSdkDirectory=`"$AndroidSdkPath`""
    }
    else
    {
        ""
    }

    & dotnet build -c $BuildConfig "$projectPath" $androidSpecificArgs | Out-Null

    Write-Host ""
    Write-Host "----------------------------------------------------------------------------------" -ForegroundColor Yellow
    Write-Host "Packing: $project" -ForegroundColor Cyan
    Write-Host ""
    & dotnet pack -c $BuildConfig -o "$LocalRepositoryPath" "$projectPath" --no-build --include-symbols $androidSpecificArgs | Out-Null

    Write-Host ""
    Write-Host "Packing DONE: $project" -ForegroundColor Green
    Write-Host "----------------------------------------------------------------------------------" -ForegroundColor Yellow
    Write-Host ""
    Write-Host ""

    if ($UploadToRemote)
    {
        $latestNugetPackage = Get-ChildItem -Path $LocalRepositoryPath -Filter "$project*.nupkg" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        $latestSnupkgFile = Get-ChildItem -Path $LocalRepositoryPath -Filter "$project*.snupkg" | Where-Object { $_.BaseName -eq $latestNugetPackage.BaseName } | Sort-Object LastWriteTime -Descending | Select-Object -First 1

        if ($latestNugetPackage -and $latestSnupkgFile)
        {

            Write-Host ""
            Write-Host "----------------------------------------------------------------------------------" -ForegroundColor Yellow
            Write-Host "Publishing to remote NuGet repository: $project" -ForegroundColor Magenta
            Write-Host ""

            dotnet nuget push $latestNugetPackage.FullName --skip-duplicate --api-key $NugetRepositoryApiKey --source $NugetRepositoryUrl
            dotnet nuget push $latestSnupkgFile.FullName --skip-duplicate --api-key $NugetRepositoryApiKey --source $NugetRepositoryUrl
        }
    }
}

Write-Host "----------------------------------------------------------------------------------" -ForegroundColor Yellow
Write-Host "All projects have been successfully packed and are located at $LocalRepositoryPath" -ForegroundColor Green
if ($UploadToRemote)
{
    Write-Host "Latest versions of projects have been uploaded to the remote NuGet repository." -ForegroundColor Green
}

# Wait for the user to press a key before exiting
Write-Host "Press any key to exit..." -ForegroundColor Magenta
$null = Read-Host