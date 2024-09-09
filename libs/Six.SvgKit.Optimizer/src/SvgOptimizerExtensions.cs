using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
[ExcludeFromCodeCoverage]
public static class SvgOptimizerExtensions
{
  public static SvgQueryBuilder DefaultOptimizations(this SvgQueryBuilder builder)
  {
    builder
      .RemoveXmlDeclaration()
      .RemoveEmptyNodes()
      .RemoveEmptyAttributes()
      .CleanupAttributes()
      .RemoveEmptyText()
      .RemoveEmptyContainers()
      .CollapseGroups();

    return builder;
  }

  public static SvgQueryBuilder MonoIconsOptimizations(
    this SvgQueryBuilder builder,
    int width = 24,
    int height = 24
  )
  {
    builder
      .RemoveAllAttributes()
      .ConvertShapesToPaths(2)
      .MergePaths()
      .MergePathCommands()
      .RoundPathSegments(2) // Round path segments to 2 decimal places (instead of 4)
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