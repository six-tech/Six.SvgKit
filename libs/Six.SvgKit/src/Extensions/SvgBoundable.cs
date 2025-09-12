using System.Drawing;
using Svg;

namespace Six.SvgKit;

[PublicAPI]
public class SvgBoundable : ISvgBoundable
{
  public float X { get; set; }
  public float Y { get; set; }
  public float Width { get; set; }
  public float Height { get; set; }

  public SvgBoundable(float x, float y, float width, float height)
  {
    X = x;
    Y = y;
    Width = width;
    Height = height;
  }

  public RectangleF Bounds => new(X, Y, Width, Height);

  public PointF Location => new(X, Y);

  public SizeF Size => new(Width, Height);
}