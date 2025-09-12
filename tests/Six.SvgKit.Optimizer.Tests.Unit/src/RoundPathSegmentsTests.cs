using SixSvg = Six.SvgKit.Svg;

namespace Template.Projects.Test.Unit;

public class RoundPathSegmentsTests
{
  [Fact]
  public void RoundPathSegments_RoundsPathSegmentValues()
  {
    var svg = SixSvg.FromString("<svg><path d='M 1.234 5.678 L 9.012 3.456'/></svg>");
    var result = svg.Query().RoundPathSegments(2).Build();

    var path = result.ModifiedSvg.Ast.Root.Children.First();
    Assert.Equal("M 1.23 5.68 L 9.01 3.46", path.GetAttribute("d"));
  }

  [Fact]
  public void RoundPathSegments_HandlesNonNumericValues()
  {
    var svg = SixSvg.FromString("<svg><path d='M 1.234 5.678 Z'/></svg>");
    var result = svg.Query().RoundPathSegments(2).Build();

    var path = result.ModifiedSvg.Ast.Root.Children.First();
    Assert.Equal("M 1.23 5.68 Z", path.GetAttribute("d"));
  }
}