namespace Six.SvgKit.Examples;

/// <summary>
/// Contains utility methods for file system operations related to SVG handling.
/// </summary>
public static class Helpers
{
  /// <summary>
  /// Searches for a specific directory by name, starting from the current directory and
  /// moving up the directory hierarchy until the specified directory is found or the root
  /// directory is reached.
  /// </summary>
  /// <param name="directoryName">The name of the directory to search for.</param>
  /// <returns>The full path to the found directory, or an empty string if the directory
  /// was not found.</returns>
  public static string FindDirectory(string directoryName)
  {
    if(string.IsNullOrWhiteSpace(directoryName))
    {
      throw new ArgumentException("Directory name cannot be null or empty.", nameof(directoryName));
    }

    DirectoryInfo? currentDir = new(Directory.GetCurrentDirectory());

    while(currentDir != null)
    {
      // Search for the directory in the current directory
      string? foundPath = SearchInCurrentDirectory(currentDir, directoryName);
      if(!string.IsNullOrEmpty(foundPath))
      {
        return foundPath;
      }

      // Move to the parent directory
      currentDir = currentDir.Parent;
    }

    return string.Empty;
  }


  /// <summary>
  /// Searches for a specific directory within the provided current directory.
  /// </summary>
  /// <param name="currentDir">The current directory to search in.</param>
  /// <param name="targetDirName">The name of the directory to search for.</param>
  /// <returns>The full path to the found directory, or null if the directory was not found.</returns>
  static string? SearchInCurrentDirectory(DirectoryInfo currentDir, string targetDirName)
  {
    try
    {
      foreach(var dir in currentDir.GetDirectories())
      {
        if(dir.Name.Equals(targetDirName, StringComparison.OrdinalIgnoreCase))
        {
          return dir.FullName;
        }
      }
    }
    catch(UnauthorizedAccessException)
    {
      // Skip directories we don't have access to
    }
    catch(IOException)
    {
      // Skip if there's an IO error
    }

    return null;
  }
}