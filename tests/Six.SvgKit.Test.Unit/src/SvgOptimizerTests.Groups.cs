namespace SvgKit.Test.Unit;

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
    var resultXml = result.ToXDocument().Root;
    
    Assert.NotNull(resultXml);
    Assert.Single(resultXml.Elements());
    Assert.Equal("g", resultXml.Elements().First().Name.LocalName);
    Assert.Single(resultXml.Elements().First().Elements());
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
    var svgElement = result.ToXDocument().Root;

    Assert.NotNull(svgElement);
    Assert.DoesNotContain(svgElement.Descendants(), e => e.Name.LocalName == "g");
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
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    
    // Group should be removed and its attributes should be moved to the inner rect element
    var rectElement = svgElement.Descendants().First(e => e.Name.LocalName == "rect");
    Assert.Equal("red", rectElement.Attribute("fill")?.Value);
    
    // Nested group should be removed and its attributes should be moved to the inner rect element
    var innerRect = rectElement.Descendants().First(e => e.Name.LocalName == "rect");
    Assert.Equal("red", innerRect.Attribute("fill")?.Value);
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
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
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
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
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
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
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
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
  }
}