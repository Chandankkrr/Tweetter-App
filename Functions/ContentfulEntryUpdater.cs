namespace Tweetter.Functions;

public class ContentfulEntryUpdater
{
    private readonly ContentfulApiOptions _options;

    public ContentfulEntryUpdater(IOptions<ContentfulApiOptions> options)
    {
        _options = options.Value;
    }

    [Function("ContentfulEntryUpdater")]
    public async Task Run(
        [QueueTrigger("contentful-entry-updater", Connection = "")]
            string entryId,
        FunctionContext context)
    {
        var logger = context.GetLogger("ContentfulEntryUpdater");
        logger.LogInformation("{ContentfulEntryUpdater} function executed at: {DateTime}",
            nameof(ContentfulEntryUpdater),
            DateTime.Now.ToString(CultureInfo.InvariantCulture));

        try
        {
            await UpdateContentfulEntry(entryId, logger);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occurred: {Message}", exception.Message);
            throw;
        }
    }

    private async Task UpdateContentfulEntry(string entryId, ILogger logger)
    {
        var httpClient = new HttpClient();
        var client = new ContentfulManagementClient(httpClient, _options);

        var entry = await client.GetEntry(entryId);
        entry.Fields.isPublished["en-US"] = true;

        var version = (int)entry.SystemProperties.Version!;

        logger.LogInformation("Updating Contentful entry {EntryId}", entryId);

        await client.CreateOrUpdateEntry(entry, version: version);

        logger.LogInformation("Publishing Contentful entry {EntryId}", entryId);

        await client.PublishEntry(entryId, version + 1);

        logger.LogInformation("Successfully updated Contentful entry {EntryId}", entryId);
    }
}