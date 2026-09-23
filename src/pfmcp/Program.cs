public partial class Program
{
    static partial void AddServices(IServiceCollection services)
    {
        services.AddHttpClient(IndexCatalog.HttpClientName, client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("pfmcp");
            client.Timeout = TimeSpan.FromSeconds(60);
        });
        services.AddSingleton<IndexCatalog>();
    }

    static partial void ConfigureApp(AppServiceConfig appServiceConfig)
    {
        appServiceConfig.SetApplicationName("pfmcp");
        appServiceConfig.AddCommand<SearchCommand>("search")
            .WithDescription("Search one or more Pagefind indexes.");
        appServiceConfig.AddCommand<InspectCommand>("inspect")
            .WithDescription("Show Pagefind bundle metadata.");
        appServiceConfig.AddCommand<ServeCommand>("serve")
            .WithDescription("Start the MCP stdio server.");
        appServiceConfig.SetDefaultCommand<ServeCommand>();
    }
}
