using System.Text.RegularExpressions;

namespace Six.SvgKit.Optimizer;

public static partial class SvgOptimizer
{

  /// <summary>
  /// Merges multiple SVG path elements into a single path element within the SVG document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder MergePaths(this SvgQueryBuilder builder)
  {
    var paths = builder
      .Select(n => n.Name == "path")
      .ToList();
    
    SvgPathHelpers.MergePaths(paths);

    return builder.Continue();
  }
  
  public static SvgQueryBuilder RoundPathSegments(this SvgQueryBuilder builder, int decimalPlaces = 3)
  {
    var pathNodes = builder
      .Select(n => n.Name == "path")
      .ToList();
    
    SvgPathHelpers.RoundPathSegments(pathNodes, decimalPlaces);

    return builder;
  }

  
  /// <summary>
  /// Merges consecutive path commands of the same type into a single command.
  /// For example, "M 10,10 L 20,20 L 30,30" becomes "M 10,10 L 20,20 30,30".
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance.</param>
  /// <returns>The SvgQueryBuilder instance for method chaining.</returns>
  public static SvgQueryBuilder MergePathCommands(this SvgQueryBuilder builder)
  {
    builder.Select(n => n.Name == "path")
      .Transform(node =>
      {
        if (node.Attributes.TryGetValue("d", out var pathData))
        {
          node.SetAttribute("d", MergePathDataCommands(pathData));
        }
      });

    return builder;
  }
  
  
  // PRIVATE
  // ----------------------------------------------------------------------------------------------------------
  
  // Helper function to merge path commands in path data
  static string MergePathDataCommands(string pathData)
  {
    return Regex.Replace(pathData, @"([MLHVZ])\s*(?=[0-9\-])", "$1");
  }
}