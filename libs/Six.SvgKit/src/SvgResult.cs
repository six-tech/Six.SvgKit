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