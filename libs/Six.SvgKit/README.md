# Six.SvgKit

[![NuGet Version](https://img.shields.io/nuget/v/Six.SvgKit.svg)](https://www.nuget.org/packages/Six.SvgKit)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Six.SvgKit.svg)](https://www.nuget.org/packages/Six.SvgKit)

**Six.SvgKit** is the core library for parsing, manipulating, and working with SVG (Scalable Vector Graphics) files in .NET applications. It provides a fluent API for SVG document processing with a focus on performance and ease of use.

## Features

- **SVG Parsing**: Load SVG files from strings or file paths
- **Document Manipulation**: Modify SVG elements, attributes, and structure
- **Fluent Query API**: Chain operations for complex SVG transformations
- **Format Conversion**: Convert SVG to bitmap formats (requires ImageMagick)
- **Metadata Support**: Track SVG instances during batch processing
- **Output Control**: Configure compression and XML declaration handling

## Installation

```bash
dotnet add package Six.SvgKit
```

## Quick Start

### Loading SVG Files

```csharp
using Six.SvgKit;

// Load from file
var svg = Svg.FromFile("icon.svg");

// Load from string
var svgString = @"<svg xmlns=""http://www.w3.org/2000/svg"" width=""24"" height=""24"">
    <circle cx=""12"" cy=""12"" r=""10"" fill=""blue"" />
</svg>";
var svg = Svg.FromString(svgString);
```

### Basic Manipulation

```csharp
// Change fill color of all path elements
var result = svg.Query()
    .Select(node => node.Name == "path")
    .Transform(node => node.SetAttribute("fill", "red"))
    .Build();

// Save the modified SVG
result.ToFile("red-paths.svg");
```

### Output Options

```csharp
// Control output formatting
svg.CompressOutput = true;        // Remove whitespace
svg.OmmitXmlDeclaration = true;   // Remove XML declaration

// Get SVG as string or bytes
string svgString = svg.ToSvg();
byte[] svgBytes = svg.ToBytes();

// Save to file
svg.ToSvgFile("output.svg");
```

## Core Classes

### Svg Class

Main entry point for working with SVG documents.

**Factory Methods:**
- `Svg.FromFile(string path)`: Load SVG from file
- `Svg.FromString(string svgString)`: Load SVG from string

**Instance Methods:**
- `Query()`: Create a new SvgQueryBuilder
- `Clone()`: Create a deep copy of the SVG
- `ToSvg()`: Convert to string representation
- `ToBytes()`: Convert to byte array
- `ToSvgFile(string path)`: Save to file

### SvgQueryBuilder Class

Fluent API for querying and transforming SVG elements.

**Selection Methods:**
- `Select(Func<SvgNode, bool> predicate)`: Select nodes matching criteria
- `SelectAttributes(Func<string, bool> keyPredicate, Action<SvgNode, string, string> action)`: Process attributes

**Transformation Methods:**
- `Transform(Action<SvgNode> action)`: Apply transformations to selected nodes
- `Continue()`: Continue query chain after modifications

**Execution:**
- `Build()`: Execute query and return SvgResult

### SvgNode Class

Represents individual elements in the SVG document tree.

**Properties:**
- `Name`: Element name (e.g., "svg", "path", "circle")
- `Attributes`: Dictionary of element attributes
- `Children`: List of child nodes
- `Parent`: Parent node reference
- `TextContent`: Text content of the node

**Methods:**
- `SetAttribute(string key, string value)`: Set attribute value
- `RemoveAttribute(string key)`: Remove attribute
- `Remove()`: Remove node from parent

### SvgResult Class

Contains the results of SVG operations.

**Properties:**
- `OriginalSvg`: SVG before modifications
- `ModifiedSvg`: SVG after modifications

**Methods:**
- `SizeComparisonInBytes(Action<int, int> callback)`: Compare file sizes
- `SizeComparisonInPercentage(Action<double> callback)`: Calculate size reduction
- `ToFile(string path)`: Save modified SVG to file

## Format Conversion

Convert SVG to bitmap formats using ImageMagick integration:

```csharp
using ImageMagick;

var svg = Svg.FromFile("icon.svg");

// Convert to PNG
svg.ToBitmapFile(MagickFormat.Png24, "icon.png", 256, 256);

// Convert to WebP
svg.ToBitmapFile(MagickFormat.WebP, "icon.webp", 128, 128);

// Convert to JPEG
svg.ToBitmapFile(MagickFormat.Jpeg, "icon.jpg", 64, 64);
```

## Advanced Usage

### Working with Metadata

```csharp
var metadata = new SvgMetadata();
var svg = Svg.FromFile("icon.svg")
    .WithMetadata(metadata);

// Metadata is preserved when cloning
var clonedSvg = svg.Clone();
```

### Complex Queries

```csharp
// Select and transform multiple element types
var result = svg.Query()
    .Select(n => n.Name == "circle" || n.Name == "rect")
    .Transform(n => {
        n.SetAttribute("stroke", "black");
        n.SetAttribute("stroke-width", "1");
    })
    .Continue()
    .Select(n => n.Name == "g" && !n.Children.Any())
    .Transform(n => n.Remove())
    .Build();
```

### Attribute Processing

```csharp
// Process specific attributes
var result = svg.Query()
    .SelectAttributes(
        key => key.StartsWith("data-"),
        (node, key, value) => node.RemoveAttribute(key)
    )
    .Build();
```

## Dependencies

- **AngleSharp**: Fast XML/HTML parsing (≥ 1.3.0)
- **AngleSharp.Xml**: XML parsing support (≥ 1.0.0)
- **Magick.NET-Q8-AnyCPU**: ImageMagick integration (≥ 14.8.2)
- **Svg**: SVG rendering support (≥ 3.4.7)

## Requirements

- **.NET 9.0 or later**: Targets .NET 9.0 for optimal performance
- **C# 13**: Uses latest language features

## Related Packages

- **[Six.SvgKit.Optimizer](../Six.SvgKit.Optimizer)**: Advanced optimization extensions

## Documentation

For complete documentation, examples, and advanced usage patterns, visit:
**[six-tech.github.io/Six.SvgKit](https://six-tech.github.io/Six.SvgKit)**

## License

This project is licensed under the MIT License - see the [LICENSE](../../LICENSE) file for details.