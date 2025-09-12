namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class RemoveAllAttributesExtensions
{
  public static SvgQueryBuilder RemoveAllAttributes(this SvgQueryBuilder builder)
  {
    builder
      .Select(_ => true)
      .Transform(n => n.Attributes.Clear());

    return builder;
  }
}