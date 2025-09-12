namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class RemoveEmptyContainersExtensions
{
  /// <summary>
  /// Remove container elements in the document that have no children or meaningful attributes,
  /// excluding the <c>&lt;svg&gt;</c> element which is ignored.
  /// </summary>
  /// <param name="builder">The <see cref="SvgQueryBuilder"/> instance to operate on.</param>
  /// <returns>The modified <see cref="SvgQueryBuilder"/> instance.</returns>
  public static SvgQueryBuilder RemoveEmptyContainers(this SvgQueryBuilder builder)
  {
    RemoveEmptyContainersInternal(builder.Ast.Root);
    return builder;
  }
  
  
  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
  static void RemoveEmptyContainersInternal(SvgNode node)
  {
    node.Children.RemoveAll(child =>
      child.IsContainer && !child.Attributes.Any() && !child.Children.Any());

    foreach(var child in node.Children)
    {
      RemoveEmptyContainersInternal(child);
    }
  }
  
}