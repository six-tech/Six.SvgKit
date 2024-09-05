namespace SvgKit.Test.Unit;

public class SvgNodeTests
{
  [Fact]
  public void Constructor_SetsNameCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");

    // Act
    var node = new SvgNode(svg, "rect");

    // Assert
    Assert.Equal("rect", node.Name);
  }

  [Fact]
  public void AddChild_AddsChildCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var parent = new SvgNode(svg, "g");
    var child = new SvgNode(svg, "rect");

    // Act
    parent.AddChild(child);

    // Assert
    Assert.Single(parent.Children);
    Assert.Equal(child, parent.Children[0]);
    Assert.Equal(parent, child.Parent);
  }

  [Fact]
  public void SetAttribute_SetsAttributeCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var node = new SvgNode(svg, "rect");

    // Act
    node.SetAttribute("width", "100");

    // Assert
    Assert.Equal("100", node.Attributes["width"]);
  }

  [Fact]
  public void RemoveAttribute_RemovesAttributeCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var node = new SvgNode(svg, "rect");
    node.SetAttribute("width", "100");

    // Act
    node.RemoveAttribute("width");

    // Assert
    Assert.False(node.Attributes.ContainsKey("width"));
  }

  [Fact]
  public void ToXElement_CreatesCorrectXElement()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var node = new SvgNode(svg, "rect");
    node.SetAttribute("width", "100");
    node.SetAttribute("height", "50");

    // Act
    var xElement = node.ToXElement();

    // Assert
    Assert.Equal("rect", xElement.Name.LocalName);
    Assert.Equal("100", xElement.Attribute("width")?.Value);
    Assert.Equal("50", xElement.Attribute("height")?.Value);
  }
}