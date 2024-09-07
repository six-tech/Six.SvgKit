using AngleSharp;
using AngleSharp.Dom;
using Svg;
using Svg.Pathing;

namespace Six.SvgKit.Optimizer;

public static class SvgPathMerger
{
  public static string MergePaths(string svgContent)
  {
    var config = Configuration.Default.WithXml();
    using var context = BrowsingContext.New(config);
    using var document = context.OpenAsync(res => res.Content(svgContent).Header("Content-Type", "text/xml")).Result;
    
    var pathElements = document.QuerySelectorAll("path");

    // Group paths by fill and stroke
    var pathGroups = pathElements.GroupBy(GetPathStyleKey);
    var mergedPaths = new List<IElement>();

    // Iterate over each group of paths and combine the paths in the current group
    foreach(var pathGroup in pathGroups)
    {
      var mergedPath = MergePathsInGroup(pathGroup, document);
      mergedPaths.Add(mergedPath);
    }

    // Replace the original paths with the merged paths
    foreach(var pathElement in pathElements)
    {
      pathElement.Remove();
    }

    var svgElement = document.QuerySelector("svg");
    foreach(var mergedPath in mergedPaths)
    {
      svgElement?.AppendChild(mergedPath);
    }

    // Return the updated SVG content
    return document.ToHtml();
  }

  static string GetPathStyleKey(IElement pathElement)
  {
    // Generate a key based on fill and stroke properties
    var fill = pathElement.GetAttribute("fill") ?? "none";
    var stroke = pathElement.GetAttribute("stroke") ?? "none";
    return $"{fill}-{stroke}";
  }

  static IElement MergePathsInGroup(IGrouping<string, IElement> pathGroup, IDocument document)
  {
    // Create a new SvgPathSegmentList to store the merged path segments
    var mergedPathSegments = new SvgPathSegmentList();

    // Iterate over each path element in the group
    foreach(var pathElement in pathGroup)
    {
      // Get the path data
      var pathData = pathElement.GetAttribute("d");

      // Parse the path data into an SvgPathSegmentList
      var pathSegments = SvgPathBuilder.Parse(pathData);

      // Add the parsed segments to the merged path segments list
      foreach(var segment in pathSegments)
      {
        mergedPathSegments.Add(segment);
      }
    }

    // Create a new path element with the merged path data
    var mergedPathElement = document.CreateElement("path");
    mergedPathElement.SetAttribute("d", mergedPathSegments.ToString());

    // Copy the style attributes from the first path element in the group
    var firstPathElement = pathGroup.First();
    CopyAttributes(firstPathElement, mergedPathElement,
      ["fill", "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin"]);

    return mergedPathElement;
  }

  static void CopyAttributes(IElement source, IElement target, string[] attributeNames)
  {
    foreach(var attributeName in attributeNames)
    {
      var attributeValue = source.GetAttribute(attributeName);
      if(attributeValue != null)
      {
        target.SetAttribute(attributeName, attributeValue);
      }
    }
  }
}