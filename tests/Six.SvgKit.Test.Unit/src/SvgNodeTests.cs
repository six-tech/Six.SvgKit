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
  public void ToXElement_CreatesCorrectXElement()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var node = new SvgNode(svg, "rect");
    node.SetAttribute("width", "100");
    node.SetAttribute("height", "50");

    // Act
    var xElement = node.ToAngleSharpElement();

    // Assert
    Assert.Equal("rect", xElement.LocalName);
    Assert.Equal("100", xElement.Attributes.GetNamedItem("width")!.Value);
    Assert.Equal("50", xElement.Attributes.GetNamedItem("height")!.Value);
  }

  
  // Attributes
  // ----------------------------------------------------------------------------------------------------------
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

  
  /// <summary>
  /// Helper method to create simple SVG document.
  /// </summary>
  static Svg CreateSvgDocument()
  {
    string svgString = @"
            <svg xmlns='http://www.w3.org/2000/svg'>
                <rect width='100' height='50'/>
            </svg>";
    return Svg.FromString(svgString);
  }
  
  [Fact]
  public void SelectAttributes_SelectsAllAttributes_WhenPredicateAlwaysTrue()
  {
    var svg = CreateSvgDocument();
    var node = svg.Ast.Root.Children[0]; // Get the rect node

    node.SelectAttributes(_ => true);

    Assert.Equal(2, node.Attributes.Count);
    Assert.Equal("100", node.Attributes["width"]);
    Assert.Equal("50", node.Attributes["height"]);
  }


  [Fact]
  public void SelectAttributes_TransformsAttributes_WhenTransformProvided()
  {
    var svg = CreateSvgDocument();
    var node = svg.Ast.Root.Children[0]; // Get the rect node

    node.SelectAttributes(
      _ => true,
      (key, value) => (key.ToUpper(), (int.Parse(value) * 2).ToString())
    );

    Assert.Equal(2, node.Attributes.Count);
    Assert.Equal("200", node.Attributes["WIDTH"]);
    Assert.Equal("100", node.Attributes["HEIGHT"]);
  }

  [Fact]
  public void SelectAttributes_SelectsAndTransformsSpecificAttributes()
  {
    var svg = CreateSvgDocument();
    var node = svg.Ast.Root.Children[0]; // Get the rect node

    node.SelectAttributes(
      attr => attr.Key == "width",
      (key, value) => (key + "_modified", (int.Parse(value) * 2).ToString())
    );

    Assert.Equal(2, node.Attributes.Count);
    Assert.Equal("200", node.Attributes["width_modified"]);
    Assert.Equal("50", node.Attributes["height"]);
  }
}