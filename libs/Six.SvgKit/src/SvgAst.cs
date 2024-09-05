using System.Xml.Linq;
using JetBrains.Annotations;

namespace Six.SvgKit;

[PublicAPI]
public class SvgAst
{
  // MEMBERS
  internal readonly XDocument XDoc;

  #region PROPERTIES
  
  /// <summary>
  /// Gets the SVG object associated with this instance.
  /// </summary>
  public Svg Svg { get; }

  /// <summary>
  /// Gets the root node of the SVG abstract syntax tree (AST).
  /// </summary>
  public SvgNode Root { get; }

  #endregion
  
  
  // CTOR / DISPOSAL
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Initializes a new instance of the <see cref="SvgAst"/> class using an SVG object and an XDocument.
  /// </summary>
  /// <param name="svg">The SVG object.</param>
  /// <param name="svgDoc">The XDocument representing the SVG document.</param>
  /// <exception cref="ArgumentException">Thrown when the SVG document or its root is null.</exception>
  public SvgAst(Svg svg, XDocument svgDoc)
  {
    if(svgDoc is null || svgDoc.Root is null)
    {
      throw new ArgumentException("SVG document is null reference", nameof(svgDoc));
    }

    Svg = svg;
    XDoc = svgDoc;
    Root = BuildAst(Svg, XDoc.Root);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SvgAst"/> class using an SVG object and an SVG string.
  /// </summary>
  /// <param name="svg">The SVG object.</param>
  /// <param name="svgString">The string representation of the SVG document.</param>
  /// <exception cref="ArgumentException">Thrown when the SVG document or its root is null.</exception>
  public SvgAst(Svg svg, string svgString)
  {
    XDoc = XDocument.Parse(svgString);

    if(XDoc is null || XDoc.Root is null)
    {
      throw new ArgumentException("SVG document is null reference", nameof(XDoc));
    }

    Svg = svg;
    Root = BuildAst(Svg, XDoc.Root);
  }


  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Builds the abstract syntax tree (AST) for the SVG document.
  /// </summary>
  /// <param name="svg">The SVG object.</param>
  /// <param name="element">The root XElement of the SVG document.</param>
  /// <returns>The root node of the SVG AST.</returns>
  SvgNode BuildAst(Svg svg, XElement element)
  {
    SvgNode node = new(svg, element.Name.LocalName);

    foreach(var attribute in element.Attributes())
    {
      node.SetAttribute(attribute.Name.LocalName, attribute.Value);
    }

    foreach(var childElement in element.Elements())
    {
      node.AddChild(BuildAst(svg, childElement));
    }

    return node;
  }
}