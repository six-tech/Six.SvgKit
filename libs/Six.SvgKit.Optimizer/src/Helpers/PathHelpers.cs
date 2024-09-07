using System.Globalization;
using System.Text.RegularExpressions;
using Svg;
using Svg.Pathing;

namespace Six.SvgKit.Optimizer;

public static class SvgPathHelpers
{
  public static void MergePaths(List<SvgNode> pathNodes)
  {
    if(pathNodes.Count == 0) return;

    var pathGroups = pathNodes
      .GroupBy(GetPathStyleKey)
      .ToList();

    // Get first node and get it's parent. This parent will 
    // be used to append the merged path elements
    // If the node has no parent the passed node will be used as root
    var parentNode = pathNodes.FirstOrDefault()?.Parent
                     ?? throw new ArgumentException("First node doesn't have a parent node");

    // Iterate over each group of paths and combine the paths in the current group
    foreach(var pathGroup in pathGroups)
    {
      MergePathsInGroup(pathGroup, parentNode);
    }

    // Replace the original paths with the merged paths
    foreach(var nodeToDelete in pathNodes)
    {
      nodeToDelete.Remove();
    }
  }


  public static void RoundPathSegments(List<SvgNode> pathNodes, int decimalPlaces = 3)
  {
    foreach(var pathNode in pathNodes)
    {
      if(pathNode.Name.Equals("path", StringComparison.OrdinalIgnoreCase) == false) continue;

      var pathData = pathNode.Attributes.GetValueOrDefault("d");

      if(string.IsNullOrEmpty(pathData)) continue;

      var optimizedPathData = OptimizePath(pathData, decimalPlaces);
      pathNode.SetAttribute("d", optimizedPathData);
    }
  }


  static string OptimizePath(string pathData, int decimalPlaces = 3)
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



  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------

  static string GetPathStyleKey(SvgNode node)
  {
    // Generate a key based on fill and stroke properties
    var fill = node.Attributes.GetValueOrDefault("fill") ?? "none";
    var stroke = node.Attributes.GetValueOrDefault("stroke") ?? "none";
    return $"{fill}-{stroke}";
  }

  static SvgNode MergePathsInGroup(IGrouping<string, SvgNode> pathGroup, SvgNode parentNode)
  {
    // Create a new SvgPathSegmentList to store the merged path segments
    var mergedPathSegments = new SvgPathSegmentList();

    foreach(var pathNode in pathGroup)
    {
      // Get the path data and
      // Parse the path data into an SvgPathSegmentList
      // Add the parsed segments to the merged path segments list
      var pathData = pathNode.Attributes.GetValueOrDefault("d");
      var pathSegments = SvgPathBuilder.Parse(pathData);

      foreach(var segment in pathSegments)
      {
        mergedPathSegments.Add(segment);
      }
    }

    // Create a new path node with the merged path data
    var mergedPathNode = parentNode.CreateChild("path");
    mergedPathNode.SetAttribute("d", mergedPathSegments.ToString());

    // Copy the style attributes from the first path node in the group
    var firstPathElement = pathGroup.First();

    CopyAttributes(firstPathElement, mergedPathNode,
      ["fill", "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin"]);

    return mergedPathNode;
  }

  static void CopyAttributes(SvgNode source, SvgNode target, string[] attributeNames)
  {
    foreach(var attributeName in attributeNames)
    {
      var attributeValue = source.Attributes.GetValueOrDefault(attributeName);
      if(attributeValue != null)
      {
        target.SetAttribute(attributeName, attributeValue);
      }
    }
  }
}