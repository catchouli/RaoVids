using Config.Net;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using RaoVids.Components;

namespace RaoVids;

public class Program
{
    public static async Task Main(string[] args)
    {
        // Load configuration from environment variables/.env file.
        var appSettings = new ConfigurationBuilder<IAppSettings>()
            .UseEnvironmentVariables()
            .UseDotEnvFile()
            .Build();

        // Initialise youtube service.
        var youtubeService = ConfigureYoutubeAPI(appSettings);

        // Iterate all of a channel's videos.
        var listSearch = youtubeService.Search.List("snippet");
        listSearch.ChannelId = "UCjM-Wd2651MWgo0s5yNQRJA";
        listSearch.Order = SearchResource.ListRequest.OrderEnum.Date;
        //listSearch.MaxResults = 50;

        while (true)
        {
            Console.WriteLine("Request page");

            var res = listSearch.Execute();

            Console.WriteLine("Results: " + res.Items.Count);

            foreach (var item in res.Items)
            {
                Console.WriteLine($"Video ID: {item.Id.VideoId}, Title: {item.Snippet.Title}");
            }

            // End if there's no more pages.
            if (res.NextPageToken == null)
            {
                Console.WriteLine("No next page token, end of search results");
                break;
            }

            // Otherwise, request the next page.
            listSearch.PageToken = res.NextPageToken;

            Thread.Sleep(1000);
        }

        Console.WriteLine("Done");
        return;

        Console.WriteLine($"Database conn string: {appSettings.DatabaseConnString}");

        // Create web application.
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add Entity Framework.
        builder.Services.AddEntityFrameworkNpgsql()
            .AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(appSettings.DatabaseConnString));

        // Build app.
        var app = builder.Build();

        // Check connection to database, which isn't strictly necessary but should make
        // configuring the service on Kubernetes a bit more painless if it goes wrong for some
        // reason.
        using (var scope = app.Services.CreateScope())
        {
            using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            try
            {
                dbContext.Database.OpenConnection();
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to open database connection: " + e.Message);
            }

            Console.WriteLine("Connected to database successfully, starting web app");
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
        app.UseAntiforgery();
        app.MapStaticAssets();
        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }

    /// <summary>
    /// Configure and authenticate with youtube API, returning a new client instance.
    /// </summary>
    /// <param name="appSettings">The application settings</param>
    /// <returns>A newly initialised YoutubeService</returns>
    private static YouTubeService ConfigureYoutubeAPI(IAppSettings appSettings)
    {
        //var json = $@"{{
                //""installed"": {{
                    //""client_id"": ""{appSettings.GoogleClientId}"",
                    //""client_secret"": ""{appSettings.GoogleClientSecret}""
                //}}
            //}}";

        // Configure YouTube API key.
        //var googleClientSecrets = GoogleClientSecrets.FromStream(
            //new MemoryStream(System.Text.Encoding.UTF8.GetBytes(
                //$@"{{
                        //""installed"": {{
                            //""client_id"": ""{appSettings.GoogleClientId}"",
                            //""client_secret"": ""{appSettings.GoogleClientSecret}""
                        //}}
                    //}}")
            //)
        //);

        //var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
            //googleClientSecrets.Secrets,
            //new[] { "https://www.googleapis.com/auth/youtube" },
            //"user",
            //CancellationToken.None,
            //new FileDataStore("RaoVids.GoogleOAuth")
        //).Result;

        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApplicationName = "RaoVids",
            ApiKey = appSettings.YoutubeDataApiKey
        });

        return youtubeService;
    }

}
