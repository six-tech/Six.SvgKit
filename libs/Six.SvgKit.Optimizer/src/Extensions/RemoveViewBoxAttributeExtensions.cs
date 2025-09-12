namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class RemoveViewBoxAttributeExtensions
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
}