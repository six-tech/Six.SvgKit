using SixSvg = Six.SvgKit.Svg;

namespace Six.SvgKit.Optimizer.Test.Unit;

public class CleanupAttributesTests
{
  private Svg CreateSvgWithAttribute(string attributeName, string attributeValue)
  {
    var svgString = $@"<svg xmlns=""http://www.w3.org/2000/svg"" {attributeName}=""{attributeValue}""></svg>";
    return SixSvg.FromString(svgString);
  }

  private string GetCleanedAttributeValue(string attributeName, string attributeValue)
  {
    var svg = CreateSvgWithAttribute(attributeName, attributeValue);

    var result = svg
      .Query()
      .CleanupAttributes()
      .Build();

    return result.ModifiedSvg.Ast.Root.GetAttribute(attributeName);
  }

  [Theory]
  [InlineData("x", "  10.000  ", "10")]
  [InlineData("y", " -5.500 ", "-5.5")]
  [InlineData("width", "100.0000", "100")]
  [InlineData("height", "  50.1  ", "50.1")]
  public void CleanupAttributes_CoordinateAttributes_Cleaned(string attribute, string input, string expected)
  {
    var result = GetCleanedAttributeValue(attribute, input);
    Assert.Equal(expected, result);
  }

  [Fact]
  public void CleanupAttributes_ViewBox_Cleaned()
  {
    var result = GetCleanedAttributeValue("viewBox", "  0  0   100.00   100.00  ");
    Assert.Equal("0 0 100 100", result);
  }

  [Fact]
  public void CleanupAttributes_Points_Cleaned()
  {
    var result = GetCleanedAttributeValue("points", "  10 , 10   20,20  30 , 30  ");
    Assert.Equal("10 10 20 20 30 30", result);
  }

  [Theory]
  [InlineData("d", "M  10  ,  10   L  20  20   C 30 30 , 40 40 , 50 50 Z ", "M10 10L20 20C30 30 40 40 50 50Z")]
  [InlineData("d", "M 100 100 L 200 200", "M100 100L200 200")]
  [InlineData("d", "M 0,0 L 10,10 20,20 C 30,30 40,40 50,50", "M0 0L10 10 20 20C30 30 40 40 50 50")]
  [InlineData("d", "M10-20A5.5.3-4 110-.1", "M10-20A5.5.3-4 110-.1")]
  public void CleanupAttributes_PathData_Cleaned(string attribute, string input, string expected)
  {
    var result = GetCleanedAttributeValue(attribute, input);
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("transform", "  translate( 10 , 20 )  rotate( 45 )  ", "translate(10,20)rotate(45)")]
  [InlineData("transform", "scale(2)", "scale(2)")]
  public void CleanupAttributes_Transform_Cleaned(string attribute, string input, string expected)
  {
    var result = GetCleanedAttributeValue(attribute, input);
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("opacity", "  0.5  ", "0.5")]
  [InlineData("fill-opacity", " 1.000 ", "1")]
  [InlineData("stroke-width", "  2.50  ", "2.5")]
  public void CleanupAttributes_NumericAttributes_Cleaned(string attribute, string input, string expected)
  {
    var result = GetCleanedAttributeValue(attribute, input);
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("offset", "  50%  ", "50%")]
  [InlineData("offset", "  0.5  ", "0.5")]
  public void CleanupAttributes_PercentageOrNumber_Cleaned(string attribute, string input, string expected)
  {
    var result = GetCleanedAttributeValue(attribute, input);
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("fill", "  #FF0000  ", "#ff0000")]
  [InlineData("stroke", " rgb( 255, 0 , 0 ) ", "rgb(255,0,0)")]
  [InlineData("fill", "  rgba( 255 , 0 , 0 , 0.5 )  ", "rgba(255,0,0,0.5)")]
  [InlineData("fill", "  currentColor  ", "currentcolor")]
  public void CleanupAttributes_ColorAttributes_Cleaned(string attribute, string input, string expected)
  {
    var result = GetCleanedAttributeValue(attribute, input);
    Assert.Equal(expected, result);
  }

  [Theory]
  [InlineData("href", "  url(#myGradient)  ", "url(#myGradient)")]
  [InlineData("xlink:href", " #someId ", "#someId")]
  public void CleanupAttributes_UrlReferences_Cleaned(string attribute, string input, string expected)
  {
    var result = GetCleanedAttributeValue(attribute, input);
    Assert.Equal(expected, result);
  }

  [Fact]
  public void CleanupAttributes_PreservingAttributes_Trimmed()
  {
    var result = GetCleanedAttributeValue("preserveAspectRatio", "  xMidYMid meet  ");
    Assert.Equal("xMidYMid meet", result);
  }

  [Fact]
  public void CleanupAttributes_UnknownAttribute_Trimmed()
  {
    var result = GetCleanedAttributeValue("custom-attribute", "  some value  ");
    Assert.Equal("some value", result);
  }
}