namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class RemoveEmptyAttributesExtensions
{
  /// <summary>
  /// Remove empty attributes from elements in the document.
  /// </summary>
  /// <remarks>
  /// Ignores the following attributes, as they're used for conditional rendering:
  /// <c>requiredFeatures</c>, <c>requiredExtensions</c>, <c>systemLanguage</c>
  /// </remarks>
  /// <param name="builder"></param>
  /// <returns></returns>
  public static SvgQueryBuilder RemoveEmptyAttributes(this SvgQueryBuilder builder)
  {
    builder
      .Select(_ => true)
      .SelectAttributes(
        _ => true,
        (node, key, value) =>
        {
          string[] ignored = ["requiredFeatures", "requiredExtensions", "systemLanguage"];

          if(string.IsNullOrEmpty(value) && !ignored.Contains(key))
          {
            node.RemoveAttribute(key);
          }
        });

    return builder;
  }
}