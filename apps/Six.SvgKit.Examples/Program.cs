using Six.SvgKit;
using Six.SvgKit.Optimizer;

Console.WriteLine("Create SVG from string and save it to file");
Console.WriteLine("---------------------------------------------");
string svgString = @"<svg xmlns=""http://www.w3.org/2000/svg"" viewBox=""0 0 24 24"">
    <path d=""M0 0h24v24H0V0z"" fill=""none""/>
    <path d=""M10 10.5h1.5v3H10zM19 3H5c-1.1 0-2 .9-2 2v14c0 1.1.9 2 2 2h14c1.1 0 2-.9 2-2V5c0-1.1-.9-2-2-2zM7.5 15H6v-4.5H4.5V9h3v6zm5.5-1c0 .55-.45 1-1 1H9.5c-.55 0-1-.45-1-1v-4c0-.55.45-1 1-1H12c.55 0 1 .45 1 1v4zm6.5 1h-1.75L16 12.75V15h-1.5V9H16v2.25L17.75 9h1.75l-2.25 3 2.25 3z""/>
</svg>";

Svg
  .FromString(svgString)
  .Query()
  .Build()
  .ToFile("../../../../test.svg");


Console.WriteLine("Create SVG from string and optimize it");
Console.WriteLine("---------------------------------------------");

Svg
  .FromString(svgString)
  .Query()
  .RemoveEmptyContainers()
  .RemoveAttribute("viewBox")
  .Build()
  .SizeComparisonInPercentage(percentage =>
  {
    Console.WriteLine($"Percentage of size reduction: {percentage} %");
  })
  .SizeComparisonInBytes((before, after) =>
  {
    Console.WriteLine($"Byte size before: {before}");
    Console.WriteLine($"Byte size after: {after}");
  })
  .ToFile("../../../../test-optimize.svg");


Svg
  .FromString(svgString)
  .Query()
  .RemoveXmlDeclaration()
  .RemoveEmptyContainers()
  .RemoveViewBoxAttribute()
  .MergePaths()
  .Build()
  .SizeComparisonInPercentage(percentage =>
  {
    Console.WriteLine($"Percentage of size reduction: {percentage} %");
  })
  .SizeComparisonInBytes((before, after) =>
  {
    Console.WriteLine($"Byte size before: {before}");
    Console.WriteLine($"Byte size after: {after}");
  })
  .ToFile("../../../../test-merge-paths.svg");


// To SVG string
var result = Svg
  .FromString(svgString)
  .Query()
  .MergePaths()
  .RemoveEmptyContainers()
  .RemoveViewBoxAttribute()
  .RemoveNamespace()
  .Build()
  .SizeComparisonInPercentage(percentage =>
  {
    Console.WriteLine($"Percentage of size reduction: {percentage} %");
  })
  .SizeComparisonInBytes((before, after) =>
  {
    Console.WriteLine($"Byte size before: {before}");
    Console.WriteLine($"Byte size after: {after}");
  });
  
  
  //.ToSvg();
Console.WriteLine($"SVG string is: {result}");