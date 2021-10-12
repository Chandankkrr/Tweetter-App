var host = new HostBuilder()
    .ConfigureServices(services =>
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddAzureAppConfiguration("azure app configuration connection string here")
            .Build();

        services.Configure<ContentfulApiOptions>(configuration.GetSection(ContentfulApiOptions.Contentful));
        services.Configure<TwitterApiOptions>(configuration.GetSection(TwitterApiOptions.Twitter));
    })
    .ConfigureFunctionsWorkerDefaults()
    .Build();

host.Run();
