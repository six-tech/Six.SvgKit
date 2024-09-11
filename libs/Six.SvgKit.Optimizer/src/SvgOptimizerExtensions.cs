using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
[ExcludeFromCodeCoverage]
public static class SvgOptimizerExtensions
{
  /// <summary>
  /// Applies a default set of SVG optimization techniques to the SvgQueryBuilder.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder used to build and optimize SVG content.</param>
  /// <returns>Returns the SvgQueryBuilder after applying the default optimizations.</returns>
  public static SvgQueryBuilder DefaultOptimizations(this SvgQueryBuilder builder)
  {
    builder
      .RemoveXmlDeclaration()
      .RemoveEmptyNodes()
      .RemoveEmptyAttributes()
      .RemoveEmptyText()
      .RemoveEmptyContainers()
      .CollapseGroups()
      .CleanupAttributes()
      .CompressOutput();

    return builder;
  }

  
  /// <summary>
  /// Applies a series of SVG optimization techniques specifically tailored for monochromatic icon sets.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder used to build and optimize SVG content.</param>
  /// <param name="width">The width to set for the root SVG element. Default is 24.</param>
  /// <param name="height">The height to set for the root SVG element. Default is 24.</param>
  /// <param name="decimalPlaces">The number of decimal places to use for rounding path segments. Default is 2.</param>
  /// <returns>Returns the SvgQueryBuilder after applying the optimizations.</returns>
  public static SvgQueryBuilder MonoIconsOptimizations(
    this SvgQueryBuilder builder,
    int width = 24,
    int height = 24,
    int decimalPlaces = 2 // Round path segments to 2 decimal places (instead of 4)
  )
  {
    builder
      .ConvertShapesToPaths(decimalPlaces)
      .MergePaths()
      .MergePathCommands()
      .RoundPathSegments(decimalPlaces) 
      .DefaultOptimizations()
      .RemoveViewBoxAttribute()
      .CompressOutput()
      .Select(n => n.Name == "svg")
      .Transform(n =>
      {
        n.SetAttribute("width", width.ToString());
        n.SetAttribute("height", height.ToString());
        n.SetAttribute("fill", "none");
      })
      .Continue()
      .Select(n => n.Name == "path")
      .Transform(n =>
      {
        // Set attributes
        // fill-rule="evenodd" clip-rule="evenodd" fill="black"
        n.SetAttribute("fill-rule", "evenodd");
        n.SetAttribute("clip-rule", "evenodd");
        n.SetAttribute("fill", "black");
      });

    return builder;
  }
}