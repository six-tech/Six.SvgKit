namespace SvgKit.Test.Unit;

public class SvgTests
{
  private const string TestSvgString = @"
            <svg xmlns=""http://www.w3.org/2000/svg"" width=""100"" height=""100"">
                <rect x=""10"" y=""10"" width=""80"" height=""80"" fill=""blue"" />
            </svg>";

  [Fact]
  public void FromString_CreatesSvgInstance()
  {
    var svg = Svg.FromString(TestSvgString);
    Assert.NotNull(svg);
    Assert.IsType<Svg>(svg);
  }

  [Fact]
  public void FromXDocument_CreatesSvgInstance()
  {
    var xDoc = XDocument.Parse(TestSvgString);
    var svg = Svg.FromXDocument(xDoc);
    Assert.NotNull(svg);
    Assert.IsType<Svg>(svg);
  }

  [Fact]
  public void WithId_SetsSvgId()
  {
    var svg = Svg.FromString(TestSvgString);
    svg.WithId("testId");
    Assert.Equal("testId", svg.Id);
  }

  [Fact]
  public void Clone_CreatesCopy()
  {
    var original = Svg.FromString(TestSvgString);
    original.WithId("originalId");
    var clone = original.Clone();

    Assert.NotNull(clone);
    Assert.IsType<Svg>(clone);
    Assert.Equal(original.Id, clone.Id);
    Assert.NotSame(original, clone);
  }

  [Fact]
  public void ToXDocument_ReturnsValidXDocument()
  {
    var svg = Svg.FromString(TestSvgString);
    var xDoc = svg.ToXDocument();

    Assert.NotNull(xDoc);
    Assert.IsType<XDocument>(xDoc);
    Assert.Equal("svg", xDoc.Root?.Name.LocalName);
  }

  [Fact]
  public void ToSvg_ReturnsValidSvgString()
  {
    var svg = Svg.FromString(TestSvgString);
    var svgString = svg.ToSvg();

    Assert.NotNull(svgString);
    Assert.Contains("<svg", svgString);
    Assert.Contains("<rect", svgString);
  }

  [Fact]
  public void ToBytes_ReturnsNonEmptyByteArray()
  {
    var svg = Svg.FromString(TestSvgString);
    var bytes = svg.ToBytes();

    Assert.NotNull(bytes);
    Assert.NotEmpty(bytes);
  }
}