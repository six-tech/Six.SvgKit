using System.Text.RegularExpressions;


namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class MergePathCommandsExtensions
{
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
          node.SetAttribute("d", Regex.Replace(pathData, @"([MLHVZ])\s*(?=[0-9\-])", "$1"));
        }
      });

    return builder;
  }
  
}