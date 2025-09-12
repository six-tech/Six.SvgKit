using SixSvg = Six.SvgKit.Svg;

namespace SvgKit.Optimizer.Test.Unit;

public class RemoveEmptyTextTests
{
  

  [Fact]
  public void RemoveEmptyText_RemovesEmptyTextElements()
  {
    // Arrange
    var svg = SixSvg.FromString(
      @"<svg>
          <text id='empty-text'></text>
          <tspan id='empty-tspan'></tspan>
          <tref id='empty-tref' xlink:href='' />
          <text id='non-empty-text'>Non-empty</text>
          <g><text id='nested-empty-text'></text></g>
        </svg>");
    
    var builder = new SvgQueryBuilder(svg);

    // Act
    var result = builder
      .RemoveEmptyText()
      .Build();

    // Assert
    var rootChildren = result.ModifiedSvg.Ast.Root.Children;

    // Check that empty text, tspan, and tref elements are removed
    Assert.DoesNotContain(rootChildren, n => n.Attributes.ContainsKey("id") &&
                                             (n.Attributes["id"] == "empty-text" ||
                                              n.Attributes["id"] == "empty-tspan" ||
                                              n.Attributes["id"] == "empty-tref"));

    // Check that non-empty text element remains
    Assert.Contains(rootChildren, n => n.Attributes.ContainsKey("id") && n.Attributes["id"] == "non-empty-text");

    // Check that nested empty text element is removed
    var group = result.ModifiedSvg.Ast.Root.Children.FirstOrDefault(n => n.Name == "g");
    
    Assert.NotNull(group);
    Assert.DoesNotContain(group.Children,
      n => n.Attributes.ContainsKey("id") && n.Attributes["id"] == "nested-empty-text");
  }

  [Theory]
  [InlineData(true, true, true)] // Remove all
  [InlineData(true, false, false)] // Remove only text
  [InlineData(false, true, false)] // Remove only tspan
  [InlineData(false, false, true)] // Remove only tref
  public void RemoveEmptyText_RespectsFlags(bool removeText, bool removeTspan, bool removeTref)
  {
    // Arrange
    var svg = SixSvg.FromString(
      @"<svg>
          <text id='text'></text>
          <tspan id='tspan'></tspan>
          <tref id='tref'/>
        </svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    var result = builder
      .RemoveEmptyText(removeText, removeTspan, removeTref)
      .Build();

    var rootChildrenIds =
      result.ModifiedSvg.Ast.Root.Children.Select(n => n.Attributes.GetValueOrDefault("id")).ToList();

    // Assert
    Assert.Equal(removeText ? 0 : 1, rootChildrenIds.Count(id => id == "text"));
    Assert.Equal(removeTspan ? 0 : 1, rootChildrenIds.Count(id => id == "tspan"));
    Assert.Equal(removeTref ? 0 : 1, rootChildrenIds.Count(id => id == "tref"));
  }
}