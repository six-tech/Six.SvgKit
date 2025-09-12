using SixSvg = Six.SvgKit.Svg;

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
    var svg = SixSvg.FromString(TestSvgString);
    Assert.NotNull(svg);
    Assert.IsType<SixSvg>(svg);
  }

  [Fact]
  public void ToSvg_ReturnsValidSvgString()
  {
    var svg = SixSvg.FromString(TestSvgString);
    var svgString = svg.ToSvg();

    Assert.NotNull(svgString);
    Assert.Contains("<svg", svgString);
    Assert.Contains("<rect", svgString);
  }

  [Fact]
  public void ToBytes_ReturnsNonEmptyByteArray()
  {
    var svg = SixSvg.FromString(TestSvgString);
    var bytes = svg.ToBytes();

    Assert.NotNull(bytes);
    Assert.NotEmpty(bytes);
  }
}