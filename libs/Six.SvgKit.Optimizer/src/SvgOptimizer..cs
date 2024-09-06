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
  /// Removes empty SVG nodes from the document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance.</param>
  /// <returns>The SvgQueryBuilder instance for method chaining.</returns>
  public static SvgQueryBuilder RemoveEmptyNodes(this SvgQueryBuilder builder)
  {
    void RemoveEmptyNodesRecursive(SvgNode node)
    {
      // Iterate through children in reverse order to safely remove while iterating
      for(int i = node.Children.Count - 1; i >= 0; i--)
      {
        var child = node.Children[i];
        RemoveEmptyNodesRecursive(child);

        if(!child.Children.Any())
        {
          node.Children.RemoveAt(i);
        }
      }
    }

    RemoveEmptyNodesRecursive(builder.Ast.Root);
    return builder;
  }


  /// <summary>
  /// Removes empty <c>text</c> and <c>tspan</c> elements, and <c>tref</c> elements that don't reference
  /// another node in the document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance.</param>
  /// <param name="removeText">If to remove empty <c>text</c> elements. Default is <c>true</c></param>
  /// <param name="removeTspan">If to remove empty <c>tspan</c> elements. Default is <c>true</c></param>
  /// <param name="removeTref">If to remove empty <c>tref</c> elements. Default is <c>true</c></param>
  /// <returns>The SvgQueryBuilder instance for method chaining.</returns>
  public static SvgQueryBuilder RemoveEmptyText(
    this SvgQueryBuilder builder,
    bool removeText = true,
    bool removeTspan = true,
    bool removeTref = true
    )
  {
    builder
      .Select(_ => true)
      .Where(n =>
      {
        return n is { Name: "text", Children.Count: 0 }
               || n is { Name: "tspan", Children.Count: 0 }
               || n.Name == "tref" && n.Attributes["href"] == string.Empty;
      }) // This is for xlink:href attribute. Xml parser sees it as href
      .RemoveNodes();

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