# AGENTS.md

A comprehensive guide for AI coding agents working with the **Six.SolutionTemplate** repository. This template provides
the context and instructions needed to help AI agents work effectively on .NET C# projects using this template.

## 🏗️ Project Overview

**Six.SolutionTemplate** is a GitHub template repository for creating .NET C# projects with professional tooling,
automated workflows, and professional documentation setup. It's designed for building libraries and applications with
minimal configuration overhead.

### Key Components

- **Application Projects**: Located in `apps/` directory
- **Library Projects**: Located in `libs/` directory
- **Test Projects**: Located in `tests/` directory
- **Benchmark Projects**: Located in `benchmarks/` directory
- **Build Scripts**: **C# file-based apps** in `scripts/` directory (requires .NET 10)
- **Documentation**: Astro.js Starlight site in `docs/` directory
- **CI/CD**: GitHub Actions workflows in `.github/workflows/`

## 🚀 Setup Commands

### Prerequisites

- **Git**: For version control and GitVersion integration
- **.NET 10.0.100-rc.1 SDK or later**: Required for C# file-based apps (build scripts); projects target .NET 9.0
- **Node.js**: For documentation site build
- **pnpm/npm**: For documentation site build
- **Docker**: For containerized testing (optional but recommended)


### Initial Setup

```bash
# Run the comprehensive setup script (RECOMMENDED)
# This will check prerequisites, restore packages, install docs dependencies,
# and guide you through configuration
dotnet run setup.cs
```

### Development Commands

```bash
# Build the solution
dotnet build

# Run all tests
dotnet run scripts/tests-run.cs

# Run tests with coverage
dotnet run scripts/tests-coverage.cs

# Generate coverage reports
dotnet run scripts/tests-report.cs

# Package and publish NuGet packages
dotnet run scripts/nuget-pack.cs --prod
```

### Setup Script Details

The `setup.cs` script is a comprehensive setup and configuration tool that automates the initial project setup:

**What it does:**
- ✅ Checks prerequisites (dotnet CLI, pnpm/npm)
- ✅ Restores .NET packages automatically
- ✅ Installs documentation dependencies
- ✅ Guides you through NuGet package configuration (GitHub repository URL, description, company, authors, tags, copyright)
- ✅ Updates configuration files (`libs/Directory.Build.props`, `docs/astro.config.mjs`)
- ✅ Provides clear next steps and summary

**Configuration Options:**
- GitHub Repository URL
- Package description and metadata
- Company and author information
- Package tags and copyright
- Documentation site configuration (GitHub Pages URL, title)

**Benefits:**
- Streamlined onboarding process
- Consistent configuration across team members
- Automatic detection of project structure
- Interactive prompts with sensible defaults
- Comprehensive error handling and validation

## 🧪 Testing Instructions

### Running Tests

- **All Tests**: `dotnet run scripts/tests-run.cs`
- **With Coverage**: `dotnet run scripts/tests-coverage.cs`
- **Coverage Reports**: `dotnet run scripts/tests-report.cs`
- **Specific Test Project**: Navigate to `tests/` directory and run `dotnet test`

### Test Structure

- Test projects are located in `tests/` directory
- Each test project references corresponding library projects
- Tests use `xUnit` framework with coverlet for coverage

### Coverage Requirements

- Generate coverage reports before committing
- Ensure all new code has corresponding tests
- Coverage reports are automatically generated in CI/CD

## 🔧 Build and Development

### Project Structure

```
Six.SolutionTemplate/
├── apps/                    # Application projects (console, web applications, Avalonia etc.)
├── libs/                    # Library projects 
├── tests/                   # Test projects
├── benchmarks/              # Benchmarking projects    
├── scripts/                 # C# build scripts
├── docs/                    # Documentation site
└── .github/workflows/       # CI/CD pipelines
```

### Build Scripts

The `scripts/` directory contains C# file-based apps for common development tasks:

- **nuget-pack.cs**: NuGet packaging and publishing
- **tests-run.cs**: Test execution
- **tests-coverage.cs**: Coverage collection
- **tests-report.cs**: Coverage reporting

## ⚙️ Configuration Files

The template includes several configuration files that ensure consistent development, versioning, and packaging:

### Core Configuration Files

**`global.json`** - .NET SDK Version Management
- Specifies the exact .NET SDK version (10.0.100-rc.1) required for build scripts
- Ensures consistent development environment across team members
- Enables rollForward to latestMinor for patch updates

**`Directory.Build.props`** - Master Build Configuration
- Centralized build settings for all projects
- Enables C# 13, nullable reference types, implicit usings
- Configures StyleCop, code analysis, and warning settings
- Sets up CI/CD optimizations for deterministic builds

**`stylecop.json`** - Code Style Enforcement
- Defines coding standards and style rules
- Integrated with build process to ensure consistent code formatting
- Configurable rules for code quality and maintainability

### Versioning & Dependencies

**`GitVersion.yml`** - Semantic Versioning Configuration
- Automated version calculation based on Git history and branch naming
- Supports conventional commits for automatic incrementing
- Configures different versioning strategies for main, release, feature, and hotfix branches
- Integrates with CI/CD pipelines for consistent versioning

**`dependabot.yml`** - Automated Dependency Updates
- Weekly checks for NuGet package updates
- Automated pull requests for dependency updates
- Ensures security updates and latest features are available
- Reduces manual maintenance overhead

### Package Management

**`nuget.config`** - NuGet Package Sources Configuration
- Defines multiple package sources (Local, Development, Staging, Production)
- Environment variable-based configuration for security
- Supports different deployment environments
- Comprehensive documentation in comments for setup

**`libs/Directory.Build.props`** - Library-Specific Configuration
- NuGet packaging settings for library projects
- SourceLink configuration for debugging support
- Package metadata and repository information
- License and copyright configuration

## 📦 NuGet Packaging

### Environment Configuration

Configure NuGet feeds using environment variables:

```bash
# Local development feed
export NUGET_FEED_LOCAL_URL="/path/to/local/feed"

# Development feed
export NUGET_FEED_DEV_URL="https://nuget.pkg.github.com/NAMESPACE/index.json"
export NUGET_FEED_DEV_USERNAME="your-username"
export NUGET_FEED_DEV_PAT="your-dev-personal-access-token"

# Staging feed
export NUGET_FEED_STAGING_URL="https://nuget.pkg.github.com/NAMESPACE/index.json"
export NUGET_FEED_STAGING_USERNAME="your-username"
export NUGET_FEED_STAGING_PAT="your-staging-personal-access-token"

# Production feed (NuGet.org for example)
export NUGET_FEED_PRODUCTION_URL="https://api.nuget.org/v3/index.json"
export NUGET_FEED_PRODUCTION_USERNAME="your-username"
export NUGET_FEED_PRODUCTION_PAT="your-production-api-key"
```

### Packaging Commands

```bash
# Package to local feed (default)
dotnet run scripts/nuget-pack.cs
dotnet run scripts/nuget-pack.cs --local

# Package to development feed
dotnet run scripts/nuget-pack.cs --dev
dotnet run scripts/nuget-pack.cs --development

# Package to staging feed
dotnet run scripts/nuget-pack.cs --staging

# Package to production feed (NuGet.org)
dotnet run scripts/nuget-pack.cs --prod
dotnet run scripts/nuget-pack.cs --production

# Package with debug configuration
dotnet run scripts/nuget-pack.cs --debug --local
```

### Package Configuration

- Package metadata is centralized in `Directory.Build.props`
- Individual projects can override specific settings
- GitVersion handles semantic versioning automatically
- SourceLink is configured for debugging support

## 📚 Documentation

### Documentation Site

- Built with Astro.js Starlight using **Six** theme
- Located in `docs/` directory
- Automatically deployed to GitHub Pages on push to main
- Supports Markdown and MDX content

### Documentation Commands

```bash
# Start development server
cd docs
npm run dev

# Build documentation
npm run build

# Preview built site
npm run preview
```

### Content Structure

The documentation site includes comprehensive guides covering:

**📚 Core Documentation**
- **Getting Started**: Introduction and setup guides
- **Setup and Configuration**: Installation and configuration details
- **Running Tests**: Testing workflows and coverage reporting
- **NuGet Packaging**: Package creation and publishing
- **Creating Documentation**: Documentation site customization
- **GitHub Workflows**: CI/CD pipeline configuration

**🤖 AI Integration Guides**
- **Agents Markdown**: Working with AI coding agents
- **Cursor Integration**: IDE integration and specialized rules
  - Cursor introduction and setup
  - Documentation rules for AI assistance
  - .NET/C# development rules
  - Web development rules
- **Kiro Integration**: Advanced AI steering and hooks
  - Kiro introduction and setup
  - Documentation steering
  - .NET development steering
  - Web development steering

**🔧 Advanced Topics**
- **NuGet Feeds**: Feed configuration and management
  - NuGet feeds setup and usage
  - Security and authentication (secrets management)
- **Project Documentation**: Comprehensive documentation strategies
  - Creating effective documentation
  - Setting up documentation sites
  - Using documentation with AI tools
- **Scripts Documentation**: Build script usage and customization
  - Test scripts (run, coverage, report)
  - NuGet packaging scripts
  - Setup and configuration scripts
- **GitHub Workflow Integration**: CI/CD pipeline documentation
  - Documentation deployment workflows
  - Test workflow integration

**🏗️ Project Structure**
- **Directory Structure**: Understanding the project layout
- **Configuration**: Detailed configuration guides
- **Best Practices**: Development and deployment patterns

## 🔄 CI/CD and Automation

### GitHub Actions Workflows

- **PR Testing**: `pr-test.yml` - Runs on pull requests
- **GitHub Pages**: `deploy-gh-pages.yml` - Deploys documentation

### Automated Processes

- Tests run automatically on pull requests
- Documentation deploys automatically on main branch pushes
- Dependabot updates dependencies weekly
- Code quality checks run in CI pipeline

### Environment Requirements

- .NET 10 SDK in CI environment
- Docker is available for containerized testing
- Proper permissions for GitHub Pages deployment

## 🎨 Code Style and Standards

### Language Features

- **C# 13**: Latest language version with preview features enabled
- **Nullable Reference Types**: Enabled by default for type safety
- **Implicit Usings**: Enabled for cleaner, more concise code
- **Target Frameworks**: Projects target .NET 9.0; build scripts require .NET 10.0.100-rc.1+
- **Source Generators**: Support for compile-time code generation

### Code Quality Tools

- **StyleCop**: Automated code style enforcement
- **EditorConfig**: Consistent coding standards
- **Code Analysis**: Built-in rule sets and analysis
- **Treat Warnings as Errors**: High quality standards

### Style Guidelines

- Follow C# coding conventions
- Use meaningful names for variables and methods
- Include XML documentation for public APIs
- Ensure proper error handling and validation

## 🚨 Security and Best Practices

### Package Security

- Use environment variables for sensitive configuration
- Never commit API keys or access tokens
- Validate all inputs and handle errors gracefully
- Follow secure coding practices

### Testing Security

- Run tests in isolated environments
- Use containerized testing when possible
- Validate security-related functionality
- Test error conditions and edge cases

## 📝 Commit and PR Guidelines

### Commit Message Format

Follow the **conventional commit format**:

```
type(scope): description

[optional body]

[optional footer]
```

Types: `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`

### Pull Request Process

1. Create a feature branch from the main
2. Make changes following code style guidelines
3. Add/update tests for new functionality
4. Ensure all tests pass and coverage is adequate
5. Update documentation if needed
6. Submit PR with a clear description and linked issues

### Development Workflow

**For New Projects:**
1. Clone the repository
2. Run `dotnet run setup.cs` to configure the project
3. Review and customize the generated configuration files
4. Start development

**Daily Development:**
- Use direct script commands for testing and building
- Run `dotnet run scripts/tests-run.cs` before committing
- Check coverage with `dotnet run scripts/tests-coverage.cs`

### Before Committing

- Run `dotnet build` to ensure solution builds
- Run `dotnet run scripts/tests-run.cs` to verify tests pass
- Check code coverage with `dotnet run scripts/tests-coverage.cs`
- Verify documentation builds correctly (`cd docs && npm run build`)
- Ensure no sensitive information is included in commits

## 🔍 Troubleshooting

### Common Issues

- **Setup Script Issues**: Ensure .NET 10.0.100-rc.1+ is installed and PATH is configured
- **Build Failures**: Check .NET version compatibility (projects: .NET 9.0, scripts: .NET 10.0+)
- **Test Failures**: Verify Docker is running for containerized tests
- **Package Issues**: Check environment variable configuration in nuget.config
- **Documentation Build**: Ensure Node.js dependencies are installed (`cd docs && npm install`)
- **Configuration Errors**: Run `dotnet run setup.cs` to regenerate configuration files

### Getting Help

- Check existing documentation in `docs/` directory
- Review GitHub Actions logs for CI/CD issues
- Consult project issues and discussions
- Refer to .NET and tooling documentation

## 📚 Additional Resources

### Official Documentation

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [NuGet Documentation](https://docs.microsoft.com/en-us/nuget/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Astro.js Starlight Documentation](https://starlight.astro.build/)

### Project-Specific Resources

- [Six.SolutionTemplate Documentation](https://six-tech.github.io/Six.SolutionTemplate)
- [GitHub Repository](https://github.com/six-tech/Six.SolutionTemplate)

---

**Note**: This AGENTS.md file is designed to help AI coding agents work effectively with the Six.SolutionTemplate
repository. It provides comprehensive guidance for development, testing, packaging, and documentation tasks while
maintaining the high-quality standards expected in enterprise .NET development.
