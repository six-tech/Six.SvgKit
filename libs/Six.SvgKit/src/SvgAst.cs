using AngleSharp;
using AngleSharp.Dom;
using JetBrains.Annotations;

namespace Six.SvgKit;

[PublicAPI]
public class SvgAst
{
  // MEMBERS
  internal readonly IDocument Document; // AngleSharp document

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
  /// Initializes a new instance of the <see cref="SvgAst"/> class using an SVG object and an SVG string.
  /// </summary>
  /// <param name="svg">The SVG object.</param>
  /// <param name="svgString">The string representation of the SVG document.</param>
  /// <exception cref="ArgumentException">Thrown when the SVG document or its root is null.</exception>
  public SvgAst(Svg svg, string svgString)
  {
    // Use AngleSharp to parse the SVG string
    var config = Configuration.Default.WithXml();
    var context = BrowsingContext.New(config);
    
    Document = context.OpenAsync(res => res.Content(svgString).Header("Content-Type", "text/xml")).Result;
    //Document = context.OpenAsync(req => req.Content(svgString)).Result;

    Svg = svg;
    Root = BuildAst(Svg, Document.DocumentElement); // Build AST from AngleSharp document
  }


  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Builds the abstract syntax tree (AST) for the SVG document.
  /// </summary>
  /// <param name="svg">The SVG object.</param>
  /// <param name="element">The root XElement of the SVG document.</param>
  /// <returns>The root node of the SVG AST.</returns>
  SvgNode BuildAst(Svg svg, IElement element)
  {
    var node = new SvgNode(svg, element.TagName.ToLowerInvariant()); // Use lowercase tag names

    foreach (var attribute in element.Attributes)
    {
      node.SetAttribute(attribute.Name, attribute.Value); 
    }

    foreach (var childElement in element.Children)
    {
      node.AddChild(BuildAst(svg, childElement));
    }

    return node;
  }
}