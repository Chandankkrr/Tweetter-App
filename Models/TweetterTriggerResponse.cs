namespace Tweetter.Models;

public class TweetterTriggerResponse
{
    [QueueOutput("post-to-twitter")]
    public string TweetContent { get; set; }
}
