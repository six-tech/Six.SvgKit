using JetBrains.Annotations;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class RemoveEmptyNodesExtensions
{
  /// <summary>
  /// Removes empty SVG nodes from the document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance.</param>
  /// <returns>The SvgQueryBuilder instance for method chaining.</returns>
  public static SvgQueryBuilder RemoveEmptyNodes(this SvgQueryBuilder builder)
  {
    void RemoveEmptyNodesRecursive(SvgNode node)
    {
      for(int i = node.Children.Count - 1; i >= 0; i--)
      {
        var child = node.Children[i];
        RemoveEmptyNodesRecursive(child);

        if((child.IsContainer && !child.Children.Any()) ||
           child.IsText && string.IsNullOrWhiteSpace(child.TextContent))
        {
          node.Children.RemoveAt(i);
        }
      }
    }

    RemoveEmptyNodesRecursive(builder.Ast.Root);
    return builder;
  }
}