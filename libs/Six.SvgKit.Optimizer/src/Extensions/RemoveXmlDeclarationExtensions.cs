namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class RemoveXmlDeclarationExtensions
{
  /// <summary>
  /// Removes the XML declaration from the SVG document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder RemoveXmlDeclaration(this SvgQueryBuilder builder)
  {
    builder.Svg.OmmitXmlDeclaration = true;
    return builder;
  }
}