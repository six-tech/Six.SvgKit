namespace Six.SvgKit.Optimizer.Test.Unit;

public partial class SvgOptimizerTests
{
  
  [Fact]
  public void MergePaths_MergesMultiplePathsIntoOne()
  {
    // Arrange
    var svg = Svg.FromString("<svg><path d='M0 0h10'/><path d='M10 10h10'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.MergePaths();

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);
    
    Assert.Single(resultXml.Elements());
    Assert.Equal("path", resultXml.Elements().First().Name.LocalName);
    Assert.Equal("M0 0h10 M10 10h10", resultXml.Elements().First().Attribute("d")?.Value);
  }
  
}