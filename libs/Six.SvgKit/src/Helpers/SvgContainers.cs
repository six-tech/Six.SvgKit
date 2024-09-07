namespace Six.SvgKit;

public static class SvgContainers
{
  // A container, as defined in the SVG specifications (https://www.w3.org/TR/SVG11/intro.html#TermContainerElement),
  // is an SVG element that can have graphical child elements. Container elements include:
  static readonly HashSet<string> _names =
  [
    "a",
    "defs",
    "glyph",
    "g",
    "marker",
    "mask",
    "missing-glyph",
    "pattern",
    "svg",
    "switch",
    "symbol"
  ];

  /// <summary>
  /// Determines if the given <see cref="SvgNode"/> is an SVG container element.
  /// </summary>
  /// <param name="node">The <see cref="SvgNode"/> to check.</param>
  /// <returns>
  /// True if the node is a container element as defined by the SVG specifications; otherwise, false.
  /// </returns>
  public static bool IsContainer(SvgNode node)
  {
    return _names.Contains(node.Name);
  }
  
}