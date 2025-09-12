using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Runtime.CompilerServices;
using Svg;
using Svg.Pathing;
using Svg.Transforms;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class ConvertShapesToPathsExtensions
{
  /// <summary>
  /// Converts supported SVG shapes (circle, ellipse, rectangle) into path elements within the SVG document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance used to query and manipulate the SVG document.</param>
  /// <param name="precision">The decimal precision value for the conversion process, with a default of 3 decimal places.</param>
  /// <returns>An updated SvgQueryBuilder instance with the modified SVG document.</returns>
  /// <exception cref="ArgumentException">Thrown when an unsupported SVG primitive is encountered.</exception>
  public static SvgQueryBuilder ConvertShapesToPaths(this SvgQueryBuilder builder, int precision = 3)
  {
    var groups = builder
      .Select(n => n.Name == "circle" || n.Name == "ellipse" || n.Name == "rect")
      .GroupBy(n => n.Name);

    // Process nodes within each group
    foreach(var group in groups)
    {
      switch(group.Key)
      {
        case "circle":
          ConvertCirclesToPaths(group, precision);
          break;
        case "ellipse":
          ConvertEllipsesToPaths(group, precision);
          break;
        case "rect":
          ConvertRectanglesToPaths(group, precision);
          break;
        default:
          throw new ArgumentException($"Unsupported SVG primitive for converting to path: {group.Key}");
      }
    }

    return builder;
  }

  
  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
  
  static void ConvertCirclesToPaths(IEnumerable<SvgNode> nodes, int precision = 3)
  {
    foreach(var circle in nodes)
    {
      var svgCircle = new SvgCircle
      {
        CenterX = ParseFloat(circle.GetAttribute("cx")),
        CenterY = ParseFloat(circle.GetAttribute("cy")),
        Radius = ParseFloat(circle.GetAttribute("r"))
      };

      if(circle.HasAttribute("transform"))
      {
        string transformAttribute = circle.GetAttribute("transform");
        svgCircle.Transforms = ParseTransforms(transformAttribute);
      }
      
      // Now we have all we need to create a path
      // We'll create a new SvgNode of type "path", transfer basic attributes and set the path data
      var pathNode = new SvgNode(circle.Svg, "path");
      pathNode.TransferAttributesFrom(["fill", "stroke", "stroke-width"], circle);
      
      var pathData = CreateTransformedCirclePath(svgCircle).ToPathData(precision);
      pathNode.SetAttribute("d", pathData);
      
      circle.ReplaceWith(pathNode);
    }
  }
  
  static void ConvertEllipsesToPaths(IEnumerable<SvgNode> nodes, int precision = 3)
  {
    foreach(var ellipse in nodes)
    {
      var svgEllipse = new SvgEllipse
      {
        CenterX = float.Parse(ellipse.GetAttribute("cx")),
        CenterY = float.Parse(ellipse.GetAttribute("cy")),
        RadiusX = float.Parse(ellipse.GetAttribute("rx")),
        RadiusY = float.Parse(ellipse.GetAttribute("ry"))
      };
      
      if(ellipse.HasAttribute("transform"))
      {
        string transformAttribute = ellipse.GetAttribute("transform");
        svgEllipse.Transforms = ParseTransforms(transformAttribute);
      }
      
      // Now we have all we need to create a path
      // We'll create a new SvgNode of type "path", transfer basic attributes and set the path data
      var pathNode = new SvgNode(ellipse.Svg, "path");
      pathNode.TransferAttributesFrom(["fill", "stroke", "stroke-width"], ellipse);
      
      var pathData = CreateTransformedEllipsePath(svgEllipse).ToPathData(precision);
      pathNode.SetAttribute("d", pathData);
      
      ellipse.ReplaceWith(pathNode);
    }
  }

  static void ConvertRectanglesToPaths(IEnumerable<SvgNode> nodes, int precision = 3)
  {
    foreach(var rectangle in nodes)
    {
      var svgRectangle = new SvgRectangle
      {
        X = float.Parse(rectangle.GetAttribute("x")),
        Y = float.Parse(rectangle.GetAttribute("y")),
        Width = float.Parse(rectangle.GetAttribute("width")),
        Height = float.Parse(rectangle.GetAttribute("height"))
      };
      
      if(rectangle.HasAttribute("transform"))
      {
        string transformAttribute = rectangle.GetAttribute("transform");
        svgRectangle.Transforms = ParseTransforms(transformAttribute);
      }
      
      // Now we have all we need to create a path
      // We'll create a new SvgNode of type "path", transfer basic attributes and set the path data
      var pathNode = new SvgNode(rectangle.Svg, "path");
      pathNode.TransferAttributesFrom(["fill", "stroke", "stroke-width"], rectangle);
      
      var pathData = CreateTransformedRectanglePath(svgRectangle).ToPathData(precision);
      pathNode.SetAttribute("d", pathData);
      
      rectangle.ReplaceWith(pathNode);
      
    }
  }
  

  static SvgTransformCollection ParseTransforms(string transformAttribute)
  {
    var transforms = new SvgTransformCollection();
    var transformStrings = transformAttribute.Split(new[] { ')' }, StringSplitOptions.RemoveEmptyEntries);

    foreach(var transformString in transformStrings)
    {
      var parts = transformString.Trim().Split('(');
      if(parts.Length != 2) continue;

      var transformType = parts[0].Trim();
      var transformValues = parts[1].Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

      switch(transformType.ToLower())
      {
        case "translate":
          if(transformValues.Length >= 2)
          {
            float tx = float.Parse(transformValues[0]);
            float ty = float.Parse(transformValues[1]);
            transforms.Add(new SvgTranslate(tx, ty));
          }

          break;
        case "rotate":
          if(transformValues.Length >= 1)
          {
            float angle = float.Parse(transformValues[0]);
            if(transformValues.Length >= 3)
            {
              float cx = float.Parse(transformValues[1]);
              float cy = float.Parse(transformValues[2]);
              transforms.Add(new SvgRotate(angle, cx, cy));
            }
            else
            {
              transforms.Add(new SvgRotate(angle));
            }
          }

          break;
        case "scale":
          if(transformValues.Length >= 1)
          {
            float sx = float.Parse(transformValues[0]);
            float sy = transformValues.Length >= 2 ? float.Parse(transformValues[1]) : sx;
            transforms.Add(new SvgScale(sx, sy));
          }

          break;
        case "skewx":
          if(transformValues.Length >= 1)
          {
            float angle = float.Parse(transformValues[0]);
            transforms.Add(new SvgSkew(angle, 0));
          }

          break;
        case "skewy":
          if(transformValues.Length >= 1)
          {
            float angle = float.Parse(transformValues[0]);
            transforms.Add(new SvgSkew(0, angle));
          }

          break;
        case "matrix":
          if(transformValues.Length >= 6)
          {
            var matrixValues = new List<float>
            {
              float.Parse(transformValues[0]),
              float.Parse(transformValues[1]),
              float.Parse(transformValues[2]),
              float.Parse(transformValues[3]),
              float.Parse(transformValues[4]),
              float.Parse(transformValues[5])
            };
            transforms.Add(new SvgMatrix(matrixValues));
          }

          break;
      }
    }

    return transforms;
  }
 

  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  static float ParseFloat(string? value) => float.Parse(value ?? "0", CultureInfo.InvariantCulture);


  static SvgPathSegmentList CreateTransformedCirclePath(SvgCircle circle)
  {
    var matrix = GetTransformMatrix(circle);
    var center = TransformPoint(matrix, circle.CenterX, circle.CenterY);
    var right = TransformPoint(matrix, circle.CenterX + circle.Radius, circle.CenterY);
    var radius = Distance(center, right);

    return
    [
      new SvgMoveToSegment(false, center with { X = center.X - radius }),

      new SvgArcSegment(radius, radius, 0, SvgArcSize.Large, SvgArcSweep.Positive, false,
        center with { X = center.X + radius }),

      new SvgArcSegment(radius, radius, 0, SvgArcSize.Large, SvgArcSweep.Positive, false,
        center with { X = center.X - radius }),

      new SvgClosePathSegment(false)
    ];
  }

  
  static SvgPathSegmentList CreateTransformedRectanglePath(SvgRectangle rect)
  {
    var matrix = GetTransformMatrix(rect);
    var topLeft = TransformPoint(matrix, rect.X, rect.Y);
    var topRight = TransformPoint(matrix, rect.X + rect.Width, rect.Y);
    var bottomRight = TransformPoint(matrix, rect.X + rect.Width, rect.Y + rect.Height);
    var bottomLeft = TransformPoint(matrix, rect.X, rect.Y + rect.Height);

    return
    [
      new SvgMoveToSegment(false, topLeft),
      new SvgLineSegment(false, topRight),
      new SvgLineSegment(false, bottomRight),
      new SvgLineSegment(false, bottomLeft),
      new SvgClosePathSegment(false)
    ];
  }

  
  static SvgPathSegmentList CreateTransformedEllipsePath(SvgEllipse ellipse)
  {
    var matrix = GetTransformMatrix(ellipse);
    var center = TransformPoint(matrix, ellipse.CenterX, ellipse.CenterY);
    var right = TransformPoint(matrix, ellipse.CenterX + ellipse.RadiusX, ellipse.CenterY);
    var bottom = TransformPoint(matrix, ellipse.CenterX, ellipse.CenterY + ellipse.RadiusY);

    var radiusX = Distance(center, right);
    var radiusY = Distance(center, bottom);

    return
    [
      new SvgMoveToSegment(false, center with { X = center.X - radiusX }),

      new SvgArcSegment(radiusX, radiusY, 0, SvgArcSize.Large, SvgArcSweep.Positive, false,
        center with { X = center.X + radiusX }),

      new SvgArcSegment(radiusX, radiusY, 0, SvgArcSize.Large, SvgArcSweep.Positive, false,
        center with { X = center.X - radiusX }),

      new SvgClosePathSegment(false)
    ];
  }

  static Matrix GetTransformMatrix(SvgElement element)
  {
    var matrix = new Matrix();
    if(element.Transforms != null)
    {
      foreach(var transform in element.Transforms)
      {
        matrix.Multiply(transform.Matrix);
      }
    }

    return matrix;
  }

  static PointF TransformPoint(Matrix matrix, float x, float y)
  {
    PointF[] points = [new (x, y)];
    matrix.TransformPoints(points);
    return points[0];
  }

  static float Distance(PointF p1, PointF p2)
  {
    return (float)Math.Sqrt(Math.Pow(p2.X - p1.X, 2) + Math.Pow(p2.Y - p1.Y, 2));
  }

}