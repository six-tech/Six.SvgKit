using SixSvg = Six.SvgKit.Svg;

namespace Six.SvgKit.Optimizer.Test.Unit;

public class RemoveEmptyNodesTests
{
  [Fact]
  public void RemoveEmptyNodes_ShouldRemoveEmptyGNodes()
  {
    // Arrange
    const string svgWithEmptyNodes = @"<svg viewBox=""0 0 10 10"">
                                                <g><rect x=""1"" y=""1"" width=""8"" height=""8"" fill=""red""/></g>
                                                <g></g>
                                                <path d=""M 1 1 L 9 1 L 9 9 L 1 9 Z"" fill=""blue""/>
                                                <g><tspan></tspan></g>
                                            </svg>";
    
    // Act
    var svg = SixSvg.FromString(svgWithEmptyNodes);
    var result = svg
      .Query()
      .RemoveEmptyNodes()
      .Build();

    // Assert
    Assert.Single(result.ModifiedSvg.Ast.Root.Children, n => n.Name == "g");
    Assert.Single(result.ModifiedSvg.Ast.Root.Children, n => n.Name == "path");
  }

  [Fact]
  public void RemoveEmptyNodes_ShouldNotModifySvgWithoutEmptyNodes()
  {
    // Arrange
    const string svgWithoutEmptyNodes = @"<svg viewBox=""0 0 10 10"">
                                                    <rect x=""1"" y=""1"" width=""8"" height=""8"" fill=""red""/>
                                                    <path d=""M 1 1 L 9 1 L 9 9 L 1 9 Z"" fill=""blue""/>
                                                </svg>";
    var svg = SixSvg.FromString(svgWithoutEmptyNodes);
    var builder = svg.Query();

    // Act
    var result = builder.RemoveEmptyNodes().Build();

    // Assert
    Assert.Single(result.ModifiedSvg.Ast.Root.Children, n => n.Name == "rect");
    Assert.Single(result.ModifiedSvg.Ast.Root.Children, n => n.Name == "path");
  }

  [Fact]
  public void RemoveEmptyNodes_ShouldRemoveEmptyNodesRecursively()
  {
    // Arrange
    const string svgWithNestedEmptyNodes = @"<svg viewBox=""0 0 10 10"">
                                                        <g>
                                                            <g>
                                                                <rect x=""1"" y=""1"" width=""8"" height=""8"" fill=""red""/>
                                                            </g>
                                                            <g></g>
                                                        </g>
                                                        <path d=""M 1 1 L 9 1 L 9 9 L 1 9 Z"" fill=""blue""/>
                                                    </svg>";
    var svg = Svg.FromString(svgWithNestedEmptyNodes);
    var builder = svg.Query();

    // Act
    var result = builder.RemoveEmptyNodes().Build();

    // Assert
    Assert.Single(result.ModifiedSvg.Ast.Root.Children, n => n.Name == "g");
    Assert.Single(result.ModifiedSvg.Ast.Root.Children[0].Children[0].Children, n => n.Name == "rect");
    Assert.Single(result.ModifiedSvg.Ast.Root.Children, n => n.Name == "path");
  }
}