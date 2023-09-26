namespace moah_api.Models;

public class OutgoingArchiveEntry
{
    public required string Id { get; set; }
    public required DateTime Created_at { get; set; }
    public required string? Snippet { get; set; }
}