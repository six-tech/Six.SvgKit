using JetBrains.Annotations;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class RemoveEmptyTextExtensions
{
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
    var list = builder
      .Select(_ => true)
      .Where(n =>
      {
        bool remove = false;
        if(removeText && n.Name.Equals("text", StringComparison.OrdinalIgnoreCase))
        {
          remove = string.IsNullOrEmpty(n.TextContent);
        }
        
        if(removeTspan && n.Name.Equals("tspan", StringComparison.OrdinalIgnoreCase))
        {
          remove = string.IsNullOrEmpty(n.TextContent);
        }
        
        if(removeTref && n.Name.Equals("tref", StringComparison.OrdinalIgnoreCase))
        {
          remove = !n.HasAttribute("xlink:href") || string.IsNullOrEmpty(n.Attributes["xlink:href"]);
        }

        return remove;
      })
      .ToList();
    
    foreach(var node in list)
    {
      node.Remove();
    }

    return builder;
  }
}