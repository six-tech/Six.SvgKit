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
    var svg = Svg.FromString("<svg><rect/><circle/></svg>");
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
    var svg = Svg.FromString("<svg><rect width='100'/><rect width='200'/></svg>");
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
    var svg = Svg.FromString("<svg><rect/><circle/><rect/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder
      .Select(node => node.Name == "rect")
      .RemoveNodes();

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);
    Assert.Single(resultXml.Elements());
    Assert.Equal("circle", resultXml.Elements().First().Name.LocalName);
  }

  [Fact]
  public void SetAttribute_SetsAttributeOnMatchingNodes()
  {
    // Arrange
    var svg = Svg.FromString("<svg><rect/><circle/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder
      .Select(node => node.Name == "rect")
      .SetAttribute("width", "100");

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);

    var rectElement = resultXml.Elements().First(e => e.Name.LocalName == "rect");
    Assert.Equal("100", rectElement.Attribute("width")?.Value);
  }

  [Fact]
  public void Select_Recursive_SelectsNodesFromNestedStructure()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g fill='red'><rect><g fill='blue'><rect/></g></rect></g></svg>");
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
    var svg = Svg.FromString("<svg><rect width='50' height='50'/><rect width='150' height='100'/></svg>");
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
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);

    var smallRect = resultXml.Elements()
      .First(e => e.Name.LocalName == "rect" && int.Parse(e.Attribute("width")!.Value) == 50);
    var largeRect = resultXml.Elements()
      .First(e => e.Name.LocalName == "rect" && int.Parse(e.Attribute("width")!.Value) == 150);

    Assert.Equal("blue", smallRect.Attribute("fill")?.Value);
    Assert.Equal("red", largeRect.Attribute("fill")?.Value);
  }

  [Fact]
  public void SetAttributeIf_SetsAttributeBasedOnCondition()
  {
    // Arrange
    var svg = Svg.FromString("<svg><circle r='5'/><circle r='15'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.SetAttributeIf(n => n.Name == "circle" && int.Parse(n.Attributes.GetValueOrDefault("r", "0")) > 10, "fill",
      "blue");

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);

    var smallCircle = resultXml.Elements()
      .First(e => e.Name.LocalName == "circle" && int.Parse(e.Attribute("r")!.Value) == 5);
    var largeCircle = resultXml.Elements()
      .First(e => e.Name.LocalName == "circle" && int.Parse(e.Attribute("r")!.Value) == 15);

    Assert.Null(smallCircle.Attribute("fill"));
    Assert.Equal("blue", largeCircle.Attribute("fill")?.Value);
  }

  [Fact]
  public void SetAttributes_SetsMultipleAttributes()
  {
    // Arrange
    var svg = Svg.FromString("<svg><path d='M0 0 L10 10'/></svg>");
    var builder = new SvgQueryBuilder(svg);
    var newAttributes = new Dictionary<string, string> { { "fill", "yellow" }, { "stroke", "black" } };

    // Act
    builder.Select(n => n.Name == "path").SetAttributes(newAttributes);

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);

    var pathElement = resultXml.Elements().First(e => e.Name.LocalName == "path");

    Assert.NotNull(pathElement);
    Assert.Equal("yellow", pathElement.Attribute("fill")?.Value);
    Assert.Equal("black", pathElement.Attribute("stroke")?.Value);
  }

  [Fact]
  public void RemoveAttributeIf_RemovesAttributeBasedOnCondition()
  {
    // Arrange
    var svg = Svg.FromString("<svg><rect fill='red' stroke='blue'/><rect stroke='green'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveAttributeIf(n => n.Name == "rect" && !n.Attributes.ContainsKey("fill"), "stroke");

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);

    var filledRect = resultXml.Elements().First(e => e.Name.LocalName == "rect" && e.Attribute("fill") != null);
    var nonFilledRect = resultXml.Elements().First(e => e.Name.LocalName == "rect" && e.Attribute("fill") == null);

    Assert.Equal("blue", filledRect.Attribute("stroke")?.Value);
    Assert.Null(nonFilledRect.Attribute("stroke"));
  }

  [Fact]
  public void HasAttribute_FiltersByAttribute()
  {
    // Arrange
    var svg = Svg.FromString("<svg><rect class='highlight'/><circle/><rect class='normal'/></svg>");
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
    var svg = Svg.FromString("<svg><text font-size='12'>Hello</text></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.Select(n => n.Name == "text")
      .TransformAttribute("font-size", size => (int.Parse(size) + 2) + "px");

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);

    var textElement = resultXml.Elements().First(e => e.Name.LocalName == "text");

    Assert.Equal("14px", textElement.Attribute("font-size")?.Value);
  }


  // Attribute SELECT
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Helper method to create simple SVG document.
  /// </summary>
  static Svg CreateSvgDocument()
  {
    string svgString = @"
            <svg xmlns='http://www.w3.org/2000/svg'>
                <rect width='100' height='50'/>
                <rect width='200' height='100'/>
                <circle radius='25'/>
            </svg>";
    return Svg.FromString(svgString);
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