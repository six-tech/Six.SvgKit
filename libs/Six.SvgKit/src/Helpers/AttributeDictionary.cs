using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace Six.SvgKit;

class CaseInsensitiveStringComparer : IEqualityComparer<string>
{
  public bool Equals(string? x, string? y)
  {
    if (x == null && y == null) return true;
    if (x == null || y == null) return false;
    return string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
  }

  public int GetHashCode(string obj)
  {
    return obj?.ToLower().GetHashCode() ?? 0;
  }
}

[PublicAPI]
[ExcludeFromCodeCoverage]
public class AttributeDictionary : Dictionary<string, string>
{
  public AttributeDictionary() : base(new CaseInsensitiveStringComparer())
  {
  }

  public AttributeDictionary(IDictionary<string, string> dictionary) 
    : base(dictionary, new CaseInsensitiveStringComparer())
  {
  }

  public new bool Remove(string key)
  {
    return base.Remove(key);
  }
}