namespace Six.SvgKit.Optimizer.Test.Unit;

public partial class SvgOptimizerTests
{
  [Fact]
  public void RemoveViewBoxAttribute_RemovesViewBoxAttribute()
  {
    // Arrange
    var svg = Svg.FromString("<svg viewBox='0 0 100 100'></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveViewBoxAttribute();

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;
    
    Assert.NotNull(resultXml);
    Assert.Null(resultXml.Attribute("viewBox"));
  }
  
  [Fact]
  public void CleanupAttributes_RemovesWhitespace()
  {
    // Arrange
    var svg = Svg.FromString("<svg><circle radius=' 25 ' /><rect transform='translate(10, 5)' /></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    var result = builder
      .CleanupAttributes()
      .Build();
    
    // Assert
    var circle = result.ModifiedSvg.Ast.Root.Children.First(n => n.Name == "circle");
    Assert.Equal("25", circle.Attributes["radius"]); // Check if leading/trailing spaces are removed

    var rect = result.ModifiedSvg.Ast.Root.Children.First(n => n.Name == "rect");
    Assert.Equal("translate(10,5)", rect.Attributes["transform"]); // Check if internal spaces are removed
  }
}