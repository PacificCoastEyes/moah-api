namespace moah_api.Models;

public class IncomingUpdatedEntry
{
    public required string Id { get; set; }
    public required string Content { get; set; }
    public required string Snippet { get; set; }
}