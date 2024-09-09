using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using AngleSharp.Html;
using JetBrains.Annotations;

namespace Six.SvgKit;

[PublicAPI]
[ExcludeFromCodeCoverage]
public class Svg
{
  // MEMBERS
  internal readonly SvgAst Ast;

  #region PROPERTIES

  /// <summary>
  /// Gets the SVG metadata. Useful for identifying the SVG instance while processing multiple SVGs.
  /// </summary>
  public SvgMetadata? Metadata { get; private set; }

  /// <summary>
  /// Will remove the XML declaration from the SVG document upon serialization.
  /// </summary>
  public bool OmmitXmlDeclaration { get; set; }

  /// <summary>
  /// Will compress the SVG document upon serialization.
  /// This will remove all unnecessary whitespaces, line breaks, etc.
  /// </summary>
  public bool CompressOutput { get; set; }

  #endregion

  // CTOR / DISPOSAL
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Initializes a new instance of the <see cref="Svg"/> class with the specified SVG string.
  /// </summary>
  /// <param name="svgString">The SVG string.</param>
  internal Svg(string svgString)
  {
    Ast = new SvgAst(this, svgString);
  }

  
  // PUBLIC METHODS
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Sets the SVG metadata. Useful for identifying the SVG instance while processing multiple SVGs.
  /// </summary>
  /// <param name="metadata">The <see cref="SvgMetadata"/> instance.</param>
  /// <returns>The current <see cref="Svg"/> instance.</returns>
  public Svg WithMetadata(SvgMetadata metadata)
  {
    Metadata = metadata;
    return this;
  }

  /// <summary>
  /// Creates a new <see cref="SvgQueryBuilder"/> for querying the SVG document.
  /// </summary>
  /// <returns>A new <see cref="SvgQueryBuilder"/> instance.</returns>
  public SvgQueryBuilder Query()
  {
    return new SvgQueryBuilder(this);
  }

  /// <summary>
  /// Clones the current SVG document by using the same <see cref="XDocument"/> instance.
  /// </summary>
  /// <returns>Returns a newly created <see cref="Svg"/> instance.</returns>
  public Svg Clone()
  {
    var clone = new Svg(ToSvg());

    if(Metadata is not null)
    {
      clone.WithMetadata(Metadata);
    }

    return clone;
  }

  
  /// <summary>
  /// Saves the SVG document to a file.
  /// </summary>
  /// <param name="path">The path to save the file to.</param>
  public void ToSvgFile(string path)
  {
    try
    {
      // Write the SVG content to the file
      File.WriteAllText(path, ToSvg(), Encoding.UTF8);
    }
    catch (Exception ex)
    {
      Console.WriteLine($"An error occurred while saving {path}: {ex.Message}");
    }
  }

  
  /// <summary>
  /// Converts the SVG document to its string representation.
  /// </summary>
  /// <param name="formatting">The formatting options for the XML writer.</param>
  /// <returns>A string representing the SVG document.</returns>
  public string ToSvg(Formatting formatting = Formatting.Indented)
  {
    using var stringWriter = new StringWriter();
    
    var doc = Ast.Root.ToAngleSharpElement();
    if(doc is null)
    {
      throw new NullReferenceException("Failed to convert the SVG document to an AngleSharp element");
    }
    
    if (CompressOutput)
    {
      // Minify the output using a custom formatter
      var formatter = new MinifyMarkupFormatter
      {
        ShouldKeepAttributeQuotes = true,
      };
      doc.ToHtml(stringWriter, formatter);
    }
    else
    {
      // Unminified output
      var formatter = new PrettyMarkupFormatter();
      doc.ToHtml(stringWriter, formatter);
    }

    return stringWriter.ToString();
  }

  
  /// <summary>
  /// Converts the SVG document to a byte array.
  /// </summary>
  /// <returns>A byte array representing the SVG document.</returns>
  public byte[] ToBytes() => Encoding.UTF8.GetBytes(ToSvg());

  
  // PUBLIC STATIC METHODS (FACTORY)
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Creates a new <see cref="Svg"/> instance from a file.
  /// </summary>
  /// <param name="path">The path to the SVG file.</param>
  /// <returns>A new <see cref="Svg"/> instance.</returns>
  public static Svg FromFile(string path)
  {
    // Check if the file exists
    if (!File.Exists(path))
    {
      throw new FileNotFoundException($"The file at path '{path}' does not exist.");
    }

    // Read the entire content of the SVG file into a string
    return new Svg(File.ReadAllText(path));
  }

  /// <summary>
  /// Creates a new <see cref="Svg"/> instance from a string.
  /// </summary>
  /// <param name="svgString">The SVG string.</param>
  /// <returns>A new <see cref="Svg"/> instance.</returns>
  public static Svg FromString(string svgString)
  {
    return new Svg(svgString);
  }
}