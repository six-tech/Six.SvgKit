<#
.Synopsis
This script builds and packages all of the projects set in the $targetProjects list 
and publishes them to the local NuGet feed specified in the NUGET_FEED_LOCAL 
environment variable (local directory path).

Needed environment variables:
- NUGET_FEED_LOCAL: The path to the local NuGet feed where the packages will be published.
- ANDROID_HOME: The path to the Android SDK. This is required for Android projects.

.Parameter BuildConfig
Indicates whether the build config should be set to Debug or Release. The default is Release.
#>

[CmdletBinding()]
param(
    [Parameter()]
    [ValidateSet('Debug','Release')]
    [string]$BuildConfig = "Release"
)

$ErrorActionPreference = "Stop"

$LogDirectory = "$PSScriptRoot\package-logs"
$PublishPath = [Environment]::GetEnvironmentVariable("DEV_LOCAL_NUGET_FEED_URL", "Process")
$AndroidSdkPath = [Environment]::GetEnvironmentVariable("ANDROID_HOME", "Process")

if (-not $PublishPath) {
    throw "Publish path for local NuGet feed not set in NUGET_FEED_LOCAL"
}

if (-not $AndroidSdkPath) {
    throw "Android SDK path not set in ANDROID_HOME"
}

# Add your libraries here
$targetProjects = @(
    "Six.SvgKit",
    "Six.SvgKit.Optimizer"
)

# Create the log directory.
if ((Test-Path -Path $LogDirectory) -ne $true) {
    New-Item -ItemType Directory -Path $LogDirectory | Write-Verbose
}

Write-Host "----------------------------------------------------------------------------------"
Write-Host "BUILDING AND PACKAGING PROJECTS"
Write-Host "----------------------------------------------------------------------------------"

foreach ($project in $targetProjects)
{
    # Search for the project file recursively
    $projectPath = Get-ChildItem -Path "$PSScriptRoot\libs" -Recurse -Filter "$project.csproj" | Select-Object -First 1 -ExpandProperty FullName

    if (-not $projectPath) {
        Write-Warning "Project file for $project not found. Skipping..."
        continue
    }

    $projectNameForLog = $project

    $androidSpecificArgs = "-p:AndroidSdkDirectory=`"$AndroidSdkPath`"" # Need to set Android SDK path for Android projects.    

    & dotnet build -c $BuildConfig "$projectPath" $androidSpecificArgs | Tee-Object -FilePath "$LogDirectory\${projectNameForLog}_build.log"

    Write-Host ""
    Write-Host "----------------------------------------------------------------------------------"
    Write-Host "Packing $project" -ForegroundColor Cyan    
    
    & dotnet pack -c $BuildConfig -o "$PublishPath" "$projectPath" --no-build --include-symbols $androidSpecificArgs | Tee-Object -FilePath "$LogDirectory\${projectNameForLog}_packaging.log"

    Write-Host ""    
    Write-Host "Packing DONE $project" -ForegroundColor Green
    Write-Host "----------------------------------------------------------------------------------"
    Write-Host ""
    Write-Host ""
}

Write-Host ""
Write-Host "----------------------------------------------------------------------------------"
Write-Host "All projects have been successfully packed and are located at $PublishPath" -ForegroundColor Green

# Wait for the user to press a key before exiting
Write-Host "Press any key to exit..."
$null = Read-Host
exit $LASTEXITCODE