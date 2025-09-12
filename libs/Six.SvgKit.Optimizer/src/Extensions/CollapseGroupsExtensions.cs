namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static class CollapseGroupsExtensions
{
  /// <summary>
  /// Finds groups that effectively do nothing and flattens them, preserving the contents of the groups.
  /// </summary>
  /// <param name="builder">The <see cref="SvgQueryBuilder"/> instance to operate on.</param>
  /// <returns>The modified <see cref="SvgQueryBuilder"/> instance.</returns>
  public static SvgQueryBuilder CollapseGroups(this SvgQueryBuilder builder)
  {
    var groups = builder
      .Select(n => n.Name == "g")
      .ToList();

    foreach(var group in groups)
    {
      CollapseGroupsInternal(group);
    }

    return builder;
  }

  static void CollapseGroupsInternal(SvgNode node)
  {
    if(node is { Name: "g", Children.Count: > 0 })
    {
      if(node.Attributes.Any() && node.Children.Count == 1)
      {
        var firstChild = node.Children[0];
        var nodeHasFilter = node.Attributes.ContainsKey("filter");
        var nodeHasClipPath = node.Attributes.ContainsKey("clip-path");
        var nodeHasMask = node.Attributes.ContainsKey("mask");

        if(firstChild.Name != "g"
           && firstChild.Attributes.ContainsKey("id") == false
           && !nodeHasFilter
           && (nodeHasClipPath == false && nodeHasMask == false))
        {
          foreach(var attr in node.Attributes)
          {
            if(firstChild.Attributes.ContainsKey(attr.Key) == false)
            {
              firstChild.SetAttribute(attr.Key, attr.Value);
            }
            else if(attr.Key == "transform")
            {
              firstChild.SetAttribute(attr.Key, attr.Value + " " + firstChild.Attributes[attr.Key]);
            }
            else if(firstChild.Attributes[attr.Key] == "inherit")
            {
              firstChild.SetAttribute(attr.Key, attr.Value);
            }
            else if(_inheritableAttributes.Contains(attr.Key) && firstChild.Attributes[attr.Key] != attr.Value)
            {
              firstChild.SetAttribute(attr.Key, attr.Value);
            }
          }

          node.ClearAttributes();
        }
      }

      if(node.Attributes.Any() == false)
      {
        var hasAnimationElement = node.Children.Any(child => _animationElements.Contains(child.Name));
        if(!hasAnimationElement && node.Parent is not null)
        {
          var index = node.Parent.Children.IndexOf(node);
          node.Parent.Children.InsertRange(index, node.Children);
          node.Parent.Children.Remove(node);
        }
      }
    }
    else
    {
      // Remove empty groups
      node.Parent?.Children.Remove(node);
    }
  }

  static readonly HashSet<string> _animationElements =
  [
    "animate",
    "animateColor",
    "animateMotion",
    "animateTransform",
    "set"
  ];

  static readonly HashSet<string> _inheritableAttributes =
  [
    "clip-rule",
    "color",
    "color-interpolation",
    "color-rendering",
    "cursor",
    "direction",
    "fill",
    "fill-opacity",
    "fill-rule",
    "font",
    "font-family",
    "font-size",
    "font-size-adjust",
    "font-stretch",
    "font-style",
    "font-variant",
    "font-weight",
    "letter-spacing",
    "marker",
    "marker-end",
    "marker-mid",
    "marker-start",
    "pointer-events",
    "shape-rendering",
    "stroke",
    "stroke-dasharray",
    "stroke-dashoffset",
    "stroke-linecap",
    "stroke-linejoin",
    "stroke-miterlimit",
    "stroke-opacity",
    "stroke-width",
    "text-anchor",
    "text-rendering",
    "transform",
    "vector-effect",
    "visibility",
    "word-spacing",
    "writing-mode"
  ];
}