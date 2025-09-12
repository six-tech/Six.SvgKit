# Design Document

## Overview

This design outlines the comprehensive update of the Cursor integration documentation to reflect the current state of the rules system. The update will transform the outdated documentation into an accurate, comprehensive guide that properly represents the hierarchical organization and expanded coverage of the current Cursor rules.

## Architecture

### Documentation Structure Redesign

The documentation will be restructured to mirror the actual `.cursor/rules/` directory organization:

```
docs/src/content/docs/AI/cursor-integration/
├── cursor-introduction.mdx (updated)
├── cursor-migration-guide.mdx (new)
├── cursor-usage-patterns.mdx (new)
├── dotnet-rules/
│   ├── index.mdx
│   ├── aspnet-rules.mdx
│   ├── avalonia-rules.mdx
│   ├── blazor-rules.mdx
│   ├── ci-cd-rules.mdx
│   ├── csharp-rules.mdx
│   └── general-dotnet-rules.mdx
├── web-rules/
│   ├── index.mdx
│   ├── astro-rules.mdx
│   └── typescript-rules.mdx
├── specialized-rules/
│   ├── index.mdx
│   ├── specs-generation-rules.mdx
│   └── documentation-rules.mdx
└── rule-management/
    ├── index.mdx
    ├── creating-rules.mdx
    └── best-practices.mdx
```

### Navigation Structure

The Astro config sidebar will be updated to reflect the new organization:

```javascript
{
  label: 'Cursor',
  items: [
    { label: 'Introduction', slug: 'ai/cursor-integration/cursor-introduction' },
    { label: 'Migration Guide', slug: 'ai/cursor-integration/cursor-migration-guide' },
    { label: 'Usage Patterns', slug: 'ai/cursor-integration/cursor-usage-patterns' },
    {
      label: '.NET Rules',
      items: [
        { label: 'Overview', slug: 'ai/cursor-integration/dotnet-rules/' },
        { label: 'ASP.NET Rules', slug: 'ai/cursor-integration/dotnet-rules/aspnet-rules' },
        { label: 'Avalonia Rules', slug: 'ai/cursor-integration/dotnet-rules/avalonia-rules' },
        { label: 'Blazor Rules', slug: 'ai/cursor-integration/dotnet-rules/blazor-rules' },
        { label: 'CI/CD Rules', slug: 'ai/cursor-integration/dotnet-rules/ci-cd-rules' },
        { label: 'C# Rules', slug: 'ai/cursor-integration/dotnet-rules/csharp-rules' },
        { label: 'General .NET Rules', slug: 'ai/cursor-integration/dotnet-rules/general-dotnet-rules' }
      ]
    },
    {
      label: 'Web Rules',
      items: [
        { label: 'Overview', slug: 'ai/cursor-integration/web-rules/' },
        { label: 'Astro Rules', slug: 'ai/cursor-integration/web-rules/astro-rules' },
        { label: 'TypeScript Rules', slug: 'ai/cursor-integration/web-rules/typescript-rules' }
      ]
    },
    {
      label: 'Specialized Rules',
      items: [
        { label: 'Overview', slug: 'ai/cursor-integration/specialized-rules/' },
        { label: 'Specs Generation', slug: 'ai/cursor-integration/specialized-rules/specs-generation-rules' },
        { label: 'Documentation', slug: 'ai/cursor-integration/specialized-rules/documentation-rules' }
      ]
    },
    {
      label: 'Rule Management',
      items: [
        { label: 'Overview', slug: 'ai/cursor-integration/rule-management/' },
        { label: 'Creating Rules', slug: 'ai/cursor-integration/rule-management/creating-rules' },
        { label: 'Best Practices', slug: 'ai/cursor-integration/rule-management/best-practices' }
      ]
    }
  ]
}
```

## Components and Interfaces

### Page Templates

#### Category Overview Pages
Each major category (dotnet-rules, web-rules, specialized-rules) will have an overview page that:
- Explains the purpose and scope of the category
- Lists all rules in the category with brief descriptions
- Provides usage guidance for the category
- Shows common patterns and combinations

#### Individual Rule Pages
Each rule category page will include:
- Rule purpose and scope
- List of specific rule files with descriptions
- Usage examples with correct file references
- Common use cases and scenarios
- Integration with other rules

#### Usage Pattern Pages
Dedicated pages for:
- How to reference rules in prompts
- Combining multiple rules effectively
- Rule precedence and conflicts
- Best practices for rule usage

### Content Components

#### Rule Reference Component
A reusable component for displaying rule information:
```astro
---
interface Props {
  ruleName: string;
  filePath: string;
  description: string;
  usageExample: string;
  relatedRules?: string[];
}
---
```

#### Usage Example Component
For showing before/after examples of rule usage:
```astro
---
interface Props {
  title: string;
  oldWay?: string;
  newWay: string;
  explanation: string;
}
---
```

## Data Models

### Rule Information Structure
```typescript
interface RuleInfo {
  name: string;
  filePath: string;
  category: 'dotnet' | 'web' | 'specialized' | 'documentation';
  subcategory?: string;
  description: string;
  purpose: string;
  usageExamples: string[];
  relatedRules: string[];
  filePattern?: string;
  alwaysApply?: boolean;
}

interface RuleCategory {
  name: string;
  path: string;
  description: string;
  rules: RuleInfo[];
  subcategories?: RuleCategory[];
}
```

### Migration Mapping
```typescript
interface MigrationMapping {
  oldReference: string;
  newReference: string;
  explanation: string;
  category: string;
}
```

## Error Handling

### Broken Links Prevention
- Implement link validation for all rule references
- Use TypeScript interfaces to ensure consistency
- Add automated checks for rule file existence

### Content Validation
- Validate that all documented rules actually exist
- Check that usage examples use correct file paths
- Ensure navigation structure matches actual content

## Testing Strategy

### Content Accuracy Testing
1. **Rule Existence Verification**: Automated tests to verify all documented rules exist in the actual `.cursor/rules/` directory
2. **Link Validation**: Tests to ensure all internal links work correctly
3. **Example Validation**: Verification that usage examples reference correct file paths

### Navigation Testing
1. **Sidebar Structure**: Tests to ensure navigation matches the actual content structure
2. **Breadcrumb Accuracy**: Verification that page hierarchies are correct
3. **Cross-references**: Testing that related rule links work properly

### Migration Guide Testing
1. **Example Accuracy**: Verification that old vs new examples are correct
2. **Completeness**: Ensure all major changes are documented
3. **Clarity**: Review that migration instructions are clear and actionable

## Implementation Phases

### Phase 1: Content Audit and Structure
1. Audit current `.cursor/rules/` directory structure
2. Create comprehensive inventory of all rules
3. Map old documentation to new structure
4. Identify gaps and new content needed

### Phase 2: Core Page Updates
1. Update `cursor-introduction.mdx` with current structure
2. Create migration guide for users of old system
3. Update navigation structure in `astro.config.mjs`
4. Create category overview pages

### Phase 3: Detailed Rule Documentation
1. Create individual pages for each rule category
2. Document all rules with proper descriptions and examples
3. Add usage patterns and best practices
4. Create cross-references between related rules

### Phase 4: Advanced Features
1. Add rule management documentation
2. Create best practices guide
3. Add troubleshooting and FAQ sections
4. Implement content validation tools

## Performance Considerations

### Page Load Optimization
- Use Astro's static generation for all documentation pages
- Implement proper image optimization for any diagrams
- Minimize JavaScript usage for better performance

### Navigation Efficiency
- Organize content hierarchically to reduce cognitive load
- Use clear, descriptive page titles and URLs
- Implement search functionality if needed

### Content Maintenance
- Design content structure for easy updates
- Use consistent templates for similar content types
- Implement automated validation where possible

## Security Considerations

### Content Safety
- Ensure no sensitive information is included in examples
- Use placeholder values for any configuration examples
- Review all code examples for security best practices

### Link Security
- Use relative links for internal navigation
- Ensure external links use appropriate security attributes
- Validate that all examples follow security guidelines

## Accessibility Standards

### Content Accessibility
- Use proper heading hierarchy (h1, h2, h3, etc.)
- Provide alt text for any images or diagrams
- Ensure sufficient color contrast for all text

### Navigation Accessibility
- Implement proper ARIA labels for navigation elements
- Ensure keyboard navigation works correctly
- Provide skip links where appropriate

### Code Example Accessibility
- Use semantic markup for code examples
- Provide clear context for all examples
- Ensure code examples are readable with screen readers