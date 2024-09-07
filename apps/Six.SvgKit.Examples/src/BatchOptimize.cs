using Six.SvgKit.Optimizer;

namespace Six.SvgKit.Examples;

public static class BatchOptimize
{
  public static void OptimizeDirectory()
  {
    string rootSvgDirectoryPath = Helpers.FindDirectory("svg");
    string inputDirectoryPath = Path.Combine(rootSvgDirectoryPath, "input");
    string outputDirectoryPath = Path.Combine(rootSvgDirectoryPath, "output");

    // Ensure output directory exists
    Directory.CreateDirectory(outputDirectoryPath);

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