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
    builder.RemoveAttribute("viewBox");
    return builder;
  }


}