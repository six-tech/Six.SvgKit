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