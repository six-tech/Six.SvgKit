namespace Six.SvgKit.Optimizer.Test.Unit;

public partial class SvgOptimizerTests
{
  [Fact]
  public void PathHelpers_MergePaths_SinglePath_ReturnsSamePath()
  {
    // Arrange
    string svgContent = "<svg><path d=\"M0 0 L10 10\" fill=\"none\" stroke=\"black\" /></svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal("<svg><path d=\"M0 0 L10 10\" fill=\"none\" stroke=\"black\"></path></svg>", result);
  }

  [Fact]
  public void PathHelpers_MergePaths_MultiplePathsWithSameStyle_MergesPathsIntoOne()
  {
    // Arrange
    string svgContent = "<svg>" +
                        "<path d=\"M0 0 L10 10\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M20 20 L30 30\" fill=\"none\" stroke=\"black\" />" +
                        "</svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal("<svg><path d=\"M0 0 L10 10 M20 20 L30 30\" fill=\"none\" stroke=\"black\"></path></svg>", result);
  }

  [Fact]
  public void PathHelpers_MergePaths_MultiplePathsWithDifferentStyles_MergesPathsIntoSeparateGroups()
  {
    // Arrange
    string svgContent = "<svg>" +
                        "<path d=\"M0 0 L10 10\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M20 20 L30 30\" fill=\"none\" stroke=\"red\" />" +
                        "<path d=\"M40 40 L50 50\" fill=\"none\" stroke=\"black\" />" +
                        "</svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal("<svg>" +
                 "<path d=\"M0 0 L10 10 M40 40 L50 50\" fill=\"none\" stroke=\"black\"></path>" +
                 "<path d=\"M20 20 L30 30\" fill=\"none\" stroke=\"red\"></path>" +
                 "</svg>", result);
  }

  [Fact]
  public void PathHelpers_MergePaths_MultiplePathsWithDifferentFillAndStroke_MergesPathsIntoSeparateGroups()
  {
    // Arrange
    string svgContent = "<svg>" +
                        "<path d=\"M0 0 L10 10\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M20 20 L30 30\" fill=\"red\" stroke=\"black\" />" +
                        "<path d=\"M40 40 L50 50\" fill=\"none\" stroke=\"black\" />" +
                        "</svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal("<svg>" +
                 "<path d=\"M0 0 L10 10 M40 40 L50 50\" fill=\"none\" stroke=\"black\"></path>" +
                 "<path d=\"M20 20 L30 30\" fill=\"red\" stroke=\"black\"></path>" +
                 "</svg>", result);
  }

  [Fact]
  public void PathHelpers_MergePaths_EmptySvg_ReturnsEmptySvg()
  {
    // Arrange
    string svgContent = "<svg></svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal("<svg></svg>", result);
  }

  [Fact]
  public void PathHelpers_MergePaths_MultiplePathsWithSameStyleAndDifferentCommands_MergesPathsCorrectly()
  {
    // Arrange
    string svgContent = "<svg>" +
                        "<path d=\"M0 0 L10 10\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M20 20 C30 30 40 40 50 50\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M60 60 Q70 70 80 80\" fill=\"none\" stroke=\"black\" />" +
                        "</svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal(
      "<svg><path d=\"M0 0 L10 10 M20 20 C30 30 40 40 50 50 M60 60 Q70 70 80 80\" fill=\"none\" stroke=\"black\"></path></svg>",
      result);
  }

  [Fact]
  public void PathHelpers_MergePaths_MultiplePathsWithDifferentStylesAndClosedPaths_MergesPathsCorrectly()
  {
    // Arrange
    string svgContent = "<svg>" +
                        "<path d=\"M0 0 L10 10 Z\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M20 20 L30 30 L40 40 Z\" fill=\"red\" stroke=\"black\" />" +
                        "<path d=\"M50 50 L60 60 Z\" fill=\"none\" stroke=\"black\" />" +
                        "</svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal("<svg>" +
                 "<path d=\"M0 0 L10 10 Z M50 50 L60 60 Z\" fill=\"none\" stroke=\"black\"></path>" +
                 "<path d=\"M20 20 L30 30 L40 40 Z\" fill=\"red\" stroke=\"black\"></path>" +
                 "</svg>", result);
  }

  [Fact]
  public void PathHelpers_MergePaths_MultiplePathsWithDifferentStylesAndArcs_MergesPathsCorrectly()
  {
    // Arrange
    string svgContent = "<svg>" +
                        "<path d=\"M0 0 A10 10 0 0 1 10 10\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M20 20 A30 30 0 0 0 50 50\" fill=\"red\" stroke=\"black\" />" +
                        "<path d=\"M60 60 A20 20 0 1 1 80 80\" fill=\"none\" stroke=\"black\" />" +
                        "</svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal("<svg>" +
                 "<path d=\"M0 0 A10 10 0 0 1 10 10 M60 60 A20 20 0 1 1 80 80\" fill=\"none\" stroke=\"black\"></path>" +
                 "<path d=\"M20 20 A30 30 0 0 0 50 50\" fill=\"red\" stroke=\"black\"></path>" +
                 "</svg>", result);
  }

  [Fact]
  public void PathHelpers_MergePaths_MultiplePathsWithSameStyleAndRelativeCommands_MergesPathsCorrectly()
  {
    // Arrange
    string svgContent = "<svg>" +
                        "<path d=\"M0 0 l10 10\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M20 20 c10 10 20 20 30 30\" fill=\"none\" stroke=\"black\" />" +
                        "<path d=\"M60 60 q10 10 20 20\" fill=\"none\" stroke=\"black\" />" +
                        "</svg>";

    // Act
    string result = SvgPathMerger.MergePaths(svgContent);

    // Assert
    Assert.Equal(
      "<svg><path d=\"M0 0 l10 10 M20 20 c10 10 20 20 30 30 M60 60 q10 10 20 20\" fill=\"none\" stroke=\"black\"></path></svg>",
      result);
  }

}