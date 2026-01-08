using Config.Net;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Microsoft.EntityFrameworkCore;
using RaoVids.Components;
using RaoVids.Services;

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

        // Initialise youtube API.
        var ytApi = ConfigureYoutubeAPI(appSettings);

        // Create web application.
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        // Add Entity Framework.
        builder.Services
            .AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(appSettings.DatabaseConnString));

        // Add application services.
        builder.Services.AddSingleton(ytApi);
        builder.Services.AddScoped<RaoVidsYoutubeService>();
        builder.Services.AddScoped<ChannelsService>();

        // Build app.
        var app = builder.Build();

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
        var youtubeService = new YouTubeService(new BaseClientService.Initializer()
        {
            ApplicationName = "RaoVids",
            ApiKey = appSettings.YoutubeDataApiKey
        });

        return youtubeService;
    }

    /// <summary>
    /// A little test of the youtube service to make sure we can get all channel videos.
    /// </summary>
    private void TestYoutubeService(RaoVidsYoutubeService ytService)
    {
        // Test calls.
        var lastUpdated = new DateTimeOffset(2025, 12, 2, 4, 30, 49, TimeSpan.Zero);

        // Get channel details for channel "raocow", including the uploads playlist ID.
        var channel = ytService.GetChannelDetails("raocow");

        // Get some of the videos from the playlist.
        var videos = ytService.GetPlaylistVideos(channel.ContentDetails.RelatedPlaylists.Uploads);

        Console.WriteLine("Got " + videos.Items.Count + " videos");
        foreach (var video in videos.Items)
        {
            Console.WriteLine($"Got video: {video.Snippet.Title} ({video.Snippet.PublishedAtDateTimeOffset}");
        }

        while (videos.NextPageToken != null)
        {
            Thread.Sleep(1000);

            Console.WriteLine("Requesting next page...");

            videos = ytService.GetPlaylistVideos(channel.ContentDetails.RelatedPlaylists.Uploads, videos.NextPageToken);

            Console.WriteLine("Got " + videos.Items.Count + " videos");
            foreach (var video in videos.Items)
            {
                if (video.Snippet.PublishedAtDateTimeOffset < lastUpdated)
                {
                    Console.WriteLine($"Got to last upload date: {video.Snippet.PublishedAtDateTimeOffset}");
                    videos.NextPageToken = null;
                    break;
                }

                Console.WriteLine($"Got video: {video.Snippet.Title} ({video.Snippet.PublishedAtDateTimeOffset}");
            }
        }

    }
}
