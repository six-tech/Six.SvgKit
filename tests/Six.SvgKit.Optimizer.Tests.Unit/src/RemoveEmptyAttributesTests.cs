using SixSvg = Six.SvgKit.Svg;

namespace Template.Projects.Test.Unit;

public class RemoveEmptyAttributesTests
{
  [Fact]
  public void RemoveEmptyAttributes_RemovesEmptyAttributes()
  {
    var svg = SixSvg.FromString("<svg><rect width='' height='50' fill=''/></svg>");
    var result = svg.Query().RemoveEmptyAttributes().Build();

    var rect = result.ModifiedSvg.Ast.Root.Children.First();
    Assert.Single(rect.Attributes);
    Assert.Equal("50", rect.GetAttribute("height"));
  }

  [Fact]
  public void RemoveEmptyAttributes_KeepsNonEmptyAttributes()
  {
    var svg = SixSvg.FromString("<svg><rect width='100' height='50' fill='red'/></svg>");
    var result = svg.Query().RemoveEmptyAttributes().Build();

    var rect = result.ModifiedSvg.Ast.Root.Children.First();
    Assert.Equal(3, rect.Attributes.Count);
  }
}