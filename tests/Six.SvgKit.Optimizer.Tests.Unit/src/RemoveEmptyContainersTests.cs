using SixSvg = Six.SvgKit.Svg;

namespace Template.Projects.Test.Unit;

public class RemoveEmptyContainersTests
{
  [Fact]
  public void RemoveEmptyContainers_RemovesEmptyGroupElements()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><g></g><g><rect/></g><g></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveEmptyContainers();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    
    Assert.Single(rootNode.Children);
    Assert.Equal("g", rootNode.Children.First().Name);
    Assert.Single(rootNode.Children.First().Children);
  }
}