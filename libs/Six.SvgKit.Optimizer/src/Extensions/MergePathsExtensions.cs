using Svg;
using Svg.Pathing;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class MergePathsExtensions
{
  /// <summary>
  /// Merges multiple SVG path elements into a single path element within the SVG document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder MergePaths(this SvgQueryBuilder builder)
  {
    var pathNodes = builder
      .Select(n => n.Name == "path")
      .ToList();
    
    if(pathNodes.Count == 0) return builder;

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

    return builder.Continue();
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

    // Transfer the style attributes from the first path node in the group
    mergedPathNode.TransferAttributesFrom(
      ["fill", "stroke", "stroke-width", "stroke-linecap", "stroke-linejoin"], pathGroup.First());
    

    return mergedPathNode;
  }
  
}