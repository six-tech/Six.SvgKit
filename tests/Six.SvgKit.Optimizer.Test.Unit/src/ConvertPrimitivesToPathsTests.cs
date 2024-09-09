using SixSvg = Six.SvgKit.Svg;

namespace Template.Projects.Test.Unit;

public class ConvertPrimitivesToPathsTests
{
  [Fact]
  public void ConvertCirclesToPaths_SimpleCircle_ConvertsToPath()
  {
    // Arrange
    string svgString = @"<svg viewBox='0 0 100 100'>
                                    <circle cx='50' cy='50' r='25' fill='red' />
                                  </svg>";
    var svg = SixSvg.FromString(svgString);

    // Act
    var result = svg.Query()
      .ConvertShapesToPaths()
      .Build();

    // Assert
    var pathNode = result.ModifiedSvg.Ast.Root.Children.FirstOrDefault(n => n.Name == "path");

    Assert.NotNull(pathNode);
    Assert.Equal("path", pathNode.Name);
    Assert.Contains("M25", pathNode.GetAttribute("d"));
    Assert.Contains("A25", pathNode.GetAttribute("d"));
    Assert.Contains("Z", pathNode.GetAttribute("d"));
    Assert.Equal("red", pathNode.GetAttribute("fill"));
  }

  [Fact]
  public void ConvertCirclesToPaths_TransformedCircle_ConvertsToPath()
  {
    // Arrange
    string svgString = @"<svg viewBox='0 0 100 100'>
                                    <circle cx='50' cy='50' r='25' fill='red' transform='translate(20 20) scale(1.5)' />
                                  </svg>";
    var svg = SixSvg.FromString(svgString);

    // Act
    var result = svg.Query()
      .ConvertShapesToPaths()
      .Build();

    // Assert
    var pathNode = result.ModifiedSvg.Ast.Root.Children.FirstOrDefault(n => n.Name == "path");

    Assert.NotNull(pathNode);
    Assert.Equal("path", pathNode.Name);
    Assert.Contains("M5", pathNode.GetAttribute("d")); // Adjusted for translation and scale
    Assert.Contains("A37.5", pathNode.GetAttribute("d")); // Adjusted for scale
    Assert.Contains("Z", pathNode.GetAttribute("d"));
    Assert.Equal("red", pathNode.GetAttribute("fill"));
  }

  [Fact]
  public void ConvertRectanglesToPaths_SimpleRectangle_ConvertsToPath()
  {
    // Arrange
    string svgString = @"<svg viewBox='0 0 200 200'>
                                    <rect x='50' y='50' width='100' height='60' fill='blue' />
                                  </svg>";
    var svg = SixSvg.FromString(svgString);

    // Act
    var result = svg.Query()
      .ConvertShapesToPaths()
      .Build();

    // Assert
    var pathNode = result.ModifiedSvg.Ast.Root.Children.FirstOrDefault(n => n.Name == "path");

    Assert.NotNull(pathNode);
    Assert.Equal("path", pathNode.Name);
    Assert.Equal("M50 50L150 50L150 110L50 110Z", pathNode.GetAttribute("d"));
    Assert.Equal("blue", pathNode.GetAttribute("fill"));
  }


  [Fact]
  public void ConvertEllipsesToPaths_SimpleEllipse_ConvertsToPath()
  {
    // Arrange
    string svgString = @"<svg viewBox='0 0 100 100'>
                                    <ellipse cx='50' cy='50' rx='40' ry='20' fill='green' />
                                  </svg>";
    var svg = SixSvg.FromString(svgString);

    // Act
    var result = svg.Query()
      .ConvertShapesToPaths()
      .Build();

    // Assert
    var pathNode = result.ModifiedSvg.Ast.Root.Children.FirstOrDefault(n => n.Name == "path");

    Assert.NotNull(pathNode);
    Assert.Equal("path", pathNode.Name);
    Assert.Contains("M10", pathNode.GetAttribute("d"));
    Assert.Contains("A40", pathNode.GetAttribute("d"));
    Assert.Contains("20", pathNode.GetAttribute("d"));
    Assert.Contains("Z", pathNode.GetAttribute("d"));
    Assert.Equal("green", pathNode.GetAttribute("fill"));
  }

}