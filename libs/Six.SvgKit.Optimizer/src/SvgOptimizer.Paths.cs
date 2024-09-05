namespace Six.SvgKit.Optimizer;

public static partial class SvgOptimizer
{

  /// <summary>
  /// Merges multiple SVG path elements into a single path element within the SVG document.
  /// </summary>
  /// <remarks><b>This approach has limitations and it's suitable for simple vectors like monochromatic icons</b></remarks>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder MergePaths(this SvgQueryBuilder builder)
  {
    var root = builder.Ast.Root;

    // Collect paths
    var paths = root.Children
      .Where(c => c.Name == "path")
      .Select(p => new { Node = p, Data = p.Attributes["d"], Fill = p.Attributes.GetValueOrDefault("fill") })
      .ToList();

    if(paths.Count > 1)
    {
      // Separate background (fill="none") and foreground paths
      var backgroundPath = paths.FirstOrDefault(p => p.Fill == "none");
      var foregroundPaths = paths.Where(p => p.Fill != "none").ToList();

      // Merge foreground paths
      var mergedPath = new SvgNode(builder.Svg, "path");
      mergedPath.SetAttribute("d", string.Join(" ", foregroundPaths.Select(p => p.Data)));
      mergedPath.SetAttribute("fill-rule", "evenodd");

      // Replace all paths with the background (if exists) and merged foreground path
      root.Children.RemoveAll(c => c.Name == "path");

      if(backgroundPath != null)
      {
        root.AddChild(backgroundPath.Node);
      }

      root.AddChild(mergedPath);
    }

    return builder;
  }

}