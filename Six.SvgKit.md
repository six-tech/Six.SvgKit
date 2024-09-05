<!--
  Metadata:
    - Project: 
    - File: CHANGELOG.md
-->


<!--
  Metadata:
    - Project: 
    - File: README.md
-->
# Library

<!--
  Metadata:
    - Project: Six.SvgKit.Examples
    - File: apps\Six.SvgKit.Examples\Program.cs
-->
```
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
  .GetSizeComparisonInPercentage(percentage =>
  {
    Console.WriteLine($"Percentage of size reduction: {percentage} %");
  })
  .GetSizeComparisonInBytes((before, after) =>
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
  .GetSizeComparisonInPercentage(percentage =>
  {
    Console.WriteLine($"Percentage of size reduction: {percentage} %");
  })
  .GetSizeComparisonInBytes((before, after) =>
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
  .GetSizeComparisonInPercentage(percentage =>
  {
    Console.WriteLine($"Percentage of size reduction: {percentage} %");
  })
  .GetSizeComparisonInBytes((before, after) =>
  {
    Console.WriteLine($"Byte size before: {before}");
    Console.WriteLine($"Byte size after: {after}");
  })
  .ToSvg();
Console.WriteLine($"SVG string is: {result}");
```

<!--
  Metadata:
    - Project: Six.SvgKit
    - File: libs\Six.SvgKit\README.md
-->
# Library

<!--
  Metadata:
    - Project: Six.SvgKit
    - File: libs\Six.SvgKit\src\Svg.cs
-->
```
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
```

<!--
  Metadata:
    - Project: Six.SvgKit
    - File: libs\Six.SvgKit\src\SvgAst.cs
-->
```
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Six.SvgKit;

[PublicAPI]
public class SvgAst
{
  // MEMBERS
  internal readonly XDocument XDoc;

  #region PROPERTIES
  
  /// <summary>
  /// Gets the SVG object associated with this instance.
  /// </summary>
  public Svg Svg { get; }

  /// <summary>
  /// Gets the root node of the SVG abstract syntax tree (AST).
  /// </summary>
  public SvgNode Root { get; }

  #endregion
  
  
  // CTOR / DISPOSAL
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Initializes a new instance of the <see cref="SvgAst"/> class using an SVG object and an XDocument.
  /// </summary>
  /// <param name="svg">The SVG object.</param>
  /// <param name="svgDoc">The XDocument representing the SVG document.</param>
  /// <exception cref="ArgumentException">Thrown when the SVG document or its root is null.</exception>
  public SvgAst(Svg svg, XDocument svgDoc)
  {
    if(svgDoc is null || svgDoc.Root is null)
    {
      throw new ArgumentException("SVG document is null reference", nameof(svgDoc));
    }

    Svg = svg;
    XDoc = svgDoc;
    Root = BuildAst(Svg, XDoc.Root);
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="SvgAst"/> class using an SVG object and an SVG string.
  /// </summary>
  /// <param name="svg">The SVG object.</param>
  /// <param name="svgString">The string representation of the SVG document.</param>
  /// <exception cref="ArgumentException">Thrown when the SVG document or its root is null.</exception>
  public SvgAst(Svg svg, string svgString)
  {
    XDoc = XDocument.Parse(svgString);

    if(XDoc is null || XDoc.Root is null)
    {
      throw new ArgumentException("SVG document is null reference", nameof(XDoc));
    }

    Svg = svg;
    Root = BuildAst(Svg, XDoc.Root);
  }


  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Builds the abstract syntax tree (AST) for the SVG document.
  /// </summary>
  /// <param name="svg">The SVG object.</param>
  /// <param name="element">The root XElement of the SVG document.</param>
  /// <returns>The root node of the SVG AST.</returns>
  SvgNode BuildAst(Svg svg, XElement element)
  {
    SvgNode node = new(svg, element.Name.LocalName);

    foreach(var attribute in element.Attributes())
    {
      node.SetAttribute(attribute.Name.LocalName, attribute.Value);
    }

    foreach(var childElement in element.Elements())
    {
      node.AddChild(BuildAst(svg, childElement));
    }

    return node;
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit
    - File: libs\Six.SvgKit\src\SvgNode.cs
-->
```
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Six.SvgKit;

/// <summary>
/// Represents a node in the SVGKit AST.
/// </summary>
/// <para>
/// Each svg xml element will be represented as a <see cref="SvgNode"/> instance.
/// </para>
[PublicAPI]
public record SvgNode
{
  static readonly XNamespace _svgNamespace = "http://www.w3.org/2000/svg";

  #region PROPERTIES

  /// <summary>
  /// Gets the unique identifier of the SVG node.
  /// </summary>
  public Guid Guid { get; } = Guid.NewGuid();

  /// <summary>
  /// Owner SVG document
  /// </summary>
  public Svg Svg { get; private set; }

  /// <summary>
  /// Parent <see cref="SvgNode"/> instance.
  /// </summary>
  public SvgNode? Parent { get; private set; }

  /// <summary>
  /// Child <see cref="SvgNode"/> instances
  /// </summary>
  public List<SvgNode> Children { get; private set; } = [];

  /// <summary>
  /// If this note is root node of SVG document.
  /// </summary>
  public bool IsRoot => Parent is null && Name == "svg";

  /// <summary>
  /// Node name.
  /// </summary>
  public string Name { get; }

  /// <summary>
  /// <see cref="Dictionary{TKey,TValue}"/> of node attributes.
  /// </summary>
  public Dictionary<string, string> Attributes { get; } = [];

  #endregion


  // CTOR / DISPOSAL
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Initializes a new instance of the <see cref="SvgNode"/> class with the specified SVG document and node name.
  /// </summary>
  /// <param name="svg">The owner SVG document.</param>
  /// <param name="name">The name of the node.</param>
  public SvgNode(Svg svg, string name)
  {
    Svg = svg;
    Name = name;
  }


  // PUBLIC METHODS
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Adds a child node to this node.
  /// </summary>
  /// <param name="child">The child node to add.</param>
  public void AddChild(SvgNode child)
  {
    Children.Add(child);
    child.OnAddedToParent(this);
  }

  /// <summary>
  /// Sets an attribute on this node.
  /// </summary>
  /// <param name="key">The attribute name.</param>
  /// <param name="value">The attribute value.</param>
  public void SetAttribute(string key, string value)
  {
    Attributes[key] = value;
  }

  /// <summary>
  /// Removes an attribute from this node.
  /// </summary>
  /// <param name="key">The attribute name.</param>
  public void RemoveAttribute(string key)
  {
    Attributes.Remove(key);
  }

  /// <summary>
  /// Converts this node and its children to an <see cref="XElement"/>.
  /// </summary>
  /// <param name="isRoot">Indicates whether this node is the root of the SVG document.</param>
  /// <returns>An <see cref="XElement"/> representing this node and its children.</returns>
  public XElement ToXElement(bool isRoot = false)
  {
    XElement element;
    XNamespace svgNs = "http://www.w3.org/2000/svg";

    // Deal with root nodes and other nodes separately
    // On root note add/remove namespace attribute if needed (Svg.RemoveNamespace)
    if(isRoot)
    {
      if(Svg.RemoveNamespace)
      {
        element = new XElement("svg");
      }
      else
      {
        element = new XElement(svgNs + "svg");
        element.Add(new XAttribute("xmlns", svgNs.NamespaceName));
      }
    }
    else
    {
      element = new XElement(Svg.RemoveNamespace ? Name : svgNs + Name);
    }

    foreach(var attr in Attributes)
    {
      if(attr.Key != "xmlns") // Avoid adding xmlns attribute to non-root elements
      {
        element.SetAttributeValue(attr.Key, attr.Value);
      }
    }

    foreach(var child in Children)
    {
      element.Add(child.ToXElement());
    }

    return element;
  }

  /// <summary>
  /// Determines whether the specified <see cref="SvgNode"/> is equal to the current <see cref="SvgNode"/>.
  /// </summary>
  /// <param name="other">The <see cref="SvgNode"/> to compare with the current <see cref="SvgNode"/>.</param>
  /// <returns><c>true</c> if the specified <see cref="SvgNode"/> is equal to the current <see cref="SvgNode"/>; otherwise, <c>false</c>.</returns>
  public virtual bool Equals(SvgNode? other)
  {
    if(other is null)
      return false;

    if(other.Guid != Guid)
      return false;

    if(ReferenceEquals(this, other))
      return true;

    if(Name != other.Name || Attributes.Count != other.Attributes.Count)
      return false;

    foreach(var kvp in Attributes)
    {
      if(!other.Attributes.TryGetValue(kvp.Key, out var otherValue) || kvp.Value != otherValue)
        return false;
    }

    return true;
  }

  /// <summary>
  /// Serves as the default hash function.
  /// </summary>
  /// <returns>A hash code for the current <see cref="SvgNode"/>.</returns>
  public override int GetHashCode()
  {
    unchecked
    {
      int hash = 17;
      hash = hash * 23 + Name.GetHashCode();
      foreach(var kvp in Attributes)
      {
        hash = hash * 23 + kvp.Key.GetHashCode();
        hash = hash * 23 + kvp.Value.GetHashCode();
      }

      return hash;
    }
  }


  // INTERNAL METHODS
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Called when this node is added to a parent node.
  /// </summary>
  /// <param name="parent">The parent node.</param>
  internal void OnAddedToParent(SvgNode parent)
  {
    Parent = parent;
  }

  /// <summary>
  /// Sets the children of this node.
  /// </summary>
  /// <param name="children">The list of child nodes.</param>
  internal void OnSetChildern(List<SvgNode> children)
  {
    Children = children;
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit
    - File: libs\Six.SvgKit\src\SvgQueryBuilder.cs
-->
```
using JetBrains.Annotations;

namespace Six.SvgKit;

/// <summary>
/// Provides functionality to build and manipulate SVG documents.
/// </summary>
[PublicAPI]
public class SvgQueryBuilder
{
  // MEMBERS
  readonly Svg _originalSvg; // The original SVG object. The new one is a clone  of this(one on which work will performed).

  
  #region PROPERTIES


  /// <summary>
  /// Gets the current SVG object being manipulated.
  /// </summary>
  public Svg Svg { get; }

  /// <summary>
  /// Gets the abstract syntax tree (AST) of the current SVG object.
  /// </summary>
  public SvgAst Ast => Svg.Ast;

  /// <summary>
  /// Gets or sets the predicate function used to filter SVG nodes.
  /// </summary>
  public Func<SvgNode, bool> Predicate { get; set; }

  #endregion
  
  
  // CTOR / DISPOSAL
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Initializes a new instance of the <see cref="SvgQueryBuilder"/> class with the specified SVG object.
  /// </summary>
  /// <param name="svg">The SVG object to manipulate.</param>
  public SvgQueryBuilder(Svg svg)
  {
    _originalSvg = svg; // Store the original SVG
    Svg = svg.Clone();
    Predicate = _ => true;
  }


  // PUBLIC METHODS
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Builds the result of the SVG query.
  /// </summary>
  /// <returns>A <see cref="SvgResult"/> containing the original and modified SVG objects.</returns>
  public SvgResult Build()
  {
    return new SvgResult(_originalSvg, Svg);
  }

  /// <summary>
  /// Selects SVG nodes that match the specified predicate.
  /// </summary>
  /// <param name="predicate">The predicate function to filter SVG nodes.</param>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance.</returns>
  public SvgQueryBuilder Select(Func<SvgNode, bool> predicate)
  {
    Predicate = predicate;
    return this;
  }

  /// <summary>
  /// Filters SVG nodes based on the specified predicate.
  /// </summary>
  /// <param name="predicate">The predicate function to filter SVG nodes.</param>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance.</returns>
  public SvgQueryBuilder Where(Func<SvgNode, bool> predicate)
  {
    var oldPredicate = Predicate;
    Predicate = node => oldPredicate(node) && predicate(node);
    return this;
  }

  /// <summary>
  /// Removes SVG nodes that match the current predicate.
  /// </summary>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance.</returns>
  public SvgQueryBuilder RemoveNodes()
  {
    Ast.Root.OnSetChildern(Ast.Root.Children.Where(child => !Predicate(child)).ToList());
    return this;
  }

  /// <summary>
  /// Sets an attribute on SVG nodes that match the current predicate.
  /// </summary>
  /// <param name="key">The attribute name.</param>
  /// <param name="value">The attribute value.</param>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance.</returns>
  public SvgQueryBuilder SetAttribute(string key, string value)
  {
    ModifyNodes(Ast.Root, n => n.SetAttribute(key, value));
    return this;
  }

  /// <summary>
  /// Removes an attribute from SVG nodes that match the current predicate.
  /// </summary>
  /// <param name="key">The attribute name.</param>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance.</returns>
  public SvgQueryBuilder RemoveAttribute(string key)
  {
    ModifyNodes(Ast.Root, n => n.RemoveAttribute(key));
    return this;
  }

  /// <summary>
  /// Converts the SVG nodes that match the current predicate to a list.
  /// </summary>
  /// <returns>A list of <see cref="SvgNode"/> instances that match the current predicate.</returns>
  public List<SvgNode> ToList()
  {
    return FindNodes(Ast.Root).ToList();
  }


  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Modifies SVG nodes based on the specified action.
  /// </summary>
  /// <param name="node">The root node to start modifying from.</param>
  /// <param name="action">The action to perform on each node.</param>
  void ModifyNodes(SvgNode node, Action<SvgNode> action)
  {
    if(Predicate(node))
    {
      action(node);
    }

    foreach(SvgNode child in node.Children)
    {
      ModifyNodes(child, action);
    }
  }

  /// <summary>
  /// Finds SVG nodes that match the current predicate.
  /// </summary>
  /// <param name="node">The root node to start finding from.</param>
  /// <returns>An enumerable of <see cref="SvgNode"/> instances that match the current predicate.</returns>
  IEnumerable<SvgNode> FindNodes(SvgNode node)
  {
    if(Predicate(node))
    {
      yield return node;
    }

    foreach(var child in node.Children)
    {
      if(Predicate(child))
      {
        yield return child;
      }
    }
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit
    - File: libs\Six.SvgKit\src\SvgResult.cs
-->
```
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using JetBrains.Annotations;

namespace Six.SvgKit;

[PublicAPI]
[ExcludeFromCodeCoverage]
public record SvgResult
{
  #region PROPERTIES
  
  /// <summary>
  /// Gets the original SVG object.
  /// </summary>
  public Svg OriginalSvg { get; }

  /// <summary>
  /// Gets the modified SVG object.
  /// </summary>
  public Svg ModifiedSvg { get; }

  /// <summary>
  /// Gets the ID of the modified SVG.
  /// </summary>
  public string Id => ModifiedSvg.Id;

  
  #endregion
  
  // CTOR / DISPOSAL
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Initializes a new instance of the <see cref="SvgResult"/> class.
  /// </summary>
  /// <param name="originalSvg">The original SVG object.</param>
  /// <param name="modifiedSvg">The modified SVG object.</param>
  public SvgResult(Svg originalSvg, Svg modifiedSvg)
  {
    ModifiedSvg = modifiedSvg;
    OriginalSvg = originalSvg;
  }

  
  // PUBLIC CHAINABLE METHODS
  // ----------------------------------------------------------------------------------------------------------
  /// <summary>
  /// Compares the size of the original and modified SVGs in percentage and invokes the provided delegate with the result.
  /// </summary>
  /// <param name="comparisonDelegate">The delegate to invoke with the comparison result.</param>
  /// <returns>The current <see cref="SvgResult"/> instance.</returns>
  public SvgResult GetSizeComparisonInPercentage(Action<double> comparisonDelegate)
  {
    int bytesBefore = OriginalSvg.ToBytes().Length;
    int bytesAfter = ModifiedSvg.ToBytes().Length;
    var pct = bytesBefore > 0 ? (1 - (double)bytesAfter / bytesBefore) * 100 : 0;

    comparisonDelegate(pct);
    return this;
  }

  /// <summary>
  /// Compares the size of the original and modified SVGs in bytes and invokes the provided delegate with the result.
  /// </summary>
  /// <param name="comparisonDelegate">The delegate to invoke with the comparison result.</param>
  /// <returns>The current <see cref="SvgResult"/> instance.</returns>
  public SvgResult GetSizeComparisonInBytes(Action<int, int> comparisonDelegate)
  {
    comparisonDelegate(OriginalSvg.ToBytes().Length, ModifiedSvg.ToBytes().Length);
    return this;
  }

  
  
  // PUBLIC NON-CHAINABLE METHODS
  // ----------------------------------------------------------------------------------------------------------

  /// <summary>
  /// Converts the modified SVG to an <see cref="XDocument"/>.
  /// </summary>
  /// <returns>An <see cref="XDocument"/> representing the modified SVG.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public XDocument ToXDocument() => ModifiedSvg.ToXDocument();

  /// <summary>
  /// Saves the modified SVG to a file.
  /// </summary>
  /// <param name="path">The path to save the file to.</param>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public void ToFile(string path) => ModifiedSvg.ToFile(path);

  /// <summary>
  /// Converts the modified SVG to its string representation.
  /// </summary>
  /// <returns>A string representing the modified SVG.</returns>
  [MethodImpl(MethodImplOptions.AggressiveInlining)]
  public string ToSvg() => ModifiedSvg.ToSvg();
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: libs\Six.SvgKit.Optimizer\README.md
-->
# Library

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: libs\Six.SvgKit.Optimizer\src\SvgOptimizer..cs
-->
```
using JetBrains.Annotations;

namespace Six.SvgKit.Optimizer;

[PublicAPI]
public static partial class SvgOptimizer
{
  /// <summary>
  /// Removes the XML declaration from the SVG document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder RemoveXmlDeclaration(this SvgQueryBuilder builder)
  {
    builder.Svg.OmmitXmlDeclaration = true;
    return builder;
  }

  
  /// <summary>
  /// Removes the SVG namespace from the root element (xmlns="http://www.w3.org/2000/svg").
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder RemoveNamespace(this SvgQueryBuilder builder)
  {
    builder.Svg.RemoveNamespace = true;
    return builder;
  }
  
  
  /// <summary>
  /// Compresses the output of a SVG document by removing unnecessary whitespace and line breaks.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder CompressOutput(this SvgQueryBuilder builder)
  {
    builder.Svg.CompressOutput = true;
    return builder;
  }

}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: libs\Six.SvgKit.Optimizer\src\SvgOptimizer.Attributes.cs
-->
```
namespace Six.SvgKit.Optimizer;

public static partial class SvgOptimizer
{
  /// <summary>
  /// Removes the viewBox attribute from the SVG document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder RemoveViewBoxAttribute(this SvgQueryBuilder builder)
  {
    builder.RemoveAttribute("viewBox");
    return builder;
  }


}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: libs\Six.SvgKit.Optimizer\src\SvgOptimizer.Groups.cs
-->
```
namespace Six.SvgKit.Optimizer;

public static partial class SvgOptimizer
{
  /// <summary>
  /// Removes empty group elements from the SVG document.
  /// </summary>
  /// <param name="builder">The <see cref="SvgQueryBuilder"/> instance to operate on.</param>
  /// <returns>The modified <see cref="SvgQueryBuilder"/> instance.</returns>
  public static SvgQueryBuilder RemoveEmptyContainers(this SvgQueryBuilder builder)
  {
    RemoveEmptyGroups(builder.Ast.Root);
    return builder;
  }


  /// <summary>
  /// Finds groups that effectively do nothing and flattens them, preserving the contents of the groups.
  /// </summary>
  /// <param name="builder">The <see cref="SvgQueryBuilder"/> instance to operate on.</param>
  /// <returns>The modified <see cref="SvgQueryBuilder"/> instance.</returns>
  public static SvgQueryBuilder CollapseGroups(this SvgQueryBuilder builder)
  {
    RemoveEmptyGroups(builder.Ast.Root);
    return builder;
  }


  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
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

  static void RemoveEmptyGroups(SvgNode node)
  {
    node.Children.RemoveAll(child =>
      child.Name == "g" && !child.Attributes.Any() && !child.Children.Any());

    foreach(var child in node.Children)
    {
      RemoveEmptyGroups(child);
    }
  }

  static void CollapseGroupsRecursive(SvgNode node)
  {
    if(node is { Name: "g", Children.Count: > 0 })
    {
      if(node.Attributes.Any() && node.Children.Count == 1)
      {
        var firstChild = node.Children[0];
        var nodeHasFilter = node.Attributes.ContainsKey("filter");
        var nodeHasClipPath = node.Attributes.ContainsKey("clip-path");
        var nodeHasMask = node.Attributes.ContainsKey("mask");

        if(firstChild.Name != "g" && firstChild.Attributes.ContainsKey("id") == false 
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
              return;
            }
          }

          node.Attributes.Clear();
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

    foreach(var child in node.Children)
    {
      CollapseGroupsRecursive(child);
    }
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: libs\Six.SvgKit.Optimizer\src\SvgOptimizer.Paths.cs
-->
```
namespace Six.SvgKit.Optimizer;

public static partial class SvgOptimizer
{

  /// <summary>
  /// Merges multiple SVG path elements into a single path element within the SVG document.
  /// </summary>
  /// <remarks><b>This approach has limitations and it's suitable for simple vectors like monochromatic icons</b></remarks>
  /// <param name="builder">The SvgQueryBuilder instance to operate on.</param>
  /// <returns>The modified SvgQueryBuilder instance.</returns>
  public static SvgQueryBuilder MergePaths(this SvgQueryBuilder builder)
  {
    var root = builder.Ast.Root;

    // Collect paths
    var paths = root.Children
      .Where(c => c.Name == "path")
      .Select(p => new { Node = p, Data = p.Attributes["d"], Fill = p.Attributes.GetValueOrDefault("fill") })
      .ToList();

    if(paths.Count > 1)
    {
      // Separate background (fill="none") and foreground paths
      var backgroundPath = paths.FirstOrDefault(p => p.Fill == "none");
      var foregroundPaths = paths.Where(p => p.Fill != "none").ToList();

      // Merge foreground paths
      var mergedPath = new SvgNode(builder.Svg, "path");
      mergedPath.SetAttribute("d", string.Join(" ", foregroundPaths.Select(p => p.Data)));
      mergedPath.SetAttribute("fill-rule", "evenodd");

      // Replace all paths with the background (if exists) and merged foreground path
      root.Children.RemoveAll(c => c.Name == "path");

      if(backgroundPath != null)
      {
        root.AddChild(backgroundPath.Node);
      }

      root.AddChild(mergedPath);
    }

    return builder;
  }

}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: libs\Six.SvgKit.Optimizer\src\Helpers\PathHelpers.cs
-->
```
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
```

<!--
  Metadata:
    - Project: Six.SvgKit.Test.Unit
    - File: tests\Six.SvgKit.Test.Unit\GlobalUsings.cs
-->
```
global using Xunit;
global using System.Xml.Linq;
global using Six.SvgKit;
global using Six.SvgKit.Optimizer;
```

<!--
  Metadata:
    - Project: Six.SvgKit.Test.Unit
    - File: tests\Six.SvgKit.Test.Unit\src\SvgAstTests.cs
-->
```
namespace SvgKit.Test.Unit;

public class SvgAstTests
{
  [Fact]
  public void Constructor_ValidXDocument_CreatesAstCorrectly()
  {
    // Arrange
    var svg = Svg.FromXDocument(XDocument.Parse("<svg><rect width='100' height='100'/></svg>"));
    var xDoc = XDocument.Parse("<svg><rect width='100' height='100'/></svg>");

    // Act
    var ast = new SvgAst(svg, xDoc);

    // Assert
    Assert.NotNull(ast.Root);
    Assert.Equal("svg", ast.Root.Name);
    Assert.Single(ast.Root.Children);
    Assert.Equal("rect", ast.Root.Children[0].Name);
  }

  [Fact]
  public void Constructor_ValidSvgString_CreatesAstCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg><circle cx='50' cy='50' r='40'/></svg>");
    var svgString = "<svg><circle cx='50' cy='50' r='40'/></svg>";

    // Act
    var ast = new SvgAst(svg, svgString);

    // Assert
    Assert.NotNull(ast.Root);
    Assert.Equal("svg", ast.Root.Name);
    Assert.Single(ast.Root.Children);
    Assert.Equal("circle", ast.Root.Children[0].Name);
  }

}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Test.Unit
    - File: tests\Six.SvgKit.Test.Unit\src\SvgNodeTests.cs
-->
```
namespace SvgKit.Test.Unit;

public class SvgNodeTests
{
  [Fact]
  public void Constructor_SetsNameCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");

    // Act
    var node = new SvgNode(svg, "rect");

    // Assert
    Assert.Equal("rect", node.Name);
  }

  [Fact]
  public void AddChild_AddsChildCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var parent = new SvgNode(svg, "g");
    var child = new SvgNode(svg, "rect");

    // Act
    parent.AddChild(child);

    // Assert
    Assert.Single(parent.Children);
    Assert.Equal(child, parent.Children[0]);
    Assert.Equal(parent, child.Parent);
  }

  [Fact]
  public void SetAttribute_SetsAttributeCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var node = new SvgNode(svg, "rect");

    // Act
    node.SetAttribute("width", "100");

    // Assert
    Assert.Equal("100", node.Attributes["width"]);
  }

  [Fact]
  public void RemoveAttribute_RemovesAttributeCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var node = new SvgNode(svg, "rect");
    node.SetAttribute("width", "100");

    // Act
    node.RemoveAttribute("width");

    // Assert
    Assert.False(node.Attributes.ContainsKey("width"));
  }

  [Fact]
  public void ToXElement_CreatesCorrectXElement()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var node = new SvgNode(svg, "rect");
    node.SetAttribute("width", "100");
    node.SetAttribute("height", "50");

    // Act
    var xElement = node.ToXElement();

    // Assert
    Assert.Equal("rect", xElement.Name.LocalName);
    Assert.Equal("100", xElement.Attribute("width")?.Value);
    Assert.Equal("50", xElement.Attribute("height")?.Value);
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Test.Unit
    - File: tests\Six.SvgKit.Test.Unit\src\SvgOptimizerTests.cs
-->
```
namespace SvgKit.Test.Unit;

public partial class SvgOptimizerTests
{
  [Fact]
  public void RemoveXmlDeclaration_SetsOmitXmlDeclarationToTrue()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveXmlDeclaration();

    // Assert
    Assert.True(builder.Svg.OmmitXmlDeclaration);
  }

  [Fact]
  public void RemoveNamespace_SetsRemoveNamespaceToTrue()
  {
    // Arrange
    var svg = Svg.FromString("<svg></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveNamespace();

    // Assert
    Assert.True(builder.Svg.RemoveNamespace);
  }

  [Fact]
  public void RemoveEmptyGroups_RemovesEmptyGroupElements()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g></g><g><rect/></g><g></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveEmptyContainers();

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;
    
    Assert.NotNull(resultXml);
    Assert.Single(resultXml.Elements());
    Assert.Equal("g", resultXml.Elements().First().Name.LocalName);
    Assert.Single(resultXml.Elements().First().Elements());
  }

  [Fact]
  public void RemoveViewBoxAttribute_RemovesViewBoxAttribute()
  {
    // Arrange
    var svg = Svg.FromString("<svg viewBox='0 0 100 100'></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.RemoveViewBoxAttribute();

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;
    
    Assert.NotNull(resultXml);
    Assert.Null(resultXml.Attribute("viewBox"));
  }

  [Fact]
  public void MergePaths_MergesMultiplePathsIntoOne()
  {
    // Arrange
    var svg = Svg.FromString("<svg><path d='M0 0h10'/><path d='M10 10h10'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.MergePaths();

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);
    
    Assert.Single(resultXml.Elements());
    Assert.Equal("path", resultXml.Elements().First().Name.LocalName);
    Assert.Equal("M0 0h10 M10 10h10", resultXml.Elements().First().Attribute("d")?.Value);
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Test.Unit
    - File: tests\Six.SvgKit.Test.Unit\src\SvgOptimizerTests.Groups.cs
-->
```
namespace SvgKit.Test.Unit;

public partial class SvgOptimizerTests
{
  [Fact]
  public void CollapseGroups_EmptyGroup_RemovesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var svgElement = result.ToXDocument().Root;

    Assert.NotNull(svgElement);
    Assert.DoesNotContain(svgElement.Descendants(), e => e.Name.LocalName == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithAttributes_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g fill='red'><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithSingleChild_MovesAttributesToChild()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g fill='red'><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    var rectElement = svgElement.Descendants().First(e => e.Name.LocalName == "rect");
    Assert.Equal("red", rectElement.Attribute("fill")?.Value);
  }

  [Fact]
  public void CollapseGroups_GroupWithFilterAttribute_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g filter='url(#f)'><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithClipPathAttribute_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g clip-path='url(#cp)'><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithMaskAttribute_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g mask='url(#m)'><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
  }

  [Fact]
  public void CollapseGroups_GroupWithAnimationElement_PreservesGroup()
  {
    // Arrange
    var svg = Svg.FromString("<svg><g><animate/><rect/></g></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.CollapseGroups();

    // Assert
    var result = builder.Build();
    var svgElement = result.ToXDocument().Root;
    
    Assert.NotNull(svgElement);
    Assert.Contains(svgElement.Descendants(), e => e.Name.LocalName == "g");
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Test.Unit
    - File: tests\Six.SvgKit.Test.Unit\src\SvgQueryBuilderTests.cs
-->
```
namespace SvgKit.Test.Unit;

public class SvgQueryBuilderTests
{
  [Fact]
  public void Select_SetsPredicateCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg><rect/><circle/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.Select(node => node.Name == "rect");

    // Assert
    Assert.True(builder.Predicate(new SvgNode(svg, "rect")));
    Assert.False(builder.Predicate(new SvgNode(svg, "circle")));
  }

  [Fact]
  public void Where_CombinesPredicatesCorrectly()
  {
    // Arrange
    var svg = Svg.FromString("<svg><rect width='100'/><rect width='200'/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder.Select(node => node.Name == "rect")
      .Where(node => node.Attributes.ContainsKey("width") && node.Attributes["width"] == "100");

    // Assert
    var rectNode100 = new SvgNode(svg, "rect");
    rectNode100.SetAttribute("width", "100");
    var rectNode200 = new SvgNode(svg, "rect");
    rectNode200.SetAttribute("width", "200");

    Assert.True(builder.Predicate(rectNode100));
    Assert.False(builder.Predicate(rectNode200));
  }

  [Fact]
  public void RemoveNodes_RemovesMatchingNodes()
  {
    // Arrange
    var svg = Svg.FromString("<svg><rect/><circle/><rect/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder
      .Select(node => node.Name == "rect")
      .RemoveNodes();

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;

    Assert.NotNull(resultXml);
    Assert.Single(resultXml.Elements());
    Assert.Equal("circle", resultXml.Elements().First().Name.LocalName);
  }

  [Fact]
  public void SetAttribute_SetsAttributeOnMatchingNodes()
  {
    // Arrange
    var svg = Svg.FromString("<svg><rect/><circle/></svg>");
    var builder = new SvgQueryBuilder(svg);

    // Act
    builder
      .Select(node => node.Name == "rect")
      .SetAttribute("width", "100");

    // Assert
    var result = builder.Build();
    var resultXml = result.ToXDocument().Root;
    
    Assert.NotNull(resultXml);
    
    var rectElement = resultXml.Elements().First(e => e.Name.LocalName == "rect");
    Assert.Equal("100", rectElement.Attribute("width")?.Value);
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Test.Unit
    - File: tests\Six.SvgKit.Test.Unit\src\SvgTests.cs
-->
```
namespace SvgKit.Test.Unit;

public class SvgTests
{
  private const string TestSvgString = @"
            <svg xmlns=""http://www.w3.org/2000/svg"" width=""100"" height=""100"">
                <rect x=""10"" y=""10"" width=""80"" height=""80"" fill=""blue"" />
            </svg>";

  [Fact]
  public void FromString_CreatesSvgInstance()
  {
    var svg = Svg.FromString(TestSvgString);
    Assert.NotNull(svg);
    Assert.IsType<Svg>(svg);
  }

  [Fact]
  public void FromXDocument_CreatesSvgInstance()
  {
    var xDoc = XDocument.Parse(TestSvgString);
    var svg = Svg.FromXDocument(xDoc);
    Assert.NotNull(svg);
    Assert.IsType<Svg>(svg);
  }

  [Fact]
  public void WithId_SetsSvgId()
  {
    var svg = Svg.FromString(TestSvgString);
    svg.WithId("testId");
    Assert.Equal("testId", svg.Id);
  }

  [Fact]
  public void Clone_CreatesCopy()
  {
    var original = Svg.FromString(TestSvgString);
    original.WithId("originalId");
    var clone = original.Clone();

    Assert.NotNull(clone);
    Assert.IsType<Svg>(clone);
    Assert.Equal(original.Id, clone.Id);
    Assert.NotSame(original, clone);
  }

  [Fact]
  public void ToXDocument_ReturnsValidXDocument()
  {
    var svg = Svg.FromString(TestSvgString);
    var xDoc = svg.ToXDocument();

    Assert.NotNull(xDoc);
    Assert.IsType<XDocument>(xDoc);
    Assert.Equal("svg", xDoc.Root?.Name.LocalName);
  }

  [Fact]
  public void ToSvg_ReturnsValidSvgString()
  {
    var svg = Svg.FromString(TestSvgString);
    var svgString = svg.ToSvg();

    Assert.NotNull(svgString);
    Assert.Contains("<svg", svgString);
    Assert.Contains("<rect", svgString);
  }

  [Fact]
  public void ToBytes_ReturnsNonEmptyByteArray()
  {
    var svg = Svg.FromString(TestSvgString);
    var bytes = svg.ToBytes();

    Assert.NotNull(bytes);
    Assert.NotEmpty(bytes);
  }
}
```

