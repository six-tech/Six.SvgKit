using System.Globalization;
using System.Text.RegularExpressions;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class RoundPathSegmentsExtensions
{
  public static SvgQueryBuilder RoundPathSegments(this SvgQueryBuilder builder, int decimalPlaces = 3)
  {
    var pathNodes = builder
      .Select(n => n.Name == "path")
      .ToList();
    
    foreach(var pathNode in pathNodes)
    {
      if(pathNode.Name.Equals("path", StringComparison.OrdinalIgnoreCase) == false) continue;

      var pathData = pathNode.Attributes.GetValueOrDefault("d");

      if(string.IsNullOrEmpty(pathData)) continue;

      var optimizedPathData = RoundPathValues(pathData, decimalPlaces);
      pathNode.SetAttribute("d", optimizedPathData);
    }

    return builder;
  }
  
  
  static string RoundPathValues(string pathData, int decimalPlaces = 3)
  {
    // This regex matches numbers (including scientific notation)
    string pattern = @"(-?\d*\.?\d+(?:[eE][-+]?\d+)?)";
        
    return Regex.Replace(pathData, pattern, match =>
    {
      if (float.TryParse(match.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out float number))
      {
        // Round to the specified number of decimal places
        return Math.Round(number, decimalPlaces).ToString($"F{decimalPlaces}", CultureInfo.InvariantCulture).TrimEnd('0').TrimEnd('.');
      }
      return match.Value; // If parsing fails, return the original value
    });
  }
}