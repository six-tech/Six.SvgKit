using System.Drawing;
using System.Drawing.Drawing2D;
using ImageMagick;
using Svg;

namespace Six.SvgKit;

[PublicAPI]
public static class ToBitmapFileExtensions
{
  /// <summary>
  /// Converts the given SVG object to a bitmap and saves it in the specified format.
  /// </summary>
  /// <param name="svg">The SVG object to be converted.</param>
  /// <param name="format">The desired bitmap format.</param>
  /// <param name="path">The file path where the bitmap will be saved.</param>
  /// <param name="width">The width of the output bitmap.</param>
  /// <param name="height">The height of the output bitmap.</param>
  /// <param name="dpi">The resolution of the output bitmap in dots per inch. Default is 96.</param>
  /// <param name="quality">The quality level of the output bitmap. Default is 75.</param>
  /// <returns>Returns the original SVG object.</returns>
  public static Svg ToBitmapFile(
    this Svg svg, MagickFormat format, string path, int width, int height, int dpi = 96, uint quality = 75)
  {
    var svgDocument = SvgDocument.FromSvg<SvgDocument>(svg.ToSvg());

    // ReSharper disable once ConvertToUsingDeclaration
    using(var bitmap = new Bitmap(width, height))
    {
      bitmap.SetResolution(dpi, dpi);

      using(var graphics = Graphics.FromImage(bitmap))
      {
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.CompositingQuality = CompositingQuality.HighQuality;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var renderer = SvgRenderer.FromGraphics(graphics);

        // Calculate the scaling factor to fit the SVG within the specified width and height
        float scaleX = width / svgDocument.Width.Value;
        float scaleY = height / svgDocument.Height.Value;
        float scale = Math.Min(scaleX, scaleY);

        // Set the bounds of the renderer to match the scaled SVG dimensions
        var bounds = new SvgBoundable(0, 0, svgDocument.Width.Value * scale, svgDocument.Height.Value * scale);
        renderer.SetBoundable(bounds);

        // Scale the graphics to fit the SVG within the bitmap
        graphics.ScaleTransform(scale, scale);

        svgDocument.Draw(renderer);
      }

      // Save the bitmap as WebP using Magick.NET
      using(var memoryStream = new MemoryStream())
      {
        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
        memoryStream.Position = 0; // Reset the stream position to the beginning to be able to save with Magick.NET

        using(var image = new MagickImage(memoryStream))
        {
          image.Format = format;
          image.Quality = quality;
          image.Write(path);
        }
      }
    }

    return svg;
  }
}