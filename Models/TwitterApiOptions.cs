namespace Tweetter.Models;

public class TwitterApiOptions
{
    public const string Twitter = "Twitter";

    public string ConsumerKey { get; set; }

    public string ConsumerSecret { get; set; }

    public string AccessToken { get; set; }

    public string AccessSecret { get; set; }
}
