<!--
  Metadata:
    - Project: Six.SvgKit
    - File: Six.SvgKit\README.md
-->
# Library

<!--
  Metadata:
    - Project: Six.SvgKit
    - File: Six.SvgKit\src\Svg.cs
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
    - File: Six.SvgKit\src\SvgAst.cs
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
    - File: Six.SvgKit\src\SvgNode.cs
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
  public Dictionary<string, string> Attributes { get; private set; } = [];

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
  /// Selects and optionally transforms attributes of an SvgNode.
  /// </summary>
  /// <param name="predicate">A function that takes an attribute key-value pair and returns a boolean indicating whether the attribute should be selected.</param>
  /// <param name="transform">An optional function to transform the attribute key and value. If null, the original key-value pairs are kept.</param>
  /// <returns>The modified SvgNode.</returns>
  public SvgNode SelectAttributes(
    Func<KeyValuePair<string, string>, bool> predicate,
    Func<string, string, (string, string)>? transform = null)
  {
    var updatedAttributes = new Dictionary<string, string>();

    foreach(var attr in Attributes)
    {
      if(predicate(attr))
      {
        if(transform != null)
        {
          (string newKey, string newValue) = transform(attr.Key, attr.Value);
          updatedAttributes[newKey] = newValue;
        }
        else
        {
          updatedAttributes[attr.Key] = attr.Value;
        }
      }
      else
      {
        updatedAttributes[attr.Key] = attr.Value;
      }
    }

    Attributes = updatedAttributes;
    return this;
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
    - File: Six.SvgKit\src\SvgQueryBuilder.cs
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
  // The original SVG object. The new one is a clone  of this(one on which work will performed).
  readonly Svg _originalSvg;

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
  /// Applies all pending transformations to the SVG object and resets the 
  /// selection predicate, allowing for a new set of operations on the modified SVG.
  /// </summary>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance for method chaining.</returns>
  public SvgQueryBuilder Continue()
  {
    // No actual operations needed here as transformations are applied lazily 
    // when Build() is called. 
    // We simply reset the Predicate for the next stage.
    Predicate = _ => true; 
    return this;
  }
  
  
  /// <summary>
  /// Selects SVG nodes that match the specified predicate.
  /// </summary>
  /// <example>
  /// <code>
  /// builder
  ///   .Select(node => node.Name == "rect");
  /// </code>
  /// </example>
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
  /// <example>
  /// <code>
  /// builder
  ///   .Select(node => node.Name == "rect")
  ///   .Where(node => node.Attributes.ContainsKey("width") &amp;&amp; node.Attributes["width"] == "100");
  /// </code>
  /// </example>
  /// <param name="predicate">The predicate function to filter SVG nodes.</param>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance.</returns>
  public SvgQueryBuilder Where(Func<SvgNode, bool> predicate)
  {
    var oldPredicate = Predicate;
    Predicate = node => oldPredicate(node) && predicate(node);
    return this;
  }


  /// <summary>
  /// Applies the specified action to each node in the current selection.
  /// </summary>
  /// <remarks>Useful for directly modifying node properties.</remarks>
  /// <example>
  /// <code>
  /// builder
  ///   .Select(n => n.Name == "rect")
  ///   .Transform(n => n.SetAttribute("transform", "translate(10, 5)"));
  /// </code>
  /// </example>
  /// <param name="action"></param>
  /// <returns></returns>
  public SvgQueryBuilder Transform(Action<SvgNode> action)
  {
    ModifyNodes(Ast.Root, action);
    return this;
  }


  /// <summary>
  /// An alias for Transform to improve readability in certain contexts.
  /// </summary>
  /// <param name="action"></param>
  /// <returns></returns>
  public SvgQueryBuilder ForEach(Action<SvgNode> action)
  {
    return Transform(action);
  }

  /// <summary>
  /// Projects each node in the current selection into a sequence of nodes
  /// and flattens the resulting sequences into one sequence.
  /// </summary>
  /// <remarks>
  /// Useful for working with nested structures or creating new nodes based on existing ones.
  /// </remarks>
  /// <example>
  /// <code>
  /// builder
  ///   .Select(n => n.Name == "circle")
  ///   .SelectMany(n => 
  ///   {
  ///     var cx = int.Parse(n.Attributes["cx"]);
  ///     var cy = int.Parse(n.Attributes["cy"]);
  ///     var newCircle = new SvgNode(n.Svg, "circle")
  ///     {
  ///      Attributes = 
  ///       {
  ///         { "cx", (cx + 5).ToString() },
  ///         { "cy", (cy + 5).ToString() },
  ///         { "r", "2" },
  ///         { "fill", "red" }
  ///       }
  ///     };
  ///    return new[] { n, newCircle }; // Return both the original and new circle
  ///   });
  /// </code>
  /// </example>
  /// <param name="selector"></param>
  /// <returns></returns>
  public SvgQueryBuilder SelectMany(Func<SvgNode, IEnumerable<SvgNode>> selector)
  {
    var selectedNodes = FindNodes(Ast.Root).SelectMany(selector).ToList();
    Ast.Root.OnSetChildern(selectedNodes);
    return this;
  }


  /// <summary>
  /// Groups the nodes in the current selection based on a key.
  /// Returns an IEnumerable{IGrouping{TKey}, SvgNode}}, allowing you to work with each group of nodes.
  /// </summary>
  /// <example>
  /// <code>
  /// var groups = builder
  ///   .Select(n => n.Name == "path")
  ///   .GroupBy(n => n.Attributes.GetValueOrDefault("fill"));
  ///   
  ///   foreach (var group in groups)
  ///   {
  ///     Console.WriteLine($"Fill Color: {group.Key}");
  ///     // Process nodes within each group
  ///   }
  /// </code>
  /// </example>
  /// <param name="keySelector"></param>
  /// <typeparam name="TKey"></typeparam>
  /// <returns></returns>
  public IEnumerable<IGrouping<TKey, SvgNode>> GroupBy<TKey>(Func<SvgNode, TKey> keySelector)
  {
    return FindNodes(Ast.Root).GroupBy(keySelector);
  }


  /// <summary>
  /// Removes previously selected SVG nodes.
  /// </summary>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance.</returns>
  public SvgQueryBuilder RemoveNodes()
  {
    Ast.Root.OnSetChildern(Ast.Root.Children.Where(child => !Predicate(child)).ToList());
    return this;
  }

  
  /// <summary>
  /// Selects and optionally transforms attributes of the currently selected nodes.
  /// </summary>
  /// <param name="predicate">A function that takes an attribute key-value pair and returns a boolean indicating whether the attribute should be selected.</param>
  /// <param name="transform">An optional function to transform the attribute key and value. If null, the original key-value pairs are kept.</param>
  /// <returns>The current SvgQueryBuilder instance.</returns>
  public SvgQueryBuilder SelectAttributes(
    Func<KeyValuePair<string, string>, bool> predicate,
    Action<SvgNode, string, string>? transform = null)
  {
    ModifyNodes(Ast.Root, node =>
    {
      foreach (var attr in node.Attributes.ToArray()) // Iterate over a copy
      {
        if (predicate(attr))
        {
          if (transform != null)
          {
            transform(node, attr.Key, attr.Value);
          }
        }
      }
    });
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
  /// Conditionally sets an attribute on nodes that match the predicate.
  /// </summary>
  /// <example>
  /// Set the fill of circles with radius > 10 to "blue"
  /// <code>
  /// builder.SetAttributeIf(n => n.Name == "circle" &amp;&amp; int.Parse(n.Attributes.GetValueOrDefault("r", "0")) &gt; 10, "fill", "blue");
  /// </code>
  /// </example>
  /// <param name="predicate"></param>
  /// <param name="key"></param>
  /// <param name="value"></param>
  /// <returns></returns>
  public SvgQueryBuilder SetAttributeIf(Func<SvgNode, bool> predicate, string key, string value)
  {
    ModifyNodes(Ast.Root, n =>
    {
      if(predicate(n))
      {
        n.SetAttribute(key, value);
      }
    });
    return this;
  }


  /// <summary>
  /// Sets multiple attributes on all selected SVG nodes.
  /// </summary>
  /// <example>
  /// Set both fill and stroke of selected paths
  /// <code>
  /// var newAttributes = new Dictionary&lt;string, string&gt; { { "fill", "yellow" }, { "stroke", "black" } };
  /// builder.Select(n => n.Name == "path").SetAttributes(newAttributes);
  /// </code>
  /// </example>
  /// <param name="attributes">A dictionary containing the key-value pairs of attributes to set.</param>
  /// <returns>The current instance of <see cref="SvgQueryBuilder"/> for method chaining.</returns>
  public SvgQueryBuilder SetAttributes(Dictionary<string, string> attributes)
  {
    ModifyNodes(Ast.Root, n =>
    {
      foreach(var attr in attributes)
      {
        n.SetAttribute(attr.Key, attr.Value);
      }
    });
    return this;
  }


  /// <summary>
  /// Conditionally removes an attribute from nodes based on the predicate.
  /// </summary>
  /// <example>
  /// Remove the "stroke" attribute from rectangles with no fill
  /// <code>
  /// builder.RemoveAttributeIf(n => n.Name == "rect" &amp;&amp; !n.Attributes.ContainsKey("fill"), "stroke");
  /// </code>
  /// </example>
  /// <param name="predicate">The condition that determines which nodes should have the attribute removed.</param>
  /// <param name="key">The key of the attribute to be removed.</param>
  /// <returns>An instance of <see cref="SvgQueryBuilder"/> with the specified attribute removed based on the predicate.</returns>
  public SvgQueryBuilder RemoveAttributeIf(Func<SvgNode, bool> predicate, string key)
  {
    ModifyNodes(Ast.Root, n =>
    {
      if(predicate(n))
      {
        n.RemoveAttribute(key);
      }
    });
    return this;
  }


  /// <summary>
  /// Filters nodes that have the specified attribute. If value is provided, it also checks for attribute value equality.
  /// </summary>
  /// <example>
  /// Select all elements with the class "highlight"
  /// <code>
  /// builder.HasAttribute("class", "highlight");
  /// </code>
  /// </example>
  /// <param name="key">The key of the attribute to filter on.</param>
  /// <param name="value">The optional value of the attribute to filter on. If null, all nodes with the specified attribute regardless of value will be included.</param>
  /// <returns>An instance of <see cref="SvgQueryBuilder"/> with the updated predicate.</returns>
  public SvgQueryBuilder HasAttribute(string key, string? value = null)
  {
    var oldPredicate = Predicate;
    Predicate = node => oldPredicate(node) &&
                        node.Attributes.ContainsKey(key) &&
                        (value == null || node.Attributes[key] == value);
    return this;
  }


  /// <summary>
  /// TApplies a transformation function to the value of the specified attribute for each selected node.
  /// </summary>
  /// <example>
  /// Increase the font size of all text elements by 2px
  /// <code>
  /// builder.Select(n => n.Name == "text").TransformAttribute("font-size", size => (int.Parse(size) + 2) + "px");
  /// </code>
  /// </example>
  /// <param name="key">The key of the attribute to be transformed.</param>
  /// <param name="transformer">The function that defines the transformation of the attribute value.</param>
  /// <returns>A reference to this <see cref="SvgQueryBuilder"/> instance after the transformation is applied.</returns>
  public SvgQueryBuilder TransformAttribute(string key, Func<string, string> transformer)
  {
    ModifyNodes(Ast.Root, n =>
    {
      if(n.Attributes.ContainsKey(key))
      {
        n.SetAttribute(key, transformer(n.Attributes[key]));
      }
    });
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
  /// <remarks>It performs a recursive search</remarks>
  /// <param name="node">The root node to start finding from.</param>
  /// <returns>An enumerable of <see cref="SvgNode"/> instances that match the current predicate.</returns>
  IEnumerable<SvgNode> FindNodes(SvgNode node)
  {
    if(Predicate(node))
    {
      yield return node;
    }

    // Recursively call FindNodes for each child
    foreach(var child in node.Children)
    {
      foreach(var descendant in FindNodes(child)) // Iterate through descendants
      {
        yield return descendant;
      }
    }
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit
    - File: Six.SvgKit\src\SvgResult.cs
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
  public SvgResult SizeComparisonInPercentage(Action<double> comparisonDelegate)
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
  public SvgResult SizeComparisonInBytes(Action<int, int> comparisonDelegate)
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
    - File: Six.SvgKit.Optimizer\README.md
-->
# Library

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: Six.SvgKit.Optimizer\src\SvgOptimizer..cs
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
  /// Removes empty SVG nodes from the document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance.</param>
  /// <returns>The SvgQueryBuilder instance for method chaining.</returns>
  public static SvgQueryBuilder RemoveEmptyNodes(this SvgQueryBuilder builder)
  {
    void RemoveEmptyNodesRecursive(SvgNode node)
    {
      // Iterate through children in reverse order to safely remove while iterating
      for(int i = node.Children.Count - 1; i >= 0; i--)
      {
        var child = node.Children[i];
        RemoveEmptyNodesRecursive(child);

        if(!child.Children.Any())
        {
          node.Children.RemoveAt(i);
        }
      }
    }

    RemoveEmptyNodesRecursive(builder.Ast.Root);
    return builder;
  }


  /// <summary>
  /// Removes empty <c>text</c> and <c>tspan</c> elements, and <c>tref</c> elements that don't reference
  /// another node in the document.
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance.</param>
  /// <param name="removeText">If to remove empty <c>text</c> elements. Default is <c>true</c></param>
  /// <param name="removeTspan">If to remove empty <c>tspan</c> elements. Default is <c>true</c></param>
  /// <param name="removeTref">If to remove empty <c>tref</c> elements. Default is <c>true</c></param>
  /// <returns>The SvgQueryBuilder instance for method chaining.</returns>
  public static SvgQueryBuilder RemoveEmptyText(
    this SvgQueryBuilder builder,
    bool removeText = true,
    bool removeTspan = true,
    bool removeTref = true
    )
  {
    builder
      .Select(_ => true)
      .Where(n =>
      {
        return n is { Name: "text", Children.Count: 0 }
               || n is { Name: "tspan", Children.Count: 0 }
               || n.Name == "tref" && n.Attributes["href"] == string.Empty;
      }) // This is for xlink:href attribute. Xml parser sees it as href
      .RemoveNodes();

    return builder.Continue();
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
    - File: Six.SvgKit.Optimizer\src\SvgOptimizer.Attributes.cs
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
    builder.RemoveAttribute("viewBox").Continue();
    return builder;
  }

  /// <summary>
  /// Removes all whitespace from attribute values in the document.
  /// </summary>
  /// <param name="builder"></param>
  /// <returns></returns>
  public static SvgQueryBuilder CleanupAttributes(this SvgQueryBuilder builder)
  {
    builder
      .Select(_ => true)
      .SelectAttributes(
        _ => true,
        (node, key, value) => node.SetAttribute(key, value.Replace(" ", "").Trim())
      )
      .Continue();
    return builder;
  }

  /// <summary>
  /// Remove empty attributes from elements in the document.
  /// </summary>
  /// <remarks>
  /// Ignores the following attributes, as they're used for conditional rendering:
  /// <c>requiredFeatures</c>, <c>requiredExtensions</c>, <c>systemLanguage</c>
  /// </remarks>
  /// <param name="builder"></param>
  /// <returns></returns>
  public static SvgQueryBuilder RemoveEmptyAttributes(this SvgQueryBuilder builder)
  {
    builder
      .Select(_ => true)
      .SelectAttributes(
        _ => true,
        (node, key, value) =>
        {
          string[] ignored = ["requiredFeatures", "requiredExtensions", "systemLanguage"];

          if(string.IsNullOrEmpty(value) && !ignored.Contains(key))
          {
            node.RemoveAttribute(key);
          }
        })
      .Continue();

    return builder;
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: Six.SvgKit.Optimizer\src\SvgOptimizer.Groups.cs
-->
```
namespace Six.SvgKit.Optimizer;

public static partial class SvgOptimizer
{
  /// <summary>
  /// Remove container elements in the document that have no children or meaningful attributes,
  /// excluding the <c>&lt;svg&gt;</c> element which is ignored.
  /// </summary>
  /// <param name="builder">The <see cref="SvgQueryBuilder"/> instance to operate on.</param>
  /// <returns>The modified <see cref="SvgQueryBuilder"/> instance.</returns>
  public static SvgQueryBuilder RemoveEmptyContainers(this SvgQueryBuilder builder)
  {
    RemoveEmptyContainersInternal(builder.Ast.Root);
    return builder.Continue();
  }


  /// <summary>
  /// Finds groups that effectively do nothing and flattens them, preserving the contents of the groups.
  /// </summary>
  /// <param name="builder">The <see cref="SvgQueryBuilder"/> instance to operate on.</param>
  /// <returns>The modified <see cref="SvgQueryBuilder"/> instance.</returns>
  public static SvgQueryBuilder CollapseGroups(this SvgQueryBuilder builder)
  {
    var groups = builder
      .Select(n => n.Name == "g")
      .ToList();

    foreach(var group in groups)
    {
      CollapseGroupsInternal(group);
    }
    
    return builder.Continue();
  }


  // PRIVATE METHODS
  // ----------------------------------------------------------------------------------------------------------
  static void RemoveEmptyContainersInternal(SvgNode node)
  {
    // A container, as defined in the SVG specifications (https://www.w3.org/TR/SVG11/intro.html#TermContainerElement),
    // is an SVG element that can have graphical child elements. Container elements include:
    var emptyGroupNames = new HashSet<string>
    {
      "a",
      "defs",
      "glyph",
      "g",
      "marker",
      "mask",
      "missing-glyph",
      "pattern",
      "svg",
      "switch",
      "symbol"
    };

    node.Children.RemoveAll(child =>
      emptyGroupNames.Contains(child.Name) && !child.Attributes.Any() && !child.Children.Any());

    foreach(var child in node.Children)
    {
      RemoveEmptyContainersInternal(child);
    }
  }

  static void CollapseGroupsInternal(SvgNode node)
  {
    if(node is { Name: "g", Children.Count: > 0 })
    {
      if(node.Attributes.Any() && node.Children.Count == 1)
      {
        var firstChild = node.Children[0];
        var nodeHasFilter = node.Attributes.ContainsKey("filter");
        var nodeHasClipPath = node.Attributes.ContainsKey("clip-path");
        var nodeHasMask = node.Attributes.ContainsKey("mask");

        if(firstChild.Name != "g"
           && firstChild.Attributes.ContainsKey("id") == false
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
              firstChild.SetAttribute(attr.Key, attr.Value);
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
    else
    {
      // Remove empty groups
      node.Parent?.Children.Remove(node);
    }
  }


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
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: Six.SvgKit.Optimizer\src\SvgOptimizer.Paths.cs
-->
```
using System.Text.RegularExpressions;

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

    return builder.Continue();
  }

  
  /// <summary>
  /// Merges consecutive path commands of the same type into a single command.
  /// For example, "M 10,10 L 20,20 L 30,30" becomes "M 10,10 L 20,20 30,30".
  /// </summary>
  /// <param name="builder">The SvgQueryBuilder instance.</param>
  /// <returns>The SvgQueryBuilder instance for method chaining.</returns>
  public static SvgQueryBuilder MergePathCommands(this SvgQueryBuilder builder)
  {
    builder.Select(n => n.Name == "path")
      .Transform(node =>
      {
        if (node.Attributes.TryGetValue("d", out var pathData))
        {
          node.SetAttribute("d", MergePathDataCommands(pathData));
        }
      });

    return builder;
  }
  
  // Helper function to merge path commands in path data
  static string MergePathDataCommands(string pathData)
  {
    return Regex.Replace(pathData, @"([MLHVZ])\s*(?=[0-9\-])", "$1");
  }
}
```

<!--
  Metadata:
    - Project: Six.SvgKit.Optimizer
    - File: Six.SvgKit.Optimizer\src\Helpers\PathHelpers.cs
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

