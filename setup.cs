#!/usr/bin/env dotnet
#:package CliWrap@3.6.6
#:package Spectre.Console@0.49.1

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CliWrap;
using CliWrap.Buffered;
using Spectre.Console;

// Display welcome message
AnsiConsole.Write(new FigletText("Six Solution Setup").Color(Color.Cyan1));
AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[grey]Repository Setup and Configuration Tool[/]");
AnsiConsole.WriteLine();

// Check prerequisites
var setupContext = new SetupContext();

await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .StartAsync("Checking prerequisites...", async ctx =>
    {
        // Check dotnet CLI
        ctx.Status("Checking dotnet CLI...");
        setupContext.HasDotnet = await CheckCommand("dotnet", "--version");
        await Task.Delay(100);
        
        // Check pnpm
        ctx.Status("Checking pnpm...");
        setupContext.HasPnpm = await CheckCommand("pnpm", "--version");
        await Task.Delay(100);
        
        // Check npm if pnpm not found
        if (!setupContext.HasPnpm)
        {
            ctx.Status("Checking npm...");
            setupContext.HasNpm = await CheckCommand("npm", "--version");
            await Task.Delay(100);
        }
        
        // Get project name from solution file
        ctx.Status("Detecting project name...");
        setupContext.ProjectName = GetProjectName();
        await Task.Delay(100);
    });

// Display prerequisite results
AnsiConsole.Write(new Rule("[yellow]Prerequisites Check[/]").LeftJustified());

var prereqTable = new Table()
    .Border(TableBorder.Rounded)
    .AddColumn("Component")
    .AddColumn("Status")
    .AddColumn("Details");

prereqTable.AddRow(
    "dotnet CLI",
    setupContext.HasDotnet ? "[green]✓ Installed[/]" : "[red]✗ Not Found[/]",
    setupContext.HasDotnet ? "Ready" : "Required for project setup"
);

prereqTable.AddRow(
    "pnpm",
    setupContext.HasPnpm ? "[green]✓ Installed[/]" : "[yellow]⚠ Not Found[/]",
    setupContext.HasPnpm ? "Preferred package manager" : "Will try npm as fallback"
);

prereqTable.AddRow(
    "npm",
    setupContext.HasNpm ? "[green]✓ Installed[/]" : (setupContext.HasPnpm ? "[grey]N/A[/]" : "[red]✗ Not Found[/]"),
    setupContext.HasNpm ? "Fallback package manager" : (setupContext.HasPnpm ? "Not needed (pnpm found)" : "Required if pnpm not available")
);

prereqTable.AddRow(
    "Project Name",
    !string.IsNullOrEmpty(setupContext.ProjectName) ? "[green]✓ Detected[/]" : "[red]✗ Not Found[/]",
    !string.IsNullOrEmpty(setupContext.ProjectName) ? setupContext.ProjectName : "No solution file found"
);

AnsiConsole.Write(prereqTable);
AnsiConsole.WriteLine();

// Check if we can continue
if (!setupContext.HasDotnet)
{
    AnsiConsole.MarkupLine("[red]Error: dotnet CLI is not installed. Please install .NET SDK and try again.[/]");
    AnsiConsole.MarkupLine("Visit: [link]https://dotnet.microsoft.com/download[/]");
    Environment.Exit(1);
}

if (!setupContext.HasPnpm && !setupContext.HasNpm)
{
    AnsiConsole.MarkupLine("[red]Error: Neither pnpm nor npm is installed. Please install Node.js/npm or pnpm and try again.[/]");
    AnsiConsole.MarkupLine("Visit: [link]https://nodejs.org/[/] or [link]https://pnpm.io/[/]");
    Environment.Exit(1);
}

if (string.IsNullOrEmpty(setupContext.ProjectName))
{
    AnsiConsole.MarkupLine("[red]Error: No solution file found in the current directory.[/]");
    Environment.Exit(1);
}

var packageManager = setupContext.HasPnpm ? "pnpm" : "npm";
AnsiConsole.MarkupLine($"[green]✓ All prerequisites met. Will use {packageManager} for package management.[/]");
AnsiConsole.WriteLine();




// Restore .NET packages
await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .StartAsync("Restoring .NET packages...", async ctx =>
    {
        try
        {
            // Find the solution file to restore from
            var solutionFile = GetSolutionFile();
            
            var result = await Cli.Wrap("dotnet")
                .WithArguments($"restore \"{solutionFile}\"")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
            
            if (result.ExitCode == 0)
            {
                AnsiConsole.MarkupLine("[green]✓ .NET packages restored successfully[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]✗ Failed to restore .NET packages[/]");
                AnsiConsole.MarkupLine($"[red]{result.StandardError}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error restoring packages: {ex.Message}[/]");
        }
    });

// Install documentation dependencies
await AnsiConsole.Status()
    .Spinner(Spinner.Known.Dots)
    .StartAsync($"Installing documentation dependencies with {packageManager}...", async ctx =>
    {
        try
        {
            var result = await Cli.Wrap(packageManager)
                .WithArguments("install")
                .WithWorkingDirectory("docs")
                .WithValidation(CommandResultValidation.None)
                .ExecuteBufferedAsync();
            
            if (result.ExitCode == 0)
            {
                AnsiConsole.MarkupLine($"[green]✓ Documentation dependencies installed successfully[/]");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]✗ Failed to install documentation dependencies[/]");
                AnsiConsole.MarkupLine($"[red]{result.StandardError}[/]");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]Error installing dependencies: {ex.Message}[/]");
        }
    });

AnsiConsole.WriteLine();
AnsiConsole.Write(new Rule("[yellow]Configuration[/]").LeftJustified());
AnsiConsole.WriteLine();

// Collect NuGet package configuration
AnsiConsole.MarkupLine("[cyan]NuGet Package Configuration[/]");
AnsiConsole.MarkupLine("[grey]Press Enter to skip any field and keep the default value[/]");
AnsiConsole.WriteLine();

var nugetConfig = new NuGetConfiguration();

// GitHub Repository URL
nugetConfig.RepositoryUrl = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]GitHub Repository URL[/] (e.g., https://github.com/owner/repo):")
        .AllowEmpty());

// Description
nugetConfig.Description = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]Package Description[/]:")
        .AllowEmpty());

// Company
nugetConfig.Company = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]Company[/]:")
        .AllowEmpty());

// Authors
nugetConfig.Authors = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]Authors[/] (comma-separated):")
        .AllowEmpty());

// Package tags
nugetConfig.PackageTags = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]Package Tags[/] (semicolon-separated):")
        .AllowEmpty());

// Copyright text
nugetConfig.Copyright = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]Copyright text[/]:")
        .AllowEmpty());

// Package Project URL
nugetConfig.PackageProjectUrl = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]Package Project URL[/] (project homepage):")
        .AllowEmpty());

// If PackageProjectUrl is empty but RepositoryUrl is provided, use RepositoryUrl
if (string.IsNullOrEmpty(nugetConfig.PackageProjectUrl) && !string.IsNullOrEmpty(nugetConfig.RepositoryUrl))
{
    nugetConfig.PackageProjectUrl = nugetConfig.RepositoryUrl;
}

AnsiConsole.WriteLine();
AnsiConsole.MarkupLine("[cyan]Documentation Configuration[/]");
AnsiConsole.WriteLine();

// Documentation configuration
var docsConfig = new DocsConfiguration();

// GitHub Pages base URL
docsConfig.GitHubPagesUrl = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]GitHub Pages base URL[/] (e.g., https://owner.github.io/repo):")
        .AllowEmpty());

// Documentation title
docsConfig.Title = AnsiConsole.Prompt(
    new TextPrompt<string>("[green]Documentation Title[/]:")
        .AllowEmpty());

AnsiConsole.WriteLine();

// Update Directory.Build.props
if (HasNuGetConfigChanges(nugetConfig))
{
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("Updating Directory.Build.props...", async ctx =>
        {
            try
            {
                UpdateDirectoryBuildProps(nugetConfig);
                AnsiConsole.MarkupLine("[green]✓ Directory.Build.props updated successfully[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating Directory.Build.props: {ex.Message}[/]");
            }
            await Task.CompletedTask;
        });
}

// Update astro.config.mjs
if (HasDocsConfigChanges(docsConfig) || !string.IsNullOrEmpty(nugetConfig.RepositoryUrl) || !string.IsNullOrEmpty(nugetConfig.Description))
{
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("Updating astro.config.mjs...", async ctx =>
        {
            try
            {
                UpdateAstroConfig(setupContext.ProjectName, docsConfig, nugetConfig);
                AnsiConsole.MarkupLine("[green]✓ astro.config.mjs updated successfully[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating astro.config.mjs: {ex.Message}[/]");
            }
            await Task.CompletedTask;
        });
}

// Final summary
AnsiConsole.WriteLine();
AnsiConsole.Write(new Rule("[green]Setup Complete![/]").LeftJustified());

var summaryPanel = new Panel(
    $"""
    [green]✓[/] Prerequisites checked
    [green]✓[/] .NET packages restored
    [green]✓[/] Documentation dependencies installed
    [green]✓[/] Configuration files updated
    
    [yellow]Project:[/] {setupContext.ProjectName}
    [yellow]Package Manager:[/] {packageManager}
    
    [cyan]Next Steps:[/]
    1. Review the updated `libs/Directory.Build.props`
    2. Review the updated: `docs/astro.config.mjs`
    3. Run tests: [grey]dotnet run build/tests-run.cs[/]
    4. View documentation: [grey]cd docs && {packageManager} run dev[/]
    """)
    .Border(BoxBorder.Rounded)
    .BorderColor(Color.Green)
    .Header("[green]Repository Setup Complete[/]");

AnsiConsole.Write(summaryPanel);

// Helper functions
async Task<bool> CheckCommand(string command, string args)
{
    try
    {
        var result = await Cli.Wrap(command)
            .WithArguments(args)
            .WithValidation(CommandResultValidation.None)
            .ExecuteBufferedAsync();
        return result.ExitCode == 0;
    }
    catch
    {
        return false;
    }
}

string GetProjectName()
{
    // Try .slnx first
    var slnxFiles = Directory.GetFiles(".", "*.slnx", SearchOption.TopDirectoryOnly);
    if (slnxFiles.Length > 0)
    {
        return Path.GetFileNameWithoutExtension(slnxFiles[0]);
    }
    
    // Then try .sln
    var slnFiles = Directory.GetFiles(".", "*.sln", SearchOption.TopDirectoryOnly);
    if (slnFiles.Length > 0)
    {
        return Path.GetFileNameWithoutExtension(slnFiles[0]);
    }
    
    return string.Empty;
}

string GetSolutionFile()
{
    // Try .slnx first
    var slnxFiles = Directory.GetFiles(".", "*.slnx", SearchOption.TopDirectoryOnly);
    if (slnxFiles.Length > 0)
    {
        return slnxFiles[0];
    }
    
    // Then try .sln
    var slnFiles = Directory.GetFiles(".", "*.sln", SearchOption.TopDirectoryOnly);
    if (slnFiles.Length > 0)
    {
        return slnFiles[0];
    }
    
    throw new FileNotFoundException("No solution file (.sln or .slnx) found in the current directory");
}

bool HasNuGetConfigChanges(NuGetConfiguration config)
{
    return !string.IsNullOrEmpty(config.RepositoryUrl) ||
           !string.IsNullOrEmpty(config.Description) ||
           !string.IsNullOrEmpty(config.Company) ||
           !string.IsNullOrEmpty(config.Authors) ||
           !string.IsNullOrEmpty(config.PackageTags) ||
           !string.IsNullOrEmpty(config.Copyright) ||
           !string.IsNullOrEmpty(config.PackageProjectUrl);
}

bool HasDocsConfigChanges(DocsConfiguration config)
{
    return !string.IsNullOrEmpty(config.GitHubPagesUrl) ||
           !string.IsNullOrEmpty(config.Title);
}

void UpdateDirectoryBuildProps(NuGetConfiguration config)
{
    var filePath = Path.Combine("libs", "Directory.Build.props");
    if (!File.Exists(filePath))
    {
        AnsiConsole.MarkupLine($"[yellow]Warning: {filePath} not found[/]");
        return;
    }
    
    var content = File.ReadAllText(filePath);
    
    // Update RepositoryUrl
    if (!string.IsNullOrEmpty(config.RepositoryUrl))
    {
        content = Regex.Replace(content, 
            @"<RepositoryUrl>.*?</RepositoryUrl>",
            $"<RepositoryUrl>{config.RepositoryUrl}</RepositoryUrl>");
    }
    
    // Update Authors
    if (!string.IsNullOrEmpty(config.Authors))
    {
        content = Regex.Replace(content,
            @"<Authors>.*?</Authors>",
            $"<Authors>{config.Authors}</Authors>");
    }
    
    // Update Copyright
    if (!string.IsNullOrEmpty(config.Copyright))
    {
        content = Regex.Replace(content,
            @"<Copyright>.*?</Copyright>",
            $"<Copyright>{config.Copyright}</Copyright>");
    }
    
    // Update Description
    if (!string.IsNullOrEmpty(config.Description))
    {
        content = Regex.Replace(content,
            @"<Description>.*?</Description>",
            $"<Description>{config.Description}</Description>",
            RegexOptions.Singleline);
    }

    // Update Company
    if (!string.IsNullOrEmpty(config.Company))
    {
        content = Regex.Replace(content,
            @"<Company>.*?</Company>",
            $"<Company>{config.Company}</Company>");
    }

    // Update PackageTags
    if (!string.IsNullOrEmpty(config.PackageTags))
    {
        content = Regex.Replace(content,
            @"<PackageTags>.*?</PackageTags>",
            $"<PackageTags>{config.PackageTags}</PackageTags>");
    }

    // Update Copyright - preserve year calculation
    if (!string.IsNullOrEmpty(config.Copyright))
    {
        content = Regex.Replace(content,
            @"<Copyright>© \$\(\[System\.DateTime\]::Now\.Year\) .*?</Copyright>",
            $"<Copyright>© $([System.DateTime]::Now.Year) {config.Copyright}</Copyright>");
    }

    // Update PackageProjectUrl
    if (!string.IsNullOrEmpty(config.PackageProjectUrl))
    {
        content = Regex.Replace(content,
            @"<PackageProjectUrl>.*?</PackageProjectUrl>",
            $"<PackageProjectUrl>{config.PackageProjectUrl}</PackageProjectUrl>");
    }
    
    File.WriteAllText(filePath, content);
}

void UpdateAstroConfig(string projectName, DocsConfiguration docsConfig, NuGetConfiguration nugetConfig)
{
    var filePath = Path.Combine("docs", "astro.config.mjs");
    if (!File.Exists(filePath))
    {
        AnsiConsole.MarkupLine($"[yellow]Warning: {filePath} not found[/]");
        return;
    }
    
    var content = File.ReadAllText(filePath);
    
    // Replace all '/Six.SolutionTemplate' with '/{projectName}'
    content = content.Replace("/Six.SolutionTemplate", $"/{projectName}");
    
    // Update site URL
    if (!string.IsNullOrEmpty(docsConfig.GitHubPagesUrl))
    {
        content = Regex.Replace(content,
            @"site:\s*'[^']*'",
            $"site: '{docsConfig.GitHubPagesUrl}'");
    }
    
    // Update title
    if (!string.IsNullOrEmpty(docsConfig.Title))
    {
        content = Regex.Replace(content,
            @"title:\s*'[^']*'",
            $"title: '{docsConfig.Title}'");
    }
    
    // Update description from NuGet config
    if (!string.IsNullOrEmpty(nugetConfig.Description))
    {
        content = Regex.Replace(content,
            @"description:\s*'[^']*'",
            $"description: '{nugetConfig.Description}'");
    }
    
    // Update editLink baseUrl
    if (!string.IsNullOrEmpty(nugetConfig.RepositoryUrl))
    {
        // Extract owner and repo from repository URL
        var repoMatch = Regex.Match(nugetConfig.RepositoryUrl, @"github\.com/([^/]+)/([^/]+)/?");
        if (repoMatch.Success)
        {
            var owner = repoMatch.Groups[1].Value;
            var repo = repoMatch.Groups[2].Value.TrimEnd('/');
            
            content = Regex.Replace(content,
                @"baseUrl:\s*'[^']*'",
                $"baseUrl: 'https://github.com/{owner}/{repo}/edit/main/docs/src/content/docs/'");
        }
    }
    
    // Update social link href
    if (!string.IsNullOrEmpty(nugetConfig.RepositoryUrl))
    {
        content = Regex.Replace(content,
            @"href:\s*'https://github\.com/[^']*'",
            $"href: '{nugetConfig.RepositoryUrl.TrimEnd('/')}'");
    }
    
    File.WriteAllText(filePath, content);
}

// Data classes
public class SetupContext
{
    public bool HasDotnet { get; set; }
    public bool HasPnpm { get; set; }
    public bool HasNpm { get; set; }
    public string ProjectName { get; set; } = string.Empty;
}

public class NuGetConfiguration
{
    public string RepositoryUrl { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public string PackageTags { get; set; } = string.Empty;
    public string Copyright { get; set; } = string.Empty;
    public string PackageProjectUrl { get; set; } = string.Empty;
}

public class DocsConfiguration
{
    public string GitHubPagesUrl { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
