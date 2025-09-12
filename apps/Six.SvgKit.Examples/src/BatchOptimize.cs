using Six.SvgKit.Optimizer;

namespace Six.SvgKit.Examples;

[PublicAPI]
[ExcludeFromCodeCoverage]
public static class BatchOptimize
{
  /// <summary>
  /// Optimizes all SVG files within the source directory and its subdirectories,
  /// and outputs the optimized files to the corresponding directories within the destination directory.
  /// </summary>
  /// <param name="sourceSvgDirectoryPath">The path to the directory containing the SVG files to be optimized.</param>
  /// <param name="destinationSvgDirectoryPath">The path to the directory where the optimized SVG files will be saved.</param>
  public static void OptimizeDirectories(string sourceSvgDirectoryPath, string destinationSvgDirectoryPath)
  {
    // Ensure output directory exists
    Directory.CreateDirectory(destinationSvgDirectoryPath);
    
    // Get all directories
    var directories = Directory.GetDirectories(sourceSvgDirectoryPath, "*", SearchOption.AllDirectories);
    foreach(string directory in directories)
    {
      var directoryName = Path.GetFileName(directory);
      
      // Prepare output directory
      var outputDirectoryPath = Path.Combine(destinationSvgDirectoryPath, directoryName);
      Directory.CreateDirectory(outputDirectoryPath);
      
      OptimizeDirectory(directory, outputDirectoryPath);
    }
  }

  
  /// <summary>
  /// Optimizes all SVG files within the input directory and outputs the optimized files to the output directory.
  /// </summary>
  /// <param name="inputDirectoryPath">The path to the directory containing the SVG files to be optimized.</param>
  /// <param name="outputDirectoryPath">The path to the directory where the optimized SVG files will be saved.</param>
  public static void OptimizeDirectory(string inputDirectoryPath, string outputDirectoryPath)
  {
    string[] files = Directory.GetFiles(inputDirectoryPath, "*.svg", SearchOption.AllDirectories);

    // Run optimization in parallel
    Parallel.ForEach(files, file =>
    {
      try
      {
        Svg svg = Svg.FromFile(file);
        string filename = Path.GetFileName(file);
        string outputPath = Path.Combine(outputDirectoryPath, filename);

        svg.Query()
          .MonoIconsOptimizations()
          .Build()
          .SizeComparisonInPercentage(percentage =>
          {
            Console.WriteLine($"File: {filename}, Percentage of size reduction: {percentage} %");
          })
          .SizeComparisonInBytes((before, after) =>
          {
            Console.WriteLine($"File: {filename}, Byte size before: {before}");
            Console.WriteLine($"File: {filename}, Byte size after: {after}");
          })
          .ToFile(outputPath);
      }
      catch(Exception ex)
      {
        Console.WriteLine($"Error processing file {file}: {ex.Message}");
      }
    });
  }
  
}