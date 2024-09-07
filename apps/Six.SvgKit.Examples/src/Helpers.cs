namespace Six.SvgKit.Examples;

public static class Helpers
{
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