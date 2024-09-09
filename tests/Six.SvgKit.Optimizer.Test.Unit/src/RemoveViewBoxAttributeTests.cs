using SixSvg = Six.SvgKit.Svg;

namespace Template.Projects.Test.Unit;

public class RemoveViewBoxAttributeTests
{
  [Fact]
  public void RemoveViewBoxAttribute_RemovesViewBoxAttribute()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg viewBox='0 0 100 100'></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveViewBoxAttribute();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    Assert.False(rootNode.Attributes.ContainsKey("viewBox"));
  }
}