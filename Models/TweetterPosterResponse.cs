namespace Tweetter.Models;

public class TweetterPosterResponse
{
    [QueueOutput("contentful-entry-updater")]
    public string EntryId { get; set; }
}
