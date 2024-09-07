namespace Six.SvgKit.Optimizer;

public static partial class SvgOptimizer
{
  /// <summary>
  /// Removes the viewBox attribute from the SVG document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder RemoveViewBoxAttribute(this SvgQueryBuilder builder)
  {
    builder
      .Select(n => n.Name == "svg")
      .Transform(n =>
      {
        n.RemoveAttribute("viewBox");
      });
    
    return builder.Continue();
  }

  /// <summary>
  /// Removes all whitespace from attribute values in the document.
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  public static SvgQueryBuilder CleanupAttributes(this SvgQueryBuilder builder)
  {
    string[] ignored = ["viewBox", "path"];
    
    builder
      .Select(_ => true)
      .SelectAttributes(
        _ => true,
        (node, key, value) =>
        {
          if(ignored.Contains(key) == false)
          {
            node.SetAttribute(key, value.Trim()); // .Replace(" ", "")); <-- BUG! Breaks the SVG 
          }
        });
    
    return builder;
  }

  /// <summary>
  /// Remove empty attributes from elements in the document.
  /// </summary>
  /// <remarks>
  /// Ignores the following attributes, as they're used for conditional rendering:
  /// <c>requiredFeatures</c>, <c>requiredExtensions</c>, <c>systemLanguage</c>
  /// </remarks>
  /// <param name="builder"></param>
  /// <returns></returns>
  public static SvgQueryBuilder RemoveEmptyAttributes(this SvgQueryBuilder builder)
  {
    builder
      .Select(_ => true)
      .SelectAttributes(
        _ => true,
        (node, key, value) =>
        {
          string[] ignored = ["requiredFeatures", "requiredExtensions", "systemLanguage"];

          if(string.IsNullOrEmpty(value) && !ignored.Contains(key))
          {
            node.RemoveAttribute(key);
          }
        });

    return builder;
  }
}