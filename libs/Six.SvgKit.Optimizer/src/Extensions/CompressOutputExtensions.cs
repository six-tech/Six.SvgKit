using JetBrains.Annotations;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class CompressOutputExtensions
{
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