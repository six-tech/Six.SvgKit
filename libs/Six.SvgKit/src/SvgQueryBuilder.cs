using JetBrains.Annotations;

namespace Six.SvgKit;

/// <summary>
/// Provides functionality to build and manipulate SVG documents.
/// </summary>
[PublicAPI]
public class SvgQueryBuilder
{
  // MEMBERS
  readonly Svg
    _originalSvg; // The original SVG object. The new one is a clone  of this(one on which work will performed).


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
  /// Creates a conditional branching logic within the query, allowing different builders for true and false conditions.
  /// </summary>
  /// <example>
  /// <code>
  /// builder
  ///   .Select(n => n.Name == "rect")
  ///   .WhereFork(
  ///     n => int.Parse(n.Attributes.GetValueOrDefault("width", "0")) > 100, // Condition
  ///   ifTrue:  n => n.SetAttribute("fill", "red"), // Action if true
  ///   ifFalse: n => n.SetAttribute("fill", "blue")  // Action if false
  ///   );
  /// </code>
  /// </example>
  /// <param name="condition">The condition to evaluate for branching.</param>
  /// <param name="ifTrue">The builder to execute if the condition is true.</param>
  /// <param name="ifFalse">
  /// The builder to execute if the condition is false.
  /// If not provided, the false branch will not be executed.
  /// </param>
  /// <returns>The current <see cref="SvgQueryBuilder"/> instance with the applied branches.</returns>
  public SvgQueryBuilder WhereFork(
    Func<SvgNode, bool> condition,
    Func<SvgQueryBuilder, SvgQueryBuilder> ifTrue,
    Func<SvgQueryBuilder, SvgQueryBuilder>? ifFalse = null)
  {
    try
    {
      // ReSharper disable once ConvertClosureToMethodGroup
      var trueBuilder = new SvgQueryBuilder(Svg).Select(n => condition(n));
      var falseBuilder = new SvgQueryBuilder(Svg).Select(n => !condition(n));

      trueBuilder = ifTrue(trueBuilder);

      if(ifFalse != null)
      {
        falseBuilder = ifFalse(falseBuilder);
      }

      // Combine the results of both branches (you might need to adjust this)
      Predicate = n => trueBuilder.Predicate(n) || falseBuilder.Predicate(n);
      return this;
    }
    catch(Exception ex)
    {
      // Error handling logic here
      Console.WriteLine($"Error in WhereFork: {ex.Message}");

      // Re-throw the exception or handle it as needed
      throw; // Re-throwing for now, but you can customize error handling
    }
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
      foreach (var attr in attributes)
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