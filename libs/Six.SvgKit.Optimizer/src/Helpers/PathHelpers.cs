using System.Text;

namespace Six.SvgKit.Optimizer;

/// <summary>
/// Provides helper methods for SVG path data manipulation.
/// </summary>
public static class PathHelpers
{
  /// <summary>
  /// Merges a list of SVG path data strings into a single path data string.
  /// </summary>
  /// <example>
  /// <para>How it works:</para>
  /// <para>1. Extracts the 'd' attribute from each path.</para>
  /// <para>2. For each path after the first: </para>
  /// <para>2.1. If it doesn't start with an "M" command, prepend "M".</para>
  /// <para>2.2. Add its data to the combined path data.</para>
  /// <para>3. Join all path data with spaces between them.</para>
  /// </example>
  /// <remarks>
  /// <para><b>This approach has limitations and it's suitable for simple vectors like monochromatic icons</b></para>
  /// <para>1. It doesn't handle different fill/stroke properties for different paths.</para>
  /// <para>2. It may not be suitable for paths that rely on their order for layering effects.</para>
  /// <para>3. Some complex SVG features (like clipping paths) might not work correctly after merging.</para>
  /// </remarks>
  /// <param name="pathDatas">A list of SVG path data strings to merge.</param>
  /// <returns>A single merged SVG path data string.</returns>
  public static string MergePaths(List<string> pathDatas)
  {
    if (pathDatas.Count == 0) return string.Empty;

    var mergedPath = new StringBuilder(pathDatas[0].Trim());

    for (int i = 1; i < pathDatas.Count; i++)
    {
      string pathData = pathDatas[i].Trim();
      if (!pathData.StartsWith("M", StringComparison.OrdinalIgnoreCase))
      {
        mergedPath.Append(" M");
      }

      mergedPath.Append(' ').Append(pathData);
    }

    return mergedPath.ToString();
  }
}