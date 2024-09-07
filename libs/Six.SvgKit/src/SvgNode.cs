using System.Xml.Linq;
using AngleSharp.Dom;
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
  readonly AttributeDictionary _attributes = [];
  
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
  public AttributeDictionary Attributes => _attributes;
  
  /// <summary>
  /// Gets if this node is a container node
  /// </summary>
  public bool IsContainer => SvgContainers.IsContainer(this);
  
  /// <summary>
  /// Gets if this node is a container node and has children
  /// </summary>
  public bool IsContainerWithChildren => SvgContainers.IsContainer(this) && Children.Count > 0;

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
  /// Adds a child <see cref="SvgNode"/> to this node.
  /// </summary>
  /// <param name="child">The child <see cref="SvgNode"/> to add.</param>
  public void AddChild(SvgNode child)
  {
    Children.Add(child);
    child.OnAddedToParent(this);
  }


  /// <summary>
  /// Creates a new child <see cref="SvgNode"/> with the specified name and adds it to this node.
  /// </summary>
  /// <param name="name">Name (type) of the child <see cref="SvgNode"/></param>
  /// <returns>Created <see cref="SvgNode"/> instance</returns>
  public SvgNode CreateChild(string name)
  {
    var child = new SvgNode(Svg, name);
    AddChild(child);
    return child;
  }

  /// <summary>
  /// Removes a specified child <see cref="SvgNode"/> from this node.
  /// </summary>
  /// <param name="childNode">The child <see cref="SvgNode"/> to be removed.</param>
  /// <returns>True if the child node was successfully removed; otherwise, false.</returns>
  public bool RemoveChild(SvgNode childNode)
  {
    if(Children.Contains(childNode) == false)
      return false;

    Children.Remove(childNode);
    childNode.OnRemovedFromParent(this);
    return true;
  }


  /// <summary>
  /// Removes this <see cref="SvgNode"/> from its parent node.
  /// </summary>
  /// <returns>True if the node was successfully removed; otherwise, false.</returns>
  public bool Remove()
  {
    if(Parent is null)
      return false;

    return Parent.RemoveChild(this);
  }


  /// <summary>
  /// Sets an attribute on this <see cref="SvgNode"/>.
  /// </summary>
  /// <param name="key">The attribute name.</param>
  /// <param name="value">The attribute value.</param>
  public void SetAttribute(string key, string value)
  {
    _attributes[key] = value;
  }


  /// <summary>
  /// Removes an attribute from this <see cref="SvgNode"/>.
  /// </summary>
  /// <param name="key">The attribute name.</param>
  public void RemoveAttribute(string key)
  {
    _attributes.Remove(key);
  }

  /// <summary>
  /// Clears all attributes from this <see cref="SvgNode"/>.
  /// </summary>
  public void ClearAttributes()
  {
    _attributes.Clear();
  }


  /// <summary>
  /// Selects and optionally transforms attributes of an <see cref="SvgNode"/>.
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

    // Clear the existing Attributes and repopulate it
    _attributes.Clear();
    foreach(var kvp in updatedAttributes)
    {
      _attributes.Add(kvp.Key, kvp.Value);
    }

    return this;
  }


  /// <summary>
  /// Converts the current <see cref="SvgNode"/> instance to an AngleSharp <see cref="IElement"/>.
  /// </summary>
  /// <param name="isRoot">Indicates whether the current node is the root of the SVG document.</param>
  /// <returns>An <see cref="IElement"/> representing the current <see cref="SvgNode"/>.</returns>
  public IElement ToAngleSharpElement(bool isRoot = false)
  {
    var document = Svg.Ast.Document; // Get the AngleSharp document from the AST

    // Create the element
    var element = isRoot
      ? document.CreateElement("svg")
      : document.CreateElement(Name);

    // Set attributes
    foreach(var attr in Attributes)
    {
      // Handle xlink namespace attributes
      if(attr.Key.StartsWith("xlink:"))
      {
        element.SetAttribute("http://www.w3.org/1999/xlink", attr.Key.Substring(6), attr.Value);
      }
      else
      {
        element.SetAttribute(attr.Key, attr.Value);
      }
    }

    // Add children recursively
    foreach(var child in Children)
    {
      element.AppendChild(child.ToAngleSharpElement());
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
  /// Called when this node has been removed from the parent node.
  /// </summary>
  /// <param name="parent">The parent node.</param>
  internal void OnRemovedFromParent(SvgNode parent)
  {
    Parent = null;
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