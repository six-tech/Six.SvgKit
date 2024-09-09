using SixSvg = Six.SvgKit.Svg;

namespace SvgKit.Optimizer.Test.Unit;

public class CollapseGroupsTests
{
  [Fact]
  public void CollapseGroups_EmptyGroup_RemovesGroup()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><g></g></svg>");
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
    var svg = SixSvg.FromString("<svg><g fill='red'><rect><g fill='red'><rect/></g></rect></g></svg>");
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
    var svg = SixSvg.FromString("<svg><g filter='url(#f)'><rect/></g></svg>");
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
    var svg = SixSvg.FromString("<svg><g clip-path='url(#cp)'><rect/></g></svg>");
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
    var svg = SixSvg.FromString("<svg><g mask='url(#m)'><rect/></g></svg>");
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
    var svg = SixSvg.FromString("<svg><g><animate/><rect/></g></svg>");
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