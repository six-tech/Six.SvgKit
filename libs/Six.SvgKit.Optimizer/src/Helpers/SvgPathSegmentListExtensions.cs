using Svg.Pathing;
using System.Text;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class SvgPathSegmentListExtensions
{
  /// <summary>
  /// Converts an SvgPathSegmentList to a string path data representation.
  /// </summary>
  /// <param name="segments">The list of SvgPathSegments to convert.</param>
  /// <param name="decimalPrecision">The number of decimal places to include in the path data. Default is 3.</param>
  /// <returns>A string representing the path data of the SvgPathSegmentList.</returns>
  public static string ToPathData(this SvgPathSegmentList segments, int decimalPrecision = 3)
  {
    if(segments.Count == 0)
    {
      return string.Empty;
    }

    var pathData = new StringBuilder();
    foreach(var segment in segments)
    {
      switch(segment)
      {
        case SvgMoveToSegment moveToSegment:
          pathData.AppendFormat("M{0} {1}", FormatNumber(moveToSegment.End.X, decimalPrecision),
            FormatNumber(moveToSegment.End.Y, decimalPrecision));
          break;
        case SvgLineSegment lineSegment:
          pathData.AppendFormat("L{0} {1}", FormatNumber(lineSegment.End.X, decimalPrecision),
            FormatNumber(lineSegment.End.Y, decimalPrecision));
          break;
        case SvgCubicCurveSegment cubicCurveSegment:
          pathData.AppendFormat("C{0} {1} {2} {3} {4} {5}",
            FormatNumber(cubicCurveSegment.FirstControlPoint.X, decimalPrecision),
            FormatNumber(cubicCurveSegment.FirstControlPoint.Y, decimalPrecision),
            FormatNumber(cubicCurveSegment.SecondControlPoint.X, decimalPrecision),
            FormatNumber(cubicCurveSegment.SecondControlPoint.Y, decimalPrecision),
            FormatNumber(cubicCurveSegment.End.X, decimalPrecision),
            FormatNumber(cubicCurveSegment.End.Y, decimalPrecision));
          break;
        case SvgQuadraticCurveSegment quadraticCurveSegment:
          pathData.AppendFormat("Q{0} {1} {2} {3}",
            FormatNumber(quadraticCurveSegment.ControlPoint.X, decimalPrecision),
            FormatNumber(quadraticCurveSegment.ControlPoint.Y, decimalPrecision),
            FormatNumber(quadraticCurveSegment.End.X, decimalPrecision),
            FormatNumber(quadraticCurveSegment.End.Y, decimalPrecision));
          break;
        case SvgArcSegment arcSegment:
          pathData.AppendFormat("A{0} {1} {2} {3} {4} {5} {6}",
            FormatNumber(arcSegment.RadiusX, decimalPrecision),
            FormatNumber(arcSegment.RadiusY, decimalPrecision),
            FormatNumber(arcSegment.Angle, decimalPrecision),
            arcSegment.Size == SvgArcSize.Large ? "1" : "0",
            arcSegment.Sweep == SvgArcSweep.Positive ? "1" : "0",
            FormatNumber(arcSegment.End.X, decimalPrecision),
            FormatNumber(arcSegment.End.Y, decimalPrecision));
          break;
        case SvgClosePathSegment _:
          pathData.Append("Z");
          break;
      }
    }

    return pathData.ToString();
  }

  
  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
  
  static string FormatNumber(float number, int decimalPrecision)
  {
    string format = "0." + new string('#', decimalPrecision);
    return number.ToString(format, System.Globalization.CultureInfo.InvariantCulture);
  }
}