using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml;
using System.Xml.Linq;
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
  /// Gets the unique identifier of the SVG instance. Use it to identify the SVG for some later processing.
  /// </summary>
  public string Id { get; private set; } = string.Empty;

  /// <summary>
  /// Will remove the namespace from the SVG document upon serialization.
  /// </summary>
  public bool RemoveNamespace { get; set; }

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
  /// Initializes a new instance of the <see cref="Svg"/> class with the specified <see cref="XDocument"/>.
  /// </summary>
  /// <param name="svgDoc">The SVG document.</param>
  internal Svg(XDocument svgDoc)
  {
    Ast = new SvgAst(this, svgDoc);
  }

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
  /// Sets the unique identifier of the SVG instance.
  /// </summary>
  /// <param name="id">The unique identifier.</param>
  /// <returns>The current <see cref="Svg"/> instance.</returns>
  public Svg WithId(string id)
  {
    Id = id;
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
    var clone = new Svg(Ast.XDoc);

    if(string.IsNullOrEmpty(Id) == false)
    {
      clone.WithId(Id);
    }

    return clone;
  }

  /// <summary>
  /// Converts the SVG document to an <see cref="XDocument"/>.
  /// </summary>
  /// <returns>An <see cref="XDocument"/> representing the SVG document.</returns>
  public XDocument ToXDocument()
  {
    XDocument doc = new(
      Ast.Root.ToXElement(true)
    );

    return doc;
  }

  /// <summary>
  /// Saves the SVG document to a file.
  /// </summary>
  /// <param name="path">The path to save the file to.</param>
  /// <param name="formatting">The formatting options for the XML writer.</param>
  public void ToFile(string path, Formatting formatting = Formatting.Indented)
  {
    var doc = ToXDocument();

    var settings = new XmlWriterSettings
    {
      Encoding = Encoding.UTF8, Indent = formatting == Formatting.Indented, OmitXmlDeclaration = OmmitXmlDeclaration
    };

    using var writer = XmlWriter.Create(path, settings);
    doc.Save(writer);
  }

  /// <summary>
  /// Converts the SVG document to its string representation.
  /// </summary>
  /// <param name="formatting">The formatting options for the XML writer.</param>
  /// <returns>A string representing the SVG document.</returns>
  public string ToSvg(Formatting formatting = Formatting.Indented)
  {
    var settings = new XmlWriterSettings
    {
      Encoding = Encoding.UTF8, Indent = formatting == Formatting.Indented, OmitXmlDeclaration = OmmitXmlDeclaration
    };

    using var stringWriter = new StringWriter();
    using(var xmlWriter = XmlWriter.Create(stringWriter, settings))
    {
      var doc = new XDocument();
      var root = Ast.Root.ToXElement(true);
      doc.Add(root);
      doc.WriteTo(xmlWriter);
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
  /// Creates a new <see cref="Svg"/> instance from an <see cref="XDocument"/>.
  /// </summary>
  /// <param name="doc">The SVG document.</param>
  /// <returns>A new <see cref="Svg"/> instance.</returns>
  public static Svg FromXDocument(XDocument doc)
  {
    return new Svg(doc);
  }

  /// <summary>
  /// Creates a new <see cref="Svg"/> instance from a file.
  /// </summary>
  /// <param name="path">The path to the SVG file.</param>
  /// <returns>A new <see cref="Svg"/> instance.</returns>
  public static Svg FromFile(string path)
  {
    return new Svg(XDocument.Load(path));
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