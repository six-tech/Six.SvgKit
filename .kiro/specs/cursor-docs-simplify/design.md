# Design Document

## Overview

This design outlines the simplification of the Cursor integration documentation to create a streamlined, error-free documentation system that focuses on essential information about Cursor rules. The approach will remove build-breaking elements, excessive examples, and complex components while maintaining the core information needed to understand and use the Cursor rules effectively.

## Architecture

### Simplified Documentation Structure

The documentation will keep the existing page structure but simplify the content within each page:

```
docs/src/content/docs/AI/cursor-integration/
├── cursor-introduction.mdx (content simplified)
├── cursor-migration-guide.mdx (content simplified)
├── cursor-usage-patterns.mdx (content simplified)
├── dotnet-rules/
│   ├── index.mdx (content simplified)
│   ├── aspnet-rules.mdx (content simplified)
│   ├── avalonia-rules.mdx (content simplified)
│   ├── blazor-rules.mdx (content simplified)
│   ├── ci-cd-rules.mdx (content simplified)
│   ├── csharp-rules.mdx (content simplified)
│   └── general-dotnet-rules.mdx (content simplified)
├── web-rules/
│   ├── index.mdx (content simplified)
│   ├── astro-rules.mdx (content simplified)
│   └── typescript-rules.mdx (content simplified)
├── specialized-rules/
│   ├── index.mdx (content simplified)
│   ├── specs-generation-rules.mdx (content simplified)
│   └── documentation-rules.mdx (content simplified)
└── rule-management/
    ├── index.mdx (content simplified)
    ├── creating-rules.mdx (content simplified)
    └── best-practices.mdx (content simplified)
```

### Content Simplification Strategy

Keep all existing pages but simplify their content by:

- Removing excessive examples and code blocks
- Removing Tab components and complex layouts
- Focusing on essential rule information (what it does, how it's triggered)
- Using simple markdown lists instead of complex components
- Keeping navigation structure exactly as it is

### Navigation Structure Preservation

The Astro config sidebar will remain exactly the same - no changes to navigation structure.

## Components and Interfaces

### Content Components

#### Simple Rule Listing Component

Replace complex CardGrid components with simple markdown lists:

```markdown
## .NET Rules

### ASP.NET Rules

- **aspnet-api.mdc** - ASP.NET Core API development patterns (Auto-triggered: `*.cs` in API projects)
- **aspnet-api-fast-endpoints.mdc** - Fast Endpoints API patterns (Manual reference)
- **aspnet-orleans.mdc** - Orleans distributed systems (Manual reference)

### Avalonia Rules

- **avalonia-controls.mdc** - Avalonia control development (Auto-triggered: `*.axaml`, `*.cs` in Avalonia projects)
- **avalonia-mvvm.mdc** - MVVM patterns for Avalonia (Manual reference)
- **avalonia-xaml.mdc** - XAML styling and templates (Auto-triggered: `*.axaml`)
```

#### Basic Usage Examples

Replace complex code blocks with simple reference formats:

```markdown
## How to Reference Rules

**Manual reference in prompts:**
```

@.cursor/rules/dotnet/aspnet/aspnet-api.mdc Create a user controller

```

**Automatic triggering:**
Rules automatically apply when working with matching file types in relevant project contexts.
```

### Removed Components

The following will be removed to fix build errors:

- All Tab components
- Complex CardGrid layouts with multiple nested LinkCards
- Aside components with complex content
- Multi-level code examples with syntax highlighting issues
- Any custom components that cause build failures

## Data Models

### Simplified Rule Information Structure

```typescript
interface SimpleRuleInfo {
  name: string; // Display name
  fileName: string; // Actual .mdc file name
  category: string; // dotnet, web, specialized, documentation
  description: string; // One-line description
  trigger: "auto" | "manual" | "both"; // How the rule is applied
  filePatterns?: string[]; // File patterns for auto-trigger (if applicable)
}

interface RuleCategory {
  name: string;
  description: string;
  rules: SimpleRuleInfo[];
}
```

## Error Handling

### Build Error Prevention

1. **Component Validation**: Only use proven Astro/Starlight components
2. **Markdown Validation**: Use standard markdown syntax without complex features
3. **Link Validation**: Ensure all internal links point to existing pages
4. **Syntax Validation**: Avoid problematic code block syntax

### Content Validation Strategy

1. **Remove all Tab components** - Replace with simple sections
2. **Simplify code blocks** - Use basic markdown code blocks without complex syntax
3. **Remove nested components** - Use flat content structure
4. **Validate imports** - Only import essential Starlight components

## Testing Strategy

### Build Testing

1. **Local Build Verification**: Test documentation builds locally before committing
2. **Component Testing**: Verify each page builds independently
3. **Link Testing**: Ensure all internal links work correctly
4. **Syntax Testing**: Validate markdown syntax is error-free

### Content Testing

1. **Readability Testing**: Ensure content is concise and scannable
2. **Accuracy Testing**: Verify rule descriptions match actual rule files
3. **Completeness Testing**: Ensure all important rules are documented
4. **Navigation Testing**: Verify simplified navigation works correctly

## Implementation Phases

### Phase 1: Content Audit and Error Identification

1. Identify all build-breaking elements in current documentation
2. List all Tab components and complex code examples
3. Catalog all existing rules that need to be documented
4. Map current complex structure to simplified structure

### Phase 2: Simplify Page Content

1. Simplify `cursor-introduction.mdx` content - remove excessive examples
2. Simplify all rule category pages - focus on rule descriptions only
3. Remove Tab components and complex code examples from all pages
4. Replace CardGrid components with simple markdown lists
5. Keep all existing pages but with simplified content

### Phase 3: Fix Build Errors

1. Remove all Tab components that cause build failures
2. Simplify problematic code blocks
3. Test documentation build process
4. Fix any remaining build errors without changing navigation structure

### Phase 4: Content Polish and Validation

1. Review all content for conciseness and clarity
2. Ensure rule descriptions are accurate and helpful
3. Validate that essential information is preserved
4. Test complete user workflows through simplified documentation

## Performance Considerations

### Page Load Optimization

- Fewer pages mean faster navigation
- Simpler components reduce JavaScript overhead
- Consolidated content reduces HTTP requests
- Static generation remains fast with fewer complex components

### Maintenance Efficiency

- Fewer files to maintain and update
- Simpler content structure reduces maintenance overhead
- Consolidated rule information in one place
- Easier to keep rule documentation in sync with actual rules

## Content Strategy

### Essential Information Only

Each rule entry will contain:

1. **Rule name** - Clear, descriptive name
2. **File name** - Exact .mdc file name for reference
3. **Purpose** - One sentence describing what the rule does
4. **Trigger type** - Manual, automatic, or both
5. **File patterns** - If auto-triggered, what file types trigger it

### Removed Content Types

The following will be removed:

- Extensive code examples showing implementation details
- Complex usage scenarios and combinations
- Detailed architectural explanations
- Advanced patterns and best practices sections
- Migration guides with before/after examples
- Rule management and creation guidance

### Content Organization Principles

1. **Scannable** - Use bullet points and short paragraphs
2. **Hierarchical** - Organize by technology category
3. **Consistent** - Use same format for all rule descriptions
4. **Minimal** - Include only essential information
5. **Accurate** - Ensure all information matches actual rule files

## Migration Strategy

### From Current Complex Structure

1. **Preserve all navigation** - Keep exact same navigation structure and all pages
2. **Simplify content only** - Remove excessive examples and complex components from within pages
3. **Fix build errors** - Remove Tab components and problematic code blocks
4. **Focus on essentials** - Keep only rule descriptions and basic usage information

### Content Preservation

Essential information to preserve:

- Rule directory structure explanation
- Basic rule reference syntax
- List of all available rules with descriptions
- Automatic vs manual triggering explanation

Information to remove:

- Detailed usage scenarios
- Complex code examples
- Advanced patterns and combinations
- Migration guides from old systems
- Rule creation and management guidance

## Quality Assurance

### Build Quality

- Documentation must build without errors
- All pages must load correctly
- Navigation must work properly
- Links must resolve correctly

### Content Quality

- Rule descriptions must be accurate
- File references must be correct
- Trigger information must be accurate
- Content must be concise and helpful

### User Experience

- Information must be easy to find
- Content must be scannable
- Navigation must be intuitive
- Essential information must be preserved

## Success Metrics

### Technical Success

- Documentation builds without errors
- All pages load correctly
- Navigation works properly
- Build time is reasonable

### Content Success

- All rules are documented
- Descriptions are accurate and helpful
- Users can quickly find rule information
- Essential usage information is preserved

### Maintenance Success

- Content is easy to update
- New rules can be added easily
- Structure supports future growth
- Maintenance overhead is reduced
