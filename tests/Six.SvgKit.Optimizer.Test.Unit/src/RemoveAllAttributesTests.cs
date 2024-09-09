using SixSvg = Six.SvgKit.Svg;

namespace Template.Projects.Test.Unit;

public class RemoveAllAttributesTests
{
  [Fact]
  public void RemoveAllAttributes_RemovesAllAttributes()
  {
    var svg = SixSvg.FromString("<svg><rect width='100' height='50' fill='red'/></svg>");
    var result = svg.Query().RemoveAllAttributes().Build();

    var rect = result.ModifiedSvg.Ast.Root.Children.First();
    Assert.Empty(rect.Attributes);
  }
}