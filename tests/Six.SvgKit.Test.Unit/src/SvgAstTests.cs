using SixSvg = Six.SvgKit.Svg;

namespace SvgKit.Test.Unit;

public class SvgAstTests
{
  [Fact]
  public void Constructor_ValidSvgString_CreatesAstCorrectly()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><circle cx='50' cy='50' r='40'/></svg>");
    var svgString = "<svg><circle cx='50' cy='50' r='40'/></svg>";

    // Act
    var ast = new SvgAst(svg, svgString);

    // Assert
    Assert.NotNull(ast.Root);
    Assert.Equal("svg", ast.Root.Name);
    Assert.Single(ast.Root.Children);
    Assert.Equal("circle", ast.Root.Children[0].Name);
  }

}