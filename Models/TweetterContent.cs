namespace Tweetter.Models;

public record TweetterContent(
    string Id,
    string Title,
    Document TweetContent,
    DateTime PublishDateTime,
    bool IsPublished,
    SystemProperties Sys
);
