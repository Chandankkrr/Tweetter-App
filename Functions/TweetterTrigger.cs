namespace Tweetter.Functions;

public class TweetterTrigger
{
    private readonly ContentfulApiOptions _options;

    public TweetterTrigger(IOptions<ContentfulApiOptions> options)
    {
        _options = options.Value;
    }

    [Function("TweetterTrigger")]
    public async Task<TweetterTriggerResponse> Run([TimerTrigger("0 0 12 * * *")] FunctionContext context)
    {
        var logger = context.GetLogger("TweetterPoster");
        logger.LogInformation("{TweetterTrigger} function executed at: {DateTime}",
            nameof(TweetterTrigger),
            DateTime.Now.ToString(CultureInfo.InvariantCulture));

        try
        {
            return await CheckForContentAndProcess(logger);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred: {Message}", exception.Message);
            throw;
        }
    }

    private async Task<TweetterTriggerResponse> CheckForContentAndProcess(ILogger logger)
    {
        var httpClient = new HttpClient();
        var client = new ContentfulClient(httpClient, _options);

        var builder = QueryBuilder<TweetterContent>.New
            .ContentTypeIs("tweetter")
            .FieldEquals(f => f.IsPublished, "false")
            .FieldLessThanOrEqualTo(f => f.PublishDateTime, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"));

        logger.LogInformation("Calling Contentful API to check for new entries");

        var entries = await client.GetEntries(builder);

        if (!entries.Items.Any())
        {
            logger.LogInformation("No tweets to post");
            return null;
        }

        var entry = entries.Items.OrderBy(f => f.PublishDateTime).First();
        var tweetContent = entry.TweetContent.Content;
        var content = new StringBuilder();
        var imageIds = new List<string>();

        foreach (var node in tweetContent)
        {
            switch (node)
            {
                case Paragraph paragraph:
                    content.AppendLine(string.Join("", paragraph.Content.Select(c => ((Text)c).Value).ToList()));
                    break;

                case AssetStructure assetStructure:
                    imageIds.Add(assetStructure.Data.Target.SystemProperties.Id);
                    break;
            }
        }

        var assetExists = imageIds.Any();

        Asset asset;
        string imageUrl = string.Empty;

        if (assetExists)
        {
            asset = await client.GetAsset(imageIds.First());
            imageUrl = $"https:{asset.File.Url}";
        }

        var tweet = new Tweet(content.ToString(), imageUrl, entry.Sys.Id);
        var tweetBody = JsonSerializer.Serialize(tweet);

        logger.LogInformation("Add new message to post-to-twitter queue");

        return new TweetterTriggerResponse
        {
            TweetContent = tweetBody
        };
    }
}
