namespace Tweetter.Functions;

public class TweetterPoster
{
    private readonly TwitterApiOptions _options;

    public TweetterPoster(IOptions<TwitterApiOptions> options)
    {
        _options = options.Value;
    }

    [Function("TweetterPoster")]
    public async Task<TweetterPosterResponse> Run(
        [QueueTrigger("post-to-twitter", Connection = "")]
            string tweetContent,
        FunctionContext executionContext
    )
    {
        var logger = executionContext.GetLogger("TweetterTrigger");
        logger.LogInformation("{TweetterPost} function executed at: {DateTime}",
            nameof(TweetterPoster),
            DateTime.Now.ToString(CultureInfo.InvariantCulture));

        if (tweetContent is not { Length: > 0 })
        {
            logger.LogError("TweetContent is invalid!");
            return null;
        }

        try
        {
            return await PostToTwitter(tweetContent, logger);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred: {Message}", exception.Message);
            throw;
        }
    }

    private async Task<TweetterPosterResponse> PostToTwitter(string tweetContent, ILogger logger)
    {
        var data = JsonSerializer.Deserialize<Tweet>(tweetContent);
        if (data is null)
        {
            logger.LogWarning("Tweet data is null!");
            return null;
        }

        var (content, imageUrl, entryId) = data;

        var userClient = new TwitterClient(
            _options.ConsumerKey,
            _options.ConsumerSecret,
            _options.AccessToken,
            _options.AccessSecret
        );

        var httpClient = new HttpClient();

        if (imageUrl is { Length: > 0 })
        {
            var mediaCategory = GetMediaCategory(imageUrl);

            logger.LogInformation("Uploading {MediaCategory} to Twitter", mediaCategory);

            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

            var uploadedImage =
                await userClient.Upload.UploadTweetImageAsync(
                    new UploadTweetImageParameters(imageBytes)
                    {
                        MediaType = MediaType.Media,
                        MediaCategory = mediaCategory
                    });

            var tweetWithTextAndImage = await userClient.Tweets.PublishTweetAsync(
                new PublishTweetParameters(content)
                {
                    Medias = { uploadedImage }
                });

            logger.LogInformation("Tweet posted successfully {TweetUrl}", tweetWithTextAndImage.Url);
        }
        else
        {
            var tweetWithText = await userClient.Tweets.PublishTweetAsync(content);

            logger.LogInformation("Tweet posted successfully {TweetUrl}", tweetWithText.Url);
        }

        return new TweetterPosterResponse
        {
            EntryId = entryId
        };
    }

    private static MediaCategory GetMediaCategory(string assetUrl)
    {
        var fileExtension = assetUrl.Split('.').Last();

        return fileExtension == "gif" ? MediaCategory.Gif : MediaCategory.Image;
    }
}
