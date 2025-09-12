using SixSvg = Six.SvgKit.Svg;

namespace Template.Projects.Test.Unit;

public class MergePathCommandsTests
{
  [Fact]
  public void MergePathCommands_MergesConsecutiveCommands()
  {
    var svg = SixSvg.FromString("<svg><path d='M 10 10 L 20 20 L 30 30'/></svg>");
    var result = svg.Query().MergePathCommands().Build();

    var path = result.ModifiedSvg.Ast.Root.Children.First();
    Assert.Equal("M10 10 L20 20 L30 30", path.GetAttribute("d"));
  }

  [Fact]
  public void MergePathCommands_DoesNotMergeNonConsecutiveCommands()
  {
    var svg = SixSvg.FromString("<svg><path d='M 10 10 L 20 20 M 30 30'/></svg>");
    var result = svg.Query().MergePathCommands().Build();

    var path = result.ModifiedSvg.Ast.Root.Children.First();
    Assert.Equal("M10 10 L20 20 M30 30", path.GetAttribute("d"));
  }
}