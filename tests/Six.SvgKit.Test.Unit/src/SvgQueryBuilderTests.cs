using SixSvg = Six.SvgKit.Svg;

using Xunit.Abstractions;

namespace SvgKit.Test.Unit;

public class SvgQueryBuilderTests
{
  readonly ITestOutputHelper _testOutputHelper;

  public SvgQueryBuilderTests(ITestOutputHelper testOutputHelper)
  {
    _testOutputHelper = testOutputHelper;
  }

  [Fact]
  public void Select_SetsPredicateCorrectly()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><rect/><circle/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.Select(node => node.Name == "rect");

    // Assert
    Assert.True(builder.Predicate(new SvgNode(svg, "rect")));
    Assert.False(builder.Predicate(new SvgNode(svg, "circle")));
  }

  [Fact]
  public void Where_CombinesPredicatesCorrectly()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><rect width='100'/><rect width='200'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder
      .Select(node => node.Name == "rect")
      .Where(node => node.Attributes.ContainsKey("width") && node.Attributes["width"] == "100");

    // Assert
    var rectNode100 = new SvgNode(svg, "rect");
    rectNode100.SetAttribute("width", "100");
    var rectNode200 = new SvgNode(svg, "rect");
    rectNode200.SetAttribute("width", "200");

    Assert.True(builder.Predicate(rectNode100));
    Assert.False(builder.Predicate(rectNode200));
  }

  [Fact]
  public void RemoveNodes_RemovesMatchingNodes()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><rect/><circle/><rect/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder
      .Select(node => node.Name == "rect")
      .RemoveNodes();

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);
    Assert.Single(rootNode.Children);
    Assert.Equal("circle", rootNode.Children.First().Name);
  }

  [Fact]
  public void SetAttribute_SetsAttributeOnMatchingNodes()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><rect/><circle/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder
      .Select(node => node.Name == "rect")
      .SetAttribute("width", "100");

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);

    var rectElement = rootNode.Children.First(e => e.Name == "rect");
    Assert.Equal("100", rectElement.Attributes["width"]);
  }

  [Fact]
  public void Select_Recursive_SelectsNodesFromNestedStructure()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><g fill='red'><rect><g fill='blue'><rect/></g></rect></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    var groups = builder
      .Select(n => n.Name == "g")
      .ToList();

    // Assert
    Assert.Equal(2, groups.Count); // Should now find both <g> elements
  }

  [Fact]
  public void Transform_ConditionallAttributes()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><rect width='50' height='50'/><rect width='150' height='100'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder
      .Select(n => n.Name == "rect")
      .Transform(n =>
      {
        var width = int.Parse(n.Attributes.GetValueOrDefault("width", "0"));
        n.SetAttribute("fill", width > 100 ? "red" : "blue");
      });

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);

    var smallRect = rootNode.Children
      .First(e => e.Name == "rect" && int.Parse(e.Attributes["width"]) == 50);
    
    var largeRect = rootNode.Children
      .First(e => e.Name == "rect" && int.Parse(e.Attributes["width"]) == 150);

    Assert.Equal("blue", smallRect.Attributes["fill"]);
    Assert.Equal("red", largeRect.Attributes["fill"]);
  }


  [Fact]
  public void SetAttributes_SetsMultipleAttributes()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><path d='M0 0 L10 10'/></svg>");
    var builder = new SvgQueryBuilder(svg);
    var newAttributes = new Dictionary<string, string> { { "fill", "yellow" }, { "stroke", "black" } };

    // Act
    builder.Select(n => n.Name == "path").SetAttributes(newAttributes);

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);

    var pathElement = rootNode.Children.First(e => e.Name == "path");

    Assert.NotNull(pathElement);
    Assert.Equal("yellow", pathElement.Attributes["fill"]);
    Assert.Equal("black", pathElement.Attributes["stroke"]);
  }

  [Fact]
  public void RemoveAttributeIf_RemovesAttributeBasedOnCondition()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><rect fill='red' stroke='blue'/><rect stroke='green'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveAttributeIf(n => n.Name == "rect" && !n.Attributes.ContainsKey("fill"), "stroke");

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);

    var filledRect = rootNode.Children[0];
    var nonFilledRect = rootNode.Children[1];
    
    Assert.Equal("blue", filledRect.Attributes["stroke"]);
    Assert.False(nonFilledRect.Attributes.TryGetValue("stroke", out _));
  }

  [Fact]
  public void HasAttribute_FiltersByAttribute()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><rect class='highlight'/><circle/><rect class='normal'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    var highlightedRects = builder
      .HasAttribute("class", "highlight")
      .ToList();

    // Assert
    Assert.Single(highlightedRects);
    Assert.Equal("rect", highlightedRects[0].Name);
    Assert.Equal("highlight", highlightedRects[0].Attributes["class"]);
  }

  [Fact]
  public void TransformAttribute_ModifiesAttributeValue()
  {
    // Arrange
    var svg = SixSvg.FromString("<svg><text font-size='12'>Hello</text></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.Select(n => n.Name == "text")
      .TransformAttribute("font-size", size => (int.Parse(size) + 2) + "px");

    // Assert
    var result = builder.Build();
    var rootNode = result.ModifiedSvg.Ast.Root;
    
    Assert.NotNull(rootNode);

    var textElement = rootNode.Children.First(e => e.Name == "text");

    Assert.Equal("14px", textElement.Attributes["font-size"]);
  }


  // Attribute SELECT
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Helper method to create simple SVG document.
  /// </summary>
  static SixSvg CreateSvgDocument()
  {
    string svgString = @"
            <svg xmlns='http://www.w3.org/2000/svg'>
                <rect width='100' height='50'/>
                <rect width='200' height='100'/>
                <circle radius='25'/>
            </svg>";
    return SixSvg.FromString(svgString);
  }

  [Fact]
  public void SelectAttributes_ModifiesAttributesForAllSelectedNodes()
  {
    var svg = CreateSvgDocument();
    var builder = new SvgQueryBuilder(svg);

    var result = builder
      .Select(n => n.Name == "rect")
      .SelectAttributes(
        attr => attr.Key == "width",
        (node, key, value) => node.SetAttribute(key, (int.Parse(value) * 2).ToString())
      )
      .Build();

    var rects = result.ModifiedSvg.Ast.Root.Children.Where(n => n.Name == "rect").ToList();
    
    Assert.Equal("200", rects[0].Attributes["width"]);
    Assert.Equal("50", rects[0].Attributes["height"]);
    Assert.Equal("400", rects[1].Attributes["width"]);
    Assert.Equal("100", rects[1].Attributes["height"]);

    var circle = result.ModifiedSvg.Ast.Root.Children.First(n => n.Name == "circle");
    Assert.Equal("25", circle.Attributes["radius"]);
  }

  [Fact]
  public void SelectAttributes_DoesNotModifyUnselectedNodes()
  {
    var svg = CreateSvgDocument();
    var builder = new SvgQueryBuilder(svg);

    var result = builder
      .Select(n => n.Name == "circle")
      .SelectAttributes(
        _ => true,
        (node, key, value) => node.SetAttribute(key, value + "_modified")
        )
      .Build();

    var rects = result.ModifiedSvg.Ast.Root.Children.Where(n => n.Name == "rect").ToList();
    var circle = result.ModifiedSvg.Ast.Root.Children.First(n => n.Name == "circle");
    
    Assert.Equal("100", rects[0].Attributes["width"]);
    Assert.Equal("50", rects[0].Attributes["height"]);
    Assert.Equal("200", rects[1].Attributes["width"]);
    Assert.Equal("100", rects[1].Attributes["height"]);

    Assert.Equal("25_modified", circle.Attributes["radius"]);
  }

  [Fact]
  public void SelectAttributes_AppliesPredicateCorrectly()
  {
    var svg = CreateSvgDocument();
    var builder = new SvgQueryBuilder(svg);

    var result = builder
      .Select(n => n.Name == "rect")
      .SelectAttributes(
        attr => attr.Key.StartsWith("w"),
        (node, key, value) => node.SetAttribute(key, (int.Parse(value) * 2).ToString())
      )
      .Build();

    var rects = result.ModifiedSvg.Ast.Root.Children.Where(n => n.Name == "rect").ToList();
    Assert.Equal("200", rects[0].Attributes["width"]);
    Assert.Equal("50", rects[0].Attributes["height"]);
    Assert.Equal("400", rects[1].Attributes["width"]);
    Assert.Equal("100", rects[1].Attributes["height"]);
  }
}