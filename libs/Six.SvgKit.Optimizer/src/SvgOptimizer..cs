using JetBrains.Annotations;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static partial class SvgOptimizer
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

  
  /// <summary>
  /// Removes the SVG namespace from the root element (xmlns="http://www.w3.org/2000/svg").
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder RemoveNamespace(this SvgQueryBuilder builder)
  {
    builder.Svg.RemoveNamespace = true;
    return builder;
  }
  
  
  /// <summary>
  /// Compresses the output of a SVG document by removing unnecessary whitespace and line breaks.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder CompressOutput(this SvgQueryBuilder builder)
  {
    builder.Svg.CompressOutput = true;
    return builder;
  }

}