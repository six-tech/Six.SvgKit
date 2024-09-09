using Six.SvgKit.Examples;

// Root directory where all svg files are stored or will be stored when generated
var svgRoot = Helpers.FindDirectory("svg");

Console.WriteLine("Optimize all SVG monochromatic icons in a directories");
Console.WriteLine("---------------------------------------------");
string rootImportDirectoryPath = Path.Combine(svgRoot, "input");
string rootExportDirectoryPath = Path.Combine(svgRoot, "output");
BatchOptimize.OptimizeDirectories(rootImportDirectoryPath, rootExportDirectoryPath);


Console.WriteLine("Create SVG from string and optimize it");
Console.WriteLine("---------------------------------------------");
var svgPath = Path.Combine(svgRoot, "from-string");
CreateSvgFromString.CreateSvgFromStringAndOptimize(svgPath);



