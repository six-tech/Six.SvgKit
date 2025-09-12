# Six.SvgKit.Optimizer

[![NuGet Version](https://img.shields.io/nuget/v/Six.SvgKit.Optimizer.svg)](https://www.nuget.org/packages/Six.SvgKit.Optimizer)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Six.SvgKit.Optimizer.svg)](https://www.nuget.org/packages/Six.SvgKit.Optimizer)

**Six.SvgKit.Optimizer** is an extension library that provides advanced SVG optimization techniques for the Six.SvgKit core library. It offers powerful tools to reduce SVG file sizes while maintaining visual quality through various optimization strategies.

## Features

- **Preset Optimizations**: Ready-made optimization workflows for common scenarios
- **Path Optimization**: Convert shapes to paths, merge paths, and optimize path data
- **Cleanup Operations**: Remove unnecessary elements, attributes, and containers
- **Output Control**: Control compression, XML declarations, and viewBox handling
- **Batch Processing**: Optimized for processing multiple SVG files efficiently
- **Customizable Pipelines**: Create custom optimization workflows for specific needs

## Installation

```bash
# Install both core library and optimizer
dotnet add package Six.SvgKit
dotnet add package Six.SvgKit.Optimizer
```

## Quick Start

### Default Optimization

```csharp
using Six.SvgKit;
using Six.SvgKit.Optimizer;

// Apply comprehensive optimization
var result = Svg.FromFile("input.svg")
    .Query()
    .DefaultOptimizations()
    .Build();

result.ToFile("optimized.svg");

// Check the results
result.SizeComparisonInPercentage(percentage =>
    Console.WriteLine($"File size reduced by {percentage:F1}%"));
```

### Icon Optimization

```csharp
// Specialized optimization for monochromatic icons
var result = Svg.FromFile("icon.svg")
    .Query()
    .MonoIconsOptimizations(width: 24, height: 24, decimalPlaces: 2)
    .Build();

result.ToFile("optimized-icon.svg");
```

### Custom Optimization Pipeline

```csharp
// Create a custom optimization workflow
var result = svg.Query()
    .ConvertShapesToPaths(2)      // Convert shapes to paths
    .MergePaths()                 // Combine similar paths
    .MergePathCommands()          // Optimize path data
    .RoundPathSegments(2)         // Round coordinates
    .RemoveEmptyNodes()           // Clean up empty elements
    .RemoveEmptyAttributes()      // Remove empty attributes
    .CollapseGroups()             // Remove unnecessary groups
    .CleanupAttributes()          // Normalize attributes
    .RemoveXmlDeclaration()       // Remove XML declaration
    .CompressOutput()             // Minify output
    .Build();
```

## Optimization Methods

### Preset Optimizations

#### DefaultOptimizations()
Applies a comprehensive set of safe optimizations suitable for most SVGs:
- Remove XML declaration
- Remove empty nodes, attributes, and text
- Remove empty containers
- Collapse unnecessary groups
- Clean up attribute values
- Compress output

#### MonoIconsOptimizations(width, height, decimalPlaces)
Specialized optimization for monochromatic icon sets:
- Convert shapes to paths
- Merge paths with similar styles
- Round path segments to specified precision
- Remove viewBox attribute
- Set standard icon attributes

### Path Operations

#### ConvertShapesToPaths(precision)
Converts basic shapes (circles, rectangles, ellipses) to path elements:

```csharp
// Convert shapes with 2 decimal places precision
builder.ConvertShapesToPaths(precision: 2);
```

#### MergePaths()
Combines multiple path elements with the same style properties:

```csharp
// Merge paths with identical fill and stroke
builder.MergePaths();
```

#### MergePathCommands()
Optimizes path data by merging consecutive commands of the same type:

```csharp
// Optimize path command sequences
builder.MergePathCommands();
```

#### RoundPathSegments(decimalPlaces)
Rounds path coordinates to reduce file size:

```csharp
// Round to 2 decimal places
builder.RoundPathSegments(2);
```

### Cleanup Operations

#### RemoveEmptyNodes()
Removes elements that have no content or useful attributes:

```csharp
builder.RemoveEmptyNodes();
```

#### RemoveEmptyAttributes()
Removes attributes with empty or whitespace-only values:

```csharp
builder.RemoveEmptyAttributes();
```

#### RemoveEmptyText()
Removes text nodes that contain only whitespace:

```csharp
builder.RemoveEmptyText();
```

#### RemoveEmptyContainers()
Removes container elements (like groups) that have no children:

```csharp
builder.RemoveEmptyContainers();
```

#### CollapseGroups()
Removes unnecessary group elements that don't add semantic value:

```csharp
builder.CollapseGroups();
```

#### CleanupAttributes()
Normalizes and optimizes attribute values (colors, numbers, transforms):

```csharp
builder.CleanupAttributes();
```

### Output Control

#### RemoveXmlDeclaration()
Removes the XML declaration from the output (useful for web use):

```csharp
builder.RemoveXmlDeclaration();
```

#### RemoveViewBoxAttribute()
Removes the viewBox attribute from the root SVG element:

```csharp
builder.RemoveViewBoxAttribute();
```

#### CompressOutput()
Enables output compression to remove unnecessary whitespace:

```csharp
builder.CompressOutput();
```

## Usage Examples

### Web Optimization

```csharp
public static SvgResult OptimizeForWeb(Svg svg)
{
    return svg.Query()
        .DefaultOptimizations()      // Clean up content
        .RemoveXmlDeclaration()     // Not needed in HTML
        .CompressOutput()           // Minimize file size
        .Build();
}
```

### Icon Set Processing

```csharp
public static void OptimizeIconSet(string inputDir, string outputDir)
{
    var iconFiles = Directory.GetFiles(inputDir, "*.svg");
    
    Parallel.ForEach(iconFiles, file =>
    {
        var result = Svg.FromFile(file)
            .Query()
            .MonoIconsOptimizations(24, 24, 1)
            .Build();
        
        var outputPath = Path.Combine(outputDir, Path.GetFileName(file));
        result.ToFile(outputPath);
    });
}
```

### Progressive Optimization

```csharp
// Apply optimizations step by step
var svg = Svg.FromFile("complex.svg");

var step1 = svg.Query().RemoveEmptyNodes().Build();
var step2 = step1.ModifiedSvg.Query().ConvertShapesToPaths().Build();
var step3 = step2.ModifiedSvg.Query().MergePaths().Build();
var final = step3.ModifiedSvg.Query().CompressOutput().Build();

final.ToFile("fully-optimized.svg");
```

### Conditional Optimization

```csharp
public static SvgResult SmartOptimize(Svg svg)
{
    var builder = svg.Query();
    var fileSize = svg.ToSvg().Length;
    
    if (fileSize < 1000)
    {
        // Light optimization for small files
        builder.RemoveEmptyAttributes().CompressOutput();
    }
    else
    {
        // Full optimization for larger files
        builder.DefaultOptimizations();
    }
    
    return builder.Build();
}
```

## Performance Tips

- **Use parallel processing** for batch operations
- **Process files in chunks** for memory management
- **Choose appropriate precision** for coordinate rounding
- **Use preset optimizations** for common scenarios
- **Test visual quality** after aggressive optimizations

## Dependencies

- **Six.SvgKit**: Core SVG processing library (≥ 1.0.0)
- **Svg**: SVG rendering support (≥ 3.4.7)

## Requirements

- **.NET 9.0 or later**: Targets .NET 9.0 for optimal performance
- **Six.SvgKit**: Core library dependency

## Documentation

For complete documentation, optimization guides, and advanced examples, visit:
**[six-tech.github.io/Six.SvgKit/optimization](https://six-tech.github.io/Six.SvgKit/svgkit/optimization)**

## Related Packages

- **[Six.SvgKit](../Six.SvgKit)**: Core SVG processing library

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.