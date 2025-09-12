# Requirements Document

## Introduction

The Cursor integration documentation in the Six.SolutionTemplate needs to be completely updated to reflect the current state of the Cursor rules system. The documentation is currently outdated and refers to an old structure that no longer exists. The rules have been reorganized into a comprehensive, hierarchical structure with many new rules added.

## Requirements

### Requirement 1: Update Documentation Structure

**User Story:** As a developer using Six.SolutionTemplate, I want the Cursor integration documentation to accurately reflect the current rules structure, so that I can understand and effectively use all available rules.

#### Acceptance Criteria

1. WHEN I read the Cursor introduction page THEN it SHALL accurately describe the current `.cursor/rules/` directory structure
2. WHEN I view the documentation THEN it SHALL show the hierarchical organization (dotnet, web, specs-generation, documentation)
3. WHEN I look at the rule categories THEN it SHALL include all current subdirectories and their purposes
4. WHEN I read about file formats THEN it SHALL correctly reference `.mdc` files instead of `.md` files

### Requirement 2: Comprehensive Rule Coverage

**User Story:** As a developer, I want documentation for all available Cursor rules, so that I can choose the most appropriate rules for my development tasks.

#### Acceptance Criteria

1. WHEN I browse the documentation THEN it SHALL include pages for all major rule categories
2. WHEN I view a rule category THEN it SHALL list all rules within that category with descriptions
3. WHEN I read about .NET rules THEN it SHALL cover ASP.NET, Avalonia, Blazor, CI/CD, C#, and general .NET rules
4. WHEN I look at web rules THEN it SHALL include Astro and TypeScript rules
5. WHEN I check specialized rules THEN it SHALL document specs-generation and documentation rules

### Requirement 3: Updated Navigation Structure

**User Story:** As a user of the documentation, I want an intuitive navigation structure that reflects the current organization, so that I can quickly find the rules I need.

#### Acceptance Criteria

1. WHEN I view the sidebar THEN it SHALL organize rules by technology and purpose
2. WHEN I navigate the Cursor section THEN it SHALL have clear subcategories for different rule types
3. WHEN I look for specific technology rules THEN they SHALL be grouped logically (e.g., all .NET rules together)
4. WHEN I want to understand rule usage THEN there SHALL be clear guidance on how to reference and use rules

### Requirement 4: Accurate Rule Usage Examples

**User Story:** As a developer learning to use Cursor rules, I want accurate examples of how to reference and use rules, so that I can effectively leverage AI assistance in my development work.

#### Acceptance Criteria

1. WHEN I read usage examples THEN they SHALL use the correct rule file names and paths
2. WHEN I see rule references THEN they SHALL follow the current naming conventions
3. WHEN I look at practical examples THEN they SHALL demonstrate real-world usage scenarios
4. WHEN I read about rule combinations THEN it SHALL show how to use multiple rules together effectively

### Requirement 5: Rule Organization Documentation

**User Story:** As a team lead or developer, I want to understand the organization and purpose of different rule categories, so that I can guide my team in using the most appropriate rules for different development scenarios.

#### Acceptance Criteria

1. WHEN I read about rule organization THEN it SHALL explain the purpose of each major category
2. WHEN I look at technology-specific rules THEN it SHALL clarify when to use each category
3. WHEN I review rule hierarchies THEN it SHALL explain the relationship between general and specific rules
4. WHEN I want to extend rules THEN it SHALL provide guidance on adding new rules to the appropriate categories

### Requirement 6: Migration Guide

**User Story:** As an existing user of the old Cursor rules system, I want guidance on how the new system differs from the old one, so that I can update my workflows and prompts accordingly.

#### Acceptance Criteria

1. WHEN I read the migration information THEN it SHALL explain what changed from the old system
2. WHEN I need to update prompts THEN it SHALL provide examples of old vs new rule references
3. WHEN I want to understand benefits THEN it SHALL explain advantages of the new organization
4. WHEN I have existing workflows THEN it SHALL guide me on updating them for the new structure