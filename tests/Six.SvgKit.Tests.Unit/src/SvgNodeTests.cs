using SixSvg = Six.SvgKit.Svg;

namespace SvgKit.Test.Unit;

public class SvgNodeTests
{
  [Fact]
  public void Constructor_SetsNameCorrectly()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg></svg>");

    // Act
    var node = new SvgNode(svg, "rect");

    // Assert
    Assert.Equal("rect", node.Name);
  }

  [Fact]
  public void AddChild_AddsChildCorrectly()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg></svg>");
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
    var svg = SixSvg.FromString("<svg></svg>");
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

  [Fact]
  public void SetAttribute_SetsAttributeCorrectly()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg></svg>");
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
    var svg = SixSvg.FromString("<svg></svg>");
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
  static SixSvg CreateSvgDocument()
  {
    string svgString = @"
            <svg xmlns='http://www.w3.org/2000/svg'>
                <rect width='100' height='50'/>
            </svg>";
    return SixSvg.FromString(svgString);
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

  [Fact]
  public void CreateChild_CreatesChildNode()
  {
    var svg = SixSvg.FromString("<svg></svg>");
    var parent = new SvgNode(svg, "g");
    var child = parent.CreateChild("rect");

    Assert.Single(parent.Children);
    Assert.Equal("rect", child.Name);
    Assert.Same(parent, child.Parent);
  }

  [Fact]
  public void RemoveChild_RemovesChildNode()
  {
    var svg = SixSvg.FromString("<svg></svg>");
    var parent = new SvgNode(svg, "g");
    var child = new SvgNode(svg, "rect");
    parent.AddChild(child);

    bool removed = parent.RemoveChild(child);

    Assert.True(removed);
    Assert.Empty(parent.Children);
    Assert.Null(child.Parent);
  }

  [Fact]
  public void Remove_RemovesNodeFromParent()
  {
    var svg = SixSvg.FromString("<svg></svg>");
    var parent = new SvgNode(svg, "g");
    var child = new SvgNode(svg, "rect");
    parent.AddChild(child);

    bool removed = child.Remove();

    Assert.True(removed);
    Assert.Empty(parent.Children);
    Assert.Null(child.Parent);
  }

  [Fact]
  public void ReplaceWith_ReplacesNodeWithNewNode()
  {
    var svg = SixSvg.FromString("<svg></svg>");
    var parent = new SvgNode(svg, "g");
    var oldChild = new SvgNode(svg, "rect");
    var newChild = new SvgNode(svg, "circle");
    parent.AddChild(oldChild);

    oldChild.ReplaceWith(newChild);

    Assert.Single(parent.Children);
    Assert.Same(newChild, parent.Children[0]);
    Assert.Same(parent, newChild.Parent);
    Assert.Null(oldChild.Parent);
  }

  [Fact]
  public void ClearAttributes_RemovesAllAttributes()
  {
    var svg = SixSvg.FromString("<svg></svg>");
    var node = new SvgNode(svg, "rect");
    node.SetAttribute("width", "100");
    node.SetAttribute("height", "50");

    node.ClearAttributes();

    Assert.Empty(node.Attributes);
  }

  [Fact]
  public void TransferAttributesTo_TransfersAttributes()
  {
    var svg = SixSvg.FromString("<svg></svg>");
    var sourceNode = new SvgNode(svg, "rect");
    sourceNode.SetAttribute("width", "100");
    sourceNode.SetAttribute("height", "50");
    var targetNode = new SvgNode(svg, "rect");

    sourceNode.TransferAttributesTo(targetNode);

    Assert.Equal("100", targetNode.Attributes["width"]);
    Assert.Equal("50", targetNode.Attributes["height"]);
  }

  [Fact]
  public void TransferAttributesFrom_TransfersAttributes()
  {
    var svg = SixSvg.FromString("<svg></svg>");
    var sourceNode = new SvgNode(svg, "rect");
    sourceNode.SetAttribute("width", "100");
    sourceNode.SetAttribute("height", "50");
    var targetNode = new SvgNode(svg, "rect");

    targetNode.TransferAttributesFrom(sourceNode);

    Assert.Equal("100", targetNode.Attributes["width"]);
    Assert.Equal("50", targetNode.Attributes["height"]);
  }
}