namespace Six.SvgKit;

public record SvgMetadata
{
  public string Id { get; init; } = string.Empty;
  public string Name { get; init; } = string.Empty;
  public string FileName { get; init; } = string.Empty;
}