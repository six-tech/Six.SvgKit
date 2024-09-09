using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class CleanupAttributesExtensions
{
  static readonly Regex _numberListRegex =
    new(@"(-?\d*\.?\d+(?:e[+-]?\d+)?)\s*,?\s*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

  static readonly Regex _transformRegex = new(@"(\w+)\s*\((.*?)\)", RegexOptions.Compiled);

  private static readonly Regex _colorRegex = new(@"^\s*(#[0-9A-Fa-f]{3,8}|(?:rgb|hsl)a?\s*\([^)]+\))\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
  private static readonly Regex _rgbColorRegex = new(@"rgb\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
  private static readonly Regex _rgbaColorRegex = new(@"rgba\s*\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([0-1](?:\.\d+)?)\s*\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

  public static SvgQueryBuilder CleanupAttributes(this SvgQueryBuilder builder)
  {
    builder
      .Select(_ => true)
      .SelectAttributes(_ => true, (node, key, value) =>
      {
        string cleanedValue = CleanAttribute(key, value);
        node.SetAttribute(key, cleanedValue);
      });

    return builder;
  }

  
  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
  
  static string CleanAttribute(string key, string value)
  {
    return key.ToLowerInvariant() switch
    {
      // Coordinate attributes
      "x" or "y" or "width" or "height" or "cx" or "cy" or "r" or "rx" or "ry"
        or "x1" or "y1" or "x2" or "y2" or "fx" or "fy" => CleanNumber(value),

      // Coordinate lists
      "viewbox" or "points" => CleanNumberList(value),

      // Path data
      "d" => CleanPathData(value),

      // Transform
      "transform" => CleanTransform(value),

      // Numeric attributes
      "opacity" or "fill-opacity" or "stroke-opacity" or "stroke-width"
        or "stroke-dashoffset" or "font-size" or "letter-spacing" => CleanNumber(value),

      // Percentage or number
      "offset" => CleanPercentageOrNumber(value),

      // Color attributes
      "fill" or "stroke" => CleanColor(value),

      // URL references
      "href" or "xlink:href" => CleanUrl(value),

      // Preserving attributes
      "preserveAspectRatio" => value.Trim(),

      // Default case: just trim whitespace
      _ => value.Trim()
    };
  }

  static string CleanNumber(string value)
  {
    if(double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double number))
    {
      return number.ToString("G", CultureInfo.InvariantCulture);
    }

    return value.Trim();
  }

  static string CleanNumberList(string value)
  {
    return string.Join(" ", _numberListRegex.Matches(value)
      .Select(m => double.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture)
        .ToString("G", CultureInfo.InvariantCulture)));
  }
  
  static string CleanPathData(string value)
  {
    // Remove any commas (just in case) and replace them with spaces
    string cleaned = value.Replace(",", " ");
    
    // Replace multiple spaces with a single space and trim
    cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();

    // Split the path data into commands and coordinates
    var parts = Regex.Split(cleaned, "(?=[MmLlHhVvCcSsQqTtAaZz])").Where(p => !string.IsNullOrWhiteSpace(p));

    var result = new StringBuilder();

    foreach (var part in parts)
    {
      char command = part[0];
      string coordinates = part.Substring(1).Trim();

      result.Append(command);
      if (!string.IsNullOrEmpty(coordinates))
      {
        result.Append(coordinates);
      }
    }

    return result.ToString();
  }

  static string CleanTransform(string value)
  {
    // Remove any newlines and extra spaces
    value = Regex.Replace(value, @"\s+", " ").Trim();

    // Handle each transform function separately
    var transformFunctions = Regex.Matches(value, @"(\w+)\s*\(([^)]+)\)");
    var result = new StringBuilder();

    foreach (Match match in transformFunctions)
    {
      string functionName = match.Groups[1].Value;
      string parameters = match.Groups[2].Value.Trim();

      // Special handling for translate function
      if (functionName.ToLower() == "translate")
      {
        parameters = Regex.Replace(parameters, @"\s*,\s*|\s+", ",");
      }
      else
      {
        parameters = Regex.Replace(parameters, @"\s+", " ");
      }

      result.Append($"{functionName}({parameters})");
    }

    return result.ToString();
  }

  static string CleanPercentageOrNumber(string value)
  {
    if(value.EndsWith("%"))
    {
      return CleanNumber(value.TrimEnd('%')) + "%";
    }

    return CleanNumber(value);
  }

  static string CleanColor(string value)
  {
    value = value.Trim().ToLowerInvariant();
    if (_colorRegex.IsMatch(value))
    {
      if (value.StartsWith("rgb("))
      {
        return _rgbColorRegex.Replace(value, match => 
          $"rgb({match.Groups[1].Value},{match.Groups[2].Value},{match.Groups[3].Value})");
      }
      else if (value.StartsWith("rgba("))
      {
        return _rgbaColorRegex.Replace(value, match => 
          $"rgba({match.Groups[1].Value},{match.Groups[2].Value},{match.Groups[3].Value},{match.Groups[4].Value})");
      }
      return value;
    }
    // Handle named colors or other color formats here if needed
    return value;
  }

  static string CleanUrl(string value)
  {
    value = value.Trim();
    if(value.StartsWith("url(") && value.EndsWith(")"))
    {
      return $"url({value.Substring(4, value.Length - 5).Trim()})";
    }

    return value;
  }
}