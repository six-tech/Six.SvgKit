namespace SvgKit.Optimizer.Test.Unit;

public partial class SvgOptimizerTests
{
  [Fact]
  public void RemoveEmptyContainers_RemovesEmptyGroupElements()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g></g><g><rect/></g><g></g></svg>");
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
  
  // CollapseGroups tests
  [Fact]
  public void CollapseGroups_EmptyGroup_RemovesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    Assert.DoesNotContain(rootNode.Children, e => e.Name == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithSingleChild_MovesAttributesToChild()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g fill='red'><rect><g fill='red'><rect/></g></rect></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    
    // Group should be removed and its attributes should be moved to the inner rect element
    var rectElement = rootNode.Children.First(e => e.Name == "rect");
    Assert.Equal("red", rectElement.Attributes["fill"]);
    
    // Nested group should be removed and its attributes should be moved to the inner rect element
    var innerRect = rectElement.Children.First(e => e.Name == "rect");
    Assert.Equal("red", rectElement.Attributes["fill"]);
  }

  [Fact]
  public void CollapseGroups_GroupWithFilterAttribute_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g filter='url(#f)'><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    Assert.Contains(rootNode.Children, e => e.Name == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithClipPathAttribute_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g clip-path='url(#cp)'><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    Assert.Contains(rootNode.Children, e => e.Name == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithMaskAttribute_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g mask='url(#m)'><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    Assert.Contains(rootNode.Children, e => e.Name == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithAnimationElement_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g><animate/><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    Assert.Contains(rootNode.Children, e => e.Name == "g");
  }
}