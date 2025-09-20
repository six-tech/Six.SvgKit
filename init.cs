#!/usr/bin/env dotnet
#:sdk Microsoft.NET.Sdk
#:property LangVersion=13

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

// Run the setup process
await RunSetupAsync();



// Helper methods
// --------------------------------------------------------------------------------------------------------

async Task RunSetupAsync()
{
// Display welcome message
AnsiConsole.MarkupLine("[grey]Six Solution Template Initalization Tool[/]");
AnsiConsole.WriteLine();

// Warning about file overwrites
AnsiConsole.MarkupLine("[yellow]⚠ Warning: This initialization tool will overwrite the following files if they exist:[/]");
AnsiConsole.MarkupLine("   • [cyan]libs/Directory.Build.props[/] - NuGet package configuration");
AnsiConsole.MarkupLine("   • [cyan]docs/astro.config.mjs[/] - Documentation site configuration");
AnsiConsole.MarkupLine("   • [cyan]docs/src/content/docs/[/] - All documentation pages");
AnsiConsole.WriteLine();

var confirmInitialization = AnsiConsole.Prompt(
    new ConfirmationPrompt("[red]Do you want to proceed with initialization?[/]")
    {
        DefaultValue = false
    });

if (!confirmInitialization)
{
    AnsiConsole.MarkupLine("[yellow]Initialization cancelled by user.[/]");
    Environment.Exit(0);
}

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

  var repositoryName = GetRepositoryName();
prereqTable.AddRow(
    "Project Name",
      !string.IsNullOrEmpty(repositoryName) ? "[green]✓ Detected[/]" : "[red]✗ Not Found[/]",
      !string.IsNullOrEmpty(repositoryName) ? repositoryName : "No solution file found"
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

  if (string.IsNullOrEmpty(repositoryName))
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

AnsiConsole.WriteLine();
AnsiConsole.Write(new Rule("[yellow]Configuration[/]").LeftJustified());
AnsiConsole.WriteLine();

  var projectConfig = new ProjectConfiguration
  {
    RepositoryName = GetRepositoryName() // Use solution file name as repository name
  };

  // Collect project configuration FIRST
  AnsiConsole.MarkupLine("[cyan]Project Configuration[/]");
AnsiConsole.MarkupLine("[grey]Press Enter to skip any field and keep the default value[/]");
AnsiConsole.WriteLine();

  // 1. Project Name
  projectConfig.Name = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]Project Name[/]:")
          .DefaultValue(projectConfig.Name)
          .AllowEmpty());

  // 2. Project Description
  projectConfig.Description = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]Project Description[/]:")
          .DefaultValue(projectConfig.Description)
        .AllowEmpty());

  // 2.1. Project Version
  projectConfig.Version = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]Project Version[/]:")
          .DefaultValue(projectConfig.Version)
        .AllowEmpty());

  // 3. Package Tags
  projectConfig.ProjectTags = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]Package Tags[/]:")
          .DefaultValue(projectConfig.ProjectTags)
        .AllowEmpty());

  // 4. Authors
  projectConfig.Authors = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]Authors[/]:")
          .DefaultValue(projectConfig.Authors)
        .AllowEmpty());

  // 5. Company
  projectConfig.Company = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]Company[/]:")
          .DefaultValue(projectConfig.Company)
        .AllowEmpty());

  // 6. Copyright text
  projectConfig.Copyright = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]Copyright text[/]:")
          .DefaultValue(projectConfig.Copyright)
        .AllowEmpty());

  // 7. GitHub Repository URL
  projectConfig.RepositoryUrl = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]GitHub Repository URL[/]:")
          .DefaultValue(projectConfig.RepositoryUrl)
        .AllowEmpty());

  // 8. Project URL (use repository URL as default)
  var projectUrl = AnsiConsole.Prompt(
      new TextPrompt<string>("[green]Project URL[/]:")
          .DefaultValue(projectConfig.RepositoryUrl)
        .AllowEmpty());

  AnsiConsole.WriteLine();

  // Ask about documentation usage
  AnsiConsole.MarkupLine("[cyan]Documentation Setup[/]");
AnsiConsole.WriteLine();

  projectConfig.UseDocumentation = AnsiConsole.Prompt(
      new ConfirmationPrompt("[green]Do you want to use the documentation site?[/]")
      {
        DefaultValue = projectConfig.UseDocumentation
      });

  if (projectConfig.UseDocumentation)
  {
// GitHub Pages base URL
    projectConfig.GitHubPagesUrl = AnsiConsole.Prompt(
        new TextPrompt<string>("[green]GitHub site URL[/]:")
            .DefaultValue(projectConfig.GitHubPagesUrl)
        .AllowEmpty());
  }

AnsiConsole.WriteLine();

  // Ask about AI features
  AnsiConsole.MarkupLine("[cyan]AI Features Setup[/]");
AnsiConsole.WriteLine();

  projectConfig.UseAgentsMd = AnsiConsole.Prompt(
      new ConfirmationPrompt("[green]Include AGENTS.md file for AI coding assistants?[/]")
      {
        DefaultValue = projectConfig.UseAgentsMd
      });

  projectConfig.UseCursorRules = AnsiConsole.Prompt(
      new ConfirmationPrompt("[green]Include Cursor IDE rules (.cursor directory)?[/]")
      {
        DefaultValue = projectConfig.UseCursorRules
      });

  projectConfig.UseKiroRules = AnsiConsole.Prompt(
      new ConfirmationPrompt("[green]Include Kiro AI steering rules (.kiro directory)?[/]")
      {
        DefaultValue = projectConfig.UseKiroRules
      });

  projectConfig.ConfigureRiderAI = AnsiConsole.Prompt(
    new ConfirmationPrompt("[green]Configure JetBrains Rider AI for conventional commit messages?[/]")
    {
        DefaultValue = projectConfig.ConfigureRiderAI
    });

AnsiConsole.WriteLine();

    // Create Directory.Build.props
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("Updating Directory.Build.props...", async ctx =>
        {
            try
            {
          ConfigureDirectoryBuildProps(projectConfig);
                AnsiConsole.MarkupLine("[green]✓ Directory.Build.props updated successfully[/]");
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Error updating Directory.Build.props: {ex.Message}[/]");
            }
            await Task.CompletedTask;
        });


  // Configure JetBrains Rider AI
  if (projectConfig.ConfigureRiderAI)
{
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("Configuring JetBrains Rider AI...", async ctx =>
        {
            try
            {
            ConfigureRiderAICommitMessages(GetRepositoryName());
            AnsiConsole.MarkupLine("[green]✓ JetBrains Rider AI configured successfully[/]");
            }
            catch (Exception ex)
            {
            AnsiConsole.MarkupLine($"[red]Error configuring JetBrains Rider AI: {ex.Message}[/]");
            }
            await Task.CompletedTask;
        });
}

  // Configure documentation setup
  if (projectConfig.UseDocumentation)
{
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync("Setting up documentation...", async ctx =>
        {
            try
            {
              ConfigureDocumentation(projectConfig);
              AnsiConsole.MarkupLine("[green]✓ Documentation setup completed[/]");
            }
            catch (Exception ex)
            {
              AnsiConsole.MarkupLine($"[red]Error setting up documentation: {ex.Message}[/]");
            }
            await Task.CompletedTask;
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
              AnsiConsole.MarkupLine("[green]✓ Documentation dependencies installed successfully[/]");
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

    // Build documentation site
    await AnsiConsole.Status()
        .Spinner(Spinner.Known.Dots)
        .StartAsync($"Building documentation site with {packageManager}...", async ctx =>
        {
          try
          {
            var result = await Cli.Wrap(packageManager)
                    .WithArguments("run build")
                    .WithWorkingDirectory("docs")
                    .WithValidation(CommandResultValidation.None)
                    .ExecuteBufferedAsync();

            if (result.ExitCode == 0)
            {
              AnsiConsole.MarkupLine("[green]✓ Documentation site built successfully[/]");
            }
            else
            {
              AnsiConsole.MarkupLine($"[red]✗ Failed to build documentation site[/]");
              AnsiConsole.MarkupLine($"[red]{result.StandardError}[/]");
            }
          }
          catch (Exception ex)
          {
            AnsiConsole.MarkupLine($"[red]Error building documentation: {ex.Message}[/]");
          }
        });
  }
  else
  {
    // Delete docs directory if user doesn't want documentation
    if (Directory.Exists("docs"))
    {
      Directory.Delete("docs", true);
      AnsiConsole.MarkupLine("[yellow]○ Documentation directory removed[/]");
    }
  }

  // Handle AI features
  ConfigureAIFeatures(projectConfig);

  // Final summary
AnsiConsole.WriteLine();
AnsiConsole.Write(new Rule("[green]Setup Complete![/]").LeftJustified());

var summaryPanel = new Panel(
    $"""
    [green]✓[/] Prerequisites checked
    [green]✓[/] .NET packages restored
    {(projectConfig.UseDocumentation ? "[green]✓[/] Documentation configured" : "[grey]○[/] Documentation skipped")}
    {(projectConfig.UseDocumentation ? "[green]✓[/] Documentation dependencies installed" : "")}
    
    [green]✓[/] Configuration files updated
    {(projectConfig.UseDocumentation ? "[green]✓[/] Documentation setup completed" : "[grey]○[/] Documentation disabled")}
    {(projectConfig.UseAgentsMd ? "[green]✓[/] AGENTS.md included" : "[grey]○[/] AGENTS.md removed")}
    {(projectConfig.UseCursorRules ? "[green]✓[/] Cursor rules included" : "[grey]○[/] Cursor rules removed")}
    {(projectConfig.UseKiroRules ? "[green]✓[/] Kiro rules included" : "[grey]○[/] Kiro rules removed")}
    {(projectConfig.ConfigureRiderAI ? "[green]✓[/] JetBrains Rider AI configured" : "[grey]○[/] JetBrains Rider AI skipped")}

    [yellow]Project:[/] {projectConfig.RepositoryName}
    [yellow]Documentation Package Manager:[/] {packageManager}

    [cyan]Next Steps:[/]
    1. Review the updated `libs/Directory.Build.props`
    {(projectConfig.UseDocumentation ? "2. Review the updated: `docs/astro.config.mjs`" : "2. Run tests: [grey]dotnet run scripts/tests-run.cs[/]")}
    {(projectConfig.UseDocumentation ? "3. View documentation: [grey]cd docs && " + packageManager + " run dev[/]" : "3. Check your project structure")}
    {(projectConfig.ConfigureRiderAI ? $"4. Review the created Rider AI configuration in `.idea/.idea.{projectConfig.RepositoryName}/.idea/workspace.xml`" : "")}
    {("Finally, explore the documentation of Six Solution Template: https://six-tech.github.io/Six.SolutionTemplate/")}
    """)
    .Border(BoxBorder.Rounded)
    .BorderColor(Color.Green)
    .Header("[green]Repository Setup Complete[/]");

AnsiConsole.Write(summaryPanel);
}

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

string GetRepositoryName()
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




// -- Directroy.Build.props for libraries/packages --
void ConfigureDirectoryBuildProps(ProjectConfiguration config)
{
  // This page string shoud be saved in/as: libs/Directory.Build.props
  var propsContent = $$$"""
            <!-- Directory.Build.props for libraries -->
            <Project>
                <!-- Import parent Directory.Build.props -->
                <Import Project="$(MSBuildThisFileDirectory)../Directory.Build.props"
                        Condition="Exists('$(MSBuildThisFileDirectory)../Directory.Build.props')"/>

                <PropertyGroup>
                    <TargetFramework>net9.0</TargetFramework>

                    <!-- Version -->
                    <Version>{{{config.Version}}}</Version>

                    <!-- Packaging -->
                    <IsPackable>true</IsPackable>
                    <SignAssembly>true</SignAssembly>
                    <GeneratePackageOnBuild>false</GeneratePackageOnBuild>

                    <!-- Company/Organization/Legal Information -->
                    <Authors>{{{config.Authors}}}</Authors>
                    <Company>{{{config.Company}}}</Company>
                    <Description>{{{config.Description}}}</Description>
                    <Copyright>© $([System.DateTime]::Now.Year) {{{config.Copyright}}}.</Copyright>

                    <!-- Package Information -->
                    <PackageTags>{{{config.ProjectTags}}}</PackageTags>
                    <PackageLicenseExpression>{{{config.ProjectLicense}}}</PackageLicenseExpression>
                    <PackageRequireLicenseAcceptance>false</PackageRequireLicenseAcceptance>
                    <PackageProjectUrl>{{{config.RepositoryUrl}}}</PackageProjectUrl>        

                    <!-- Repository Information -->
                    <RepositoryUrl>{{{config.RepositoryUrl}}}</RepositoryUrl>
                    <RepositoryType>git</RepositoryType>

                    <!-- Embedded Assets -->
                    <PackageReadmeFile>README.md</PackageReadmeFile>
                    <PackageIcon>$(MSBuildThisFileDirectory)\project_icon.png</PackageIcon>

                    <!-- 
                        XML Documentation
                        Also turn off warnings for missing documentation 
                    -->
                    <GenerateDocumentationFile>True</GenerateDocumentationFile>
                </PropertyGroup>


                <!-- 
                    SOURCE DEBUGGING 
                -->
                <PropertyGroup>

                    <!-- Enable Source Link -->
                    <PublishRepositoryUrl>true</PublishRepositoryUrl>
                    <EmbedUntrackedSources>true</EmbedUntrackedSources>
                    <IncludeSymbols>true</IncludeSymbols>
                    <SymbolPackageFormat>snupkg</SymbolPackageFormat>

                    <!-- Recommended for deterministic builds in CI -->
                    <ContinuousIntegrationBuild Condition="'$(CI)' == 'true'">true</ContinuousIntegrationBuild>

                    <!-- Debug Symbols         
                    It’s important to include debug information in your package for better maintainability and support. 
                    This typically involves setting the correct debug type in your project files.
                    
                    If you wany to embed the PDBs in the package, you can set the DebugType property to 'embedded'.
                    See more: https://github.com/dotnet/sourcelink 
                    -->
                    <DebugType>portable</DebugType>

                </PropertyGroup>
                <ItemGroup>
                    <!--
                    Source links on GitHub
                    See: https://github.com/dotnet/sourcelink 
                     
                    There'a a new version (8.0.0) but is seems it have problems with source generators 
                    -->
                    <PackageReference Include="Microsoft.SourceLink.GitHub" Version="1.1.1" PrivateAssets="All"/>
                </ItemGroup>


                <!-- 
                    Package Validation
                    See: https://learn.microsoft.com/en-us/dotnet/fundamentals/package-validation/overview
                -->
                <PropertyGroup>
                    <!--            
                        <EnablePackageValidation>true</EnablePackageValidation>
                    
                        Uncomment to validate against some previous package (API changes) 
                        https://docs.microsoft.com/en-us/nuget/reference/msbuild-targets#packagevalidationbaselineversion
                        <PackageValidationBaselineVersion>1.0.0</PackageValidationBaselineVersion>
                    -->
                </PropertyGroup>

                <!--
                    To include the README.md file in the packages
                -->
                <ItemGroup Condition="'$(MSBuildProjectExtension)' != '.shproj'">
                    <None Include="README.md" Pack="true" PackagePath="\"/>
                    <None Include="project_icon.png" Pack="true" PackagePath="\"/>
                </ItemGroup>

            </Project>
            """;

  // Save to libs/Directory.Build.props
  if (Directory.Exists("libs") == false)
  {
    AnsiConsole.MarkupLine("[yellow] Libs directory not found; Creating[/]");
    Directory.CreateDirectory("libs");
  }
  
  var props = Path.Combine("libs", "Directory.Build.props");
  
  if (File.Exists(props))
    File.Delete(props);
  
  File.WriteAllText(props, propsContent);
}


// -- AI Features --
void ConfigureAIFeatures(ProjectConfiguration projectConfig)
{
  // Handle AGENTS.md
  if (!projectConfig.UseAgentsMd)
  {
    if (File.Exists("AGENTS.md"))
    {
      File.Delete("AGENTS.md");
      AnsiConsole.MarkupLine("[yellow]○ AGENTS.md removed[/]");
    }
  }

  // Handle .cursor directory
  if (!projectConfig.UseCursorRules)
  {
    if (Directory.Exists(".cursor"))
    {
      Directory.Delete(".cursor", true);
      AnsiConsole.MarkupLine("[yellow]○ Cursor rules directory removed[/]");
    }
  }

  // Handle .kiro directory
  if (!projectConfig.UseKiroRules)
  {
    if (Directory.Exists(".kiro"))
    {
      Directory.Delete(".kiro", true);
      AnsiConsole.MarkupLine("[yellow]○ Kiro rules directory removed[/]");
    }
  }

}

void ConfigureRiderAICommitMessages(string projectName)
{
    if (string.IsNullOrEmpty(projectName))
    {
        throw new ArgumentException("Project name cannot be empty");
    }

    // Create directory structure: .idea/.idea.{projectName}/.idea/
    var baseDir = Path.Combine(".idea", $".idea.{projectName}", ".idea");
    Directory.CreateDirectory(baseDir);

    // Create workspace.xml file
    var workspaceXmlPath = Path.Combine(baseDir, "workspace.xml");
    var workspaceXmlContent = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<project version=""4"">
  <component name=""AIAssistantCustomInstructionsStorage"">
    <option name=""instructions"">
      <map>
        <entry key=""AIAssistant.VCS.GenerateCommitMessage"">
          <value>
            <AIAssistantStoredInstruction>
              <option name=""actionId"" value=""AIAssistant.VCS.GenerateCommitMessage"" />
              <option name=""content"" value=""1) Describe all changes in a short and descriptive manner.&#10;&#10;2) ALWAYS use conventional commit format. &#10;Examples:&#10;&#10;Good:&#10;ref: Refactored this and that&#10;docs: Updated documentation for this and that&#10;fix: Fixed issues #284343434&#10;&#10;Bad:&#10;Refactored this and that&#10;Updated documentation for this and that&#10;Fixed issues #284343434&#10;&#10;3) Never surround commit messages with ```&#10;&#10;4) Use bullet points for multiple changes"" />
            </AIAssistantStoredInstruction>
          </value>
        </entry>
      </map>
    </option>
  </component>
</project>";

    File.WriteAllText(workspaceXmlPath, workspaceXmlContent);
}


// -- In-Repo Documentation Setup --

void ConfigureDocumentation(ProjectConfiguration projectConfig)
{
  // Delete all content inside docs/src/content/docs (but keep the directory for IDE references)
  var docsContentPath = Path.Combine("docs", "src", "content", "docs");
  if (Directory.Exists(docsContentPath))
  {
    // Get all files and directories once
    var files = Directory.GetFiles(docsContentPath);
    var directories = Directory.GetDirectories(docsContentPath);
    
    // Delete all files in the root docs directory
    foreach (var file in files)
    {
      File.Delete(file);
    }
    
    // Delete all subdirectories (this will delete all nested content too)
    foreach (var dir in directories)
    {
      Directory.Delete(dir, recursive: true);
    }
  }

  // Delete astro.config.mjs in docs/
  var astroConfigPath = Path.Combine("docs", "astro.config.mjs");
  if (File.Exists(astroConfigPath))
  {
    File.Delete(astroConfigPath);
  }

  // Create documentation files using the create methods
  CreateAstroConfig(
      projectConfig.Name,
      projectConfig.Description,
      projectConfig.RepositoryName,
      projectConfig.RepositoryUrl,
      projectConfig.GitHubPagesUrl);

  CreateIndexPage(
      projectConfig.Name,
      projectConfig.Description,
      projectConfig.RepositoryName);

  CreateIntroductionPage(
      projectConfig.Name,
      projectConfig.Description);

  CreateSetupAndConfigurationPage(projectConfig.Name);
}

void CreateAstroConfig(string projectName, string projectDescription, string repositoryName, string repositoryUrl, string githubPagesRootUrl)
{
  var config = $$$"""
            import {defineConfig} from 'astro/config';
            import starlight from '@astrojs/starlight';
            import starlightThemeSix from '@six-tech/starlight-theme-six'
            
            export default defineConfig({
                base: '/{{{repositoryName}}}/', // Last fragment for GitHub pages (your repo name) - i.e: /Six.SolutionTemplate/
                site: '{{{githubPagesRootUrl}}}', // Base URL for your docs site (your GitHub pages URL) - i.e: https://six-tech.github.io
                integrations: [
                    starlight({
                        title: '{{{projectName}}}',
                        description: '{{{projectDescription}}}',
                        editLink: {
                            // Base url for edit links on GitHub. Basically edit-url to the docs' folder.
                            baseUrl: '{{{repositoryUrl}}}/edit/main/docs/', // i.e: https://github.com/six-tech/Six.SolutionTemplate
                        },
                        social: [
                            {
                                icon: 'github',
                                label: 'GitHub',
                                href: '{{{repositoryUrl}}}'
                            },
                        ],
                        plugins: [
                            starlightThemeSix({
                                navLinks: [
                                    {
                                        label: 'Docs',
                                        title: 'Link to main documentation page',
                                        link: '/introduction',
                                    },
                                    {
                                        label: 'Six',
                                        title: 'Link to Six homepage',
                                        link: 'https://six.technology',
                                        attrs: {
                                            target: '_blank',
                                            rel: 'noopener',
                                        },
                                    },
                                ],
                            }),
                        ],
                        sidebar: [
                            {
                                label: 'Introduction',
                                items: [
                                    {label: 'Introduction', slug: 'introduction'},
                                    {label: 'Setup and Configuration', slug: 'introduction/setup-and-configuration'}
                                ],
                            },
                        ],
            
                    }),
                ],
            });
            
            
            """;

  // Save to docs/astro.config.mjs
  var configPath = Path.Combine("docs", "astro.config.mjs");
  File.WriteAllText(configPath, config);
}

void CreateIndexPage(string projectName, string projectDescription, string repositoryName, string? heroBannerImage = "six-solution-hero-banner-blank.png")
{
  var page = $$$"""
            ---
            title: {{{projectName}}}
            description: {{{projectDescription}}}
            head:
              - tag: title
            editUrl: false
            template: splash
            ---
            
            import {Card, CardGrid} from '@astrojs/starlight/components'
            import {ContainerSection, HeroX} from '@six-tech/starlight-theme-six/components'
            import heroImage from '../../assets/{{{heroBannerImage}}}'
            
            <HeroX
                height="400px"
                mt="2rem"
                mb="0"
                px="4rem"
                radius="1.5rem"
                image={{
                    background: heroImage,
                    alt: "Hero background image",
                    backgroundSize: "cover",
                    backgroundPosition: "center",
                    backgroundRepeat: "no-repeat",
                    contentPosition: "bottom-left"
                }}
                actions={[
                    {
                        text: "Get Started",
                        link: "/{{{repositoryName}}}/introduction/"
                    }
                ]}
            />
            
            <ContainerSection>
                <CardGrid>
                    <Card title="Feature 1" icon="rocket">
                        Describe you feature here...
                    </Card>
            
                    <Card title="Feature 2" icon="setting">
                        Describe you feature here...
                    </Card>
                </CardGrid>
            </ContainerSection>
            """;

  // Save to docs/src/content/docs/index.mdx
  var indexPath = Path.Combine("docs", "src", "content", "docs", "index.mdx");
  Directory.CreateDirectory(Path.GetDirectoryName(indexPath)!);
  File.WriteAllText(indexPath, page);
}

void CreateIntroductionPage(string projectName, string projectDescription, string? heroBannerImage = "project-banner.png")
{
  var page = $$$"""
            ---
            title: Introduction
            description: {{{projectDescription}}}
            ---
            
            import {Aside, CardGrid, LinkCard} from '@astrojs/starlight/components'
            
            ![Hero Image](../../../assets/{{{heroBannerImage}}})
            
            # Welcome to {{{projectName}}}
            
            **{{{projectName}}}** is a {{{projectDescription}}} 
            
            <Aside type="tip">
                This project has been built using the [**Six Solution Template**](https://six-tech.github.io/Six.SolutionTemplate/six/introduction/).
                Therefore, **all features provided by the template are available out of the box.**
            
                Please explore the **Six Solution Template** [feature-set here](https://six-tech.github.io/Six.SolutionTemplate/six/introduction/)
            </Aside>
            
            
            **Ready to explore?** Dive into our detailed documentation to understand how each feature works and how to customize it 
            for your specific needs.
            
            """;

  // Save to docs/src/content/docs/introduction.mdx
  var introPath = Path.Combine("docs", "src", "content", "docs", "introduction", "index.mdx");
  Directory.CreateDirectory(Path.GetDirectoryName(introPath)!);
  File.WriteAllText(introPath, page);
}

void CreateSetupAndConfigurationPage(string projectName, string? heroBannerImage = "six-solution-hero-banner.png")
{
  var page = $$$"""
            ---
            title: Setup and Configuration
            description: How to set up and configure your new project
            ---
            
            import {CardGrid, Card, Steps, Aside} from '@astrojs/starlight/components'
            
            # Introduction
            
            This solution provides an automated initialization process that handles all the initial configuration tasks,
            making it easy to get started with your new project.
            
            ## Prerequisites
            
            Before running the setup, ensure you have the following tools installed:
            
            <CardGrid>
            
            <Card title="🔧 .NET 10 SDK" icon="download">
            ### Required Tool
            - **Download**: [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download)
            - **Purpose**: Builds and runs .NET projects, executes C# scripts
            - **Verification**: Run `dotnet --version` to check installation
            </Card>
            
            <Card title="📦 Package Managers" icon="npm">
            ### Choose One
            #### pnpm (Recommended)
            - **Install**: `npm install -g pnpm`
            - **Advantages**: Faster, disk-efficient, better dependency management
            - **Website**: [pnpm.io](https://pnpm.io/)
            
            #### npm (Alternative)
            - **Included with**: [Node.js](https://nodejs.org/)
            - **Verification**: Run `npm --version` to check installation
            </Card>
            
            </CardGrid>
            
            # Quick Start
            
            After cloning the repository and ensuring all prerequisites are available, it is as simple as running the automated 
            initialization from the solution root directory to set up your project. 
            
            But before that, let's take a look at the features provided by the **Six Solution Template**.
            
            ## Six Solution Template
            
            ![Hero Image](../../../assets/{{{heroBannerImage}}})
            
            This project has been built using the [**Six Solution Template**](https://six-tech.github.io/Six.SolutionTemplate/six/introduction/).
            Therefore, all features provided by the template are available out of the box.
            
            - [Standardized project structure](https://six-tech.github.io/Six.SolutionTemplate/six/structure/) for libraries, applications, tests, and benchmarks
            - [Hierarchical configuration management](https://six-tech.github.io/Six.SolutionTemplate/six/structure/str-configuration/) with sensible defaults
            - [C#-based development scripts](https://six-tech.github.io/Six.SolutionTemplate/six/scripts/) for cross-platform automation (testing, packaging, and publishing)
            - [Cursor and Kiro rules](https://six-tech.github.io/Six.SolutionTemplate/six/ai/introduction/) for AI assistance
            - [In-Repository Documentation](https://six-tech.github.io/Six.SolutionTemplate/six/project-documentation/setting-up-documentation/)
            - [Automatic deployment](https://six-tech.github.io/Six.SolutionTemplate/six/github-workflow-integration/) of documentation to GitHub Pages
            
            
            Please explore the whole [feature-set here](https://six-tech.github.io/Six.SolutionTemplate/six/introduction/)
            
            
            ## Automated Setup Process
            After cloning the repository, run the automated initialization from the solution root directory:
            
            <Aside type="caution" title="File Overwrite Warning">
                The initialization tool will overwrite the following files if they exist:
                - `libs/Directory.Build.props` - NuGet package configuration
                - `docs/astro.config.mjs` - Documentation site configuration
                - `docs/src/content/docs/` - All documentation pages
            
                Make sure to backup any custom changes before running the initialization.
            </Aside>
            
            ```bash
            dotnet run init.cs
            ```
            
            The initialization script will:
            1. ✅ Check all prerequisites
            2. 🔄 Restore .NET NuGet packages
            3. ⚙️ Guide you through project configuration
            4. 📚 Setup documentation (if enabled)
            5. 📦 Install documentation dependencies (if enabled)
            6. 🤖 Configure AI features (AGENTS.md, Cursor rules, Kiro rules)
            7. 🛠️ Setup JetBrains Rider AI commit messages (if enabled)
            8. 📝 Generate all configuration files
            
            
            # Automated Initialization Process
            
            <Steps>
            
            1. **File Overwrite Confirmation**
               The script first warns about files that will be overwritten and asks for confirmation before proceeding.
            
            2. **Prerequisites Check**
               Automatically verifies:
               - **dotnet CLI** availability and version
               - **pnpm** or **npm** installation
               - **Solution file** detection (.slnx or .sln)
            
            3. **Package Restoration**
               Runs `dotnet restore` to restore all NuGet packages for the solution.
            
            4. **Interactive Project Configuration**
               Guides you through configuring your project with prompts for:
               - **Project Name** (human-readable)
               - **Project Description**
               - **Package Tags** (semicolon-separated)
               - **Authors** (comma-separated)
               - **Company**
               - **Copyright text**
               - **GitHub Repository URL**
               - **Project URL**
            
            5. **Documentation Configuration**
               If documentation is enabled:
               - **GitHub Pages base URL**
               - Documentation title (automatically set from project name)
            
            6. **AI Features Configuration**
               Configure AI integration options:
               - **AGENTS.md** file for AI coding assistants
               - **Cursor IDE rules** (.cursor directory)
               - **Kiro AI steering rules** (.kiro directory)
               - **JetBrains Rider AI** conventional commit messages
            
            7. **File Generation**
               Generates all configuration files:
               - **`libs/Directory.Build.props`**: Complete NuGet package configuration
               - **`docs/astro.config.mjs`**: Documentation site configuration
               - **Documentation pages**: Introduction, setup guide, and index pages
            
            8. **Documentation Dependencies**
               If documentation is enabled, installs Node.js dependencies using `pnpm` or `npm`.
            
            </Steps>
            
            
            ---- 
            
            # Manual Setup (Alternative)
            
            If you prefer to set up manually or need to customize specific aspects:
            
            ## 1. Restore .NET Packages
            ```bash
            dotnet restore
            ```
            
            ## 2. Install Documentation Dependencies
            ```bash
            cd docs
            pnpm install
            # or
            npm install
            ```
            
            ## 3. Configure NuGet Package Properties
            Edit `libs/Directory.Build.props`:
            ```xml
            <PropertyGroup>
              <RepositoryUrl>https://github.com/your-username/your-repo</RepositoryUrl>
              <Authors>Your Name</Authors>
              <Copyright>Copyright © 2024</Copyright>
              <Description>Your project description</Description>
              <PackageProjectUrl>https://your-project-url.com</PackageProjectUrl>
            </PropertyGroup>
            ```
            
            ## 4. Configure Documentation
            Edit `docs/astro.config.mjs`:
            ```javascript
            export default defineConfig({
              site: 'https://your-username.github.io/your-repo',
              title: 'Your Project Documentation',
              description: 'Your project description',
              // ... other configuration
            });
            ```
            
            
            
            
            # Troubleshooting
            
            ## .NET CLI Not Found
            ```
            Error: dotnet CLI is not installed
            ```
            **Solution**: Install .NET 10 SDK from [dotnet.microsoft.com](https://dotnet.microsoft.com/download)
            
            ## No Package Manager Found
            ```
            Error: Neither pnpm nor npm is installed
            ```
            **Solution**: Install Node.js (includes npm) from [nodejs.org](https://nodejs.org/) or pnpm with `npm install -g pnpm`
            
            
            ## Solution File Not Found
            ```
            Error: No solution file found in the current directory
            ```
            **Solution**: Ensure you're running the setup from the repository root directory containing `.sln` or `.slnx` files
            
            ## Package Installation Failures
            If package installation fails:
            1. Check your internet connection
            2. Verify Node.js version compatibility
            3. Try clearing package manager caches:
               ```bash
               # For pnpm
               pnpm store prune
               
               # For npm
               npm cache clean --force
               ```
            
            
            """;

  // Save to docs/src/content/docs/setup-and-configuration.mdx
  var setupPath = Path.Combine("docs", "src", "content", "docs", "introduction", "setup-and-configuration.mdx");
  Directory.CreateDirectory(Path.GetDirectoryName(setupPath)!);
  File.WriteAllText(setupPath, page);
}



// Data classes
// -----------------------------------------------------------------------------------------------------

public record SetupContext
{
    public bool HasDotnet { get; set; }
    public bool HasPnpm { get; set; }
    public bool HasNpm { get; set; }
}

/// <summary>
/// Represents the configuration details for a project.
/// This record provides metadata information about the project such as its name, description,
/// repository location, authors, licensing, and other related properties.
/// </summary>
public record ProjectConfiguration
{
  /// <summary>
  /// Gets or sets the project name
  /// </summary>
  public string Name { get; set; } = "Six Solution Template";

  /// Gets or sets the description of the project.
  /// This property provides a detailed overview of the purpose and features
  /// of the solution template, offering clear insights into its functionality
  /// and intended use.
  public string Description { get; set; } = "A production-ready solution template that provides a complete foundation for building .NET libraries and applications";

  /// <summary>
  /// Gets or sets the version of the project.
  /// </summary>
  /// <remarks>This property will be used in NuGet packaging to set <c>Version</c> attribute.</remarks>
  public string Version { get; set; } = "1.0.0-alpha.1";
  
  /// <summary>
  /// Gets or sets the name of the repository associated with the project.
  /// </summary>
  /// <remarks>
  /// If empty, it will be extracted from solution name <c>(.slnx or .sln)</c>
  /// </remarks>
  public string RepositoryName { get; set; } = string.Empty;

  /// <summary>
  /// <para>
  /// Gets or sets the URL of the repository associated with the project.
  /// </para>
  /// <para>
  /// This property represents the URL where the source code for the project is hosted,
  /// typically on a platform like GitHub, GitLab, or Bitbucket.
  /// </para>
  /// </summary>
  /// <remarks>
  /// This property will be used in NuGet packaging to set <c>RepositoryUrl</c> attribute
  /// and several properties related to documentation.
  /// </remarks>
  public string RepositoryUrl { get; set; } = "https://github.com/six-tech/Six.SolutionTemplate";

  /// <summary>
  /// Gets or sets the name of the company associated with the project configuration.
  /// </summary>
  /// <remarks>This property will be used in NuGet packaging to set <c>Company</c> attribute.</remarks>
  public string Company { get; set; } = "Six Technology";

  /// <summary>
  /// Gets or sets the authors of the project.
  /// This property specifies the individuals or organizations responsible for creating the project.
  /// </summary>
  /// <remarks>This property will be used in NuGet packaging to set <c>Authors</c> attribute.</remarks>
  public string Authors { get; set; } = "Six Technology";

  /// <summary>
  /// Gets or sets the tags associated with the project. These tags represent key characteristics
  /// or topics related to the project, which can be used for categorization or discovery purposes.
  /// </summary>
  /// <remarks>This property will be used in NuGet packaging to set <c>PackageTags</c> attribute.</remarks>
  public string ProjectTags { get; set; } = "dotnet;csharp";

  /// <summary>
  /// Gets or sets the license under which the project is distributed.
  /// </summary>
  /// <remarks>
  /// Specifies the licensing terms for the project. Common examples include "MIT", "Apache-2.0", "GPL-3.0", etc.
  /// Default is <c>MIT</c>
  /// </remarks>
  public string ProjectLicense { get; set; } = "MIT";

  /// <summary>
  /// Gets or sets the copyright information for the project, typically including the owner and any associated legal rights or restrictions.
  /// </summary>
  public string Copyright { get; set; } = "Six Technology, Inc. All rights reserved.";

  // AI Configuration Properties

  /// <summary>
  /// Gets or sets whether to include AGENTS.md file for AI coding assistants.
  /// </summary>
  public bool UseAgentsMd { get; set; } = true;

  /// <summary>
  /// Gets or sets whether to include Cursor IDE rules (.cursor directory).
  /// </summary>
  public bool UseCursorRules { get; set; } = true;

  /// <summary>
  /// Gets or sets whether to include Kiro AI steering rules (.kiro directory).
  /// </summary>
  public bool UseKiroRules { get; set; } = true;

  /// <summary>
  /// Gets or sets whether to configure JetBrains Rider AI for conventional commit messages.
  /// </summary>
  public bool ConfigureRiderAI { get; set; } = true;

    
  // Documentation Configuration Properties

  /// <summary>
  /// Gets or sets the base URL for GitHub Pages associated with the project repository.
  /// </summary>
  /// <remarks>
  /// This property provides the root URL for hosting project-related documentation or web resources
  /// on GitHub Pages. It typically corresponds to the GitHub Pages endpoint of the repository.
  /// </remarks>
  public string GitHubPagesUrl { get; set; } = "https://six-tech.github.io";

  /// <summary>
  /// Gets or sets whether to use documentation for this project.
  /// </summary>
  public bool UseDocumentation { get; set; } = true;
}

