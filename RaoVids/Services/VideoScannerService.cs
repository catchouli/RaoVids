using Microsoft.EntityFrameworkCore;
using RaoVids.Models;

namespace RaoVids.Services;

/// <summary>
/// Background service that periodically scans registered channels for new videos.
/// </summary>
public class VideoScannerService : BackgroundService
{
    private readonly ILogger<VideoScannerService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _scanInterval = TimeSpan.FromMinutes(5);

    public VideoScannerService(ILogger<VideoScannerService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("VideoScannerService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ScanChannelsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while scanning channels.");
            }

            // Wait for the specified interval before scanning again
            await Task.Delay(_scanInterval, stoppingToken);
        }

        _logger.LogInformation("VideoScannerService is stopping.");
    }

    private async Task ScanChannelsAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var ytService = scope.ServiceProvider.GetRequiredService<RaoVidsYoutubeService>();
            var logService = scope.ServiceProvider.GetRequiredService<LogService>();

            // Get all registered channels
            var channels = await dbContext.Channels.ToListAsync(stoppingToken);

            _logger.LogInformation($"Scanning {channels.Count} channels for new videos.");

            foreach (var channel in channels)
            {
                try
                {
                    await ScanChannelAsync(channel, ytService, dbContext, logService, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"An error occurred while scanning channel {channel.ChannelName}.");
                    await logService.AddLogMessageAsync($"Error scanning channel {channel.ChannelName}: {ex.Message}");
                }
            }
        }
    }

    private async Task ScanChannelAsync(Channel channel, RaoVidsYoutubeService ytService, AppDbContext dbContext, LogService logService, CancellationToken stoppingToken)
    {
        // Get channel details to find the uploads playlist
        var channelDetails = ytService.GetChannelDetails(channel.ChannelId);
        if (channelDetails == null)
        {
            await logService.AddLogMessageAsync($"Could not find channel {channel.ChannelName} ({channel.ChannelId})");
            return;
        }

        var uploadsPlaylistId = channelDetails.ContentDetails.RelatedPlaylists.Uploads;

        // If this is the first scan (LastUpdatedAt is default/zero), fetch all videos
        bool isFirstScan = channel.LastUpdatedAt == default(DateTimeOffset);

        // Fetch all videos from the playlist
        string pageToken = null;
        int addedVideoCount = 0;

        do
        {
            var playlistVideos = ytService.GetPlaylistVideos(uploadsPlaylistId, pageToken);

            if (playlistVideos.Items == null || playlistVideos.Items.Count == 0)
            {
                break;
            }

            foreach (var playlistItem in playlistVideos.Items)
            {
                // Stop if we've reached videos older than the last update (unless it's the first scan)
                if (!isFirstScan && playlistItem.Snippet.PublishedAtDateTimeOffset < channel.LastUpdatedAt)
                {
                    pageToken = null; // Force exit from loop
                    break;
                }

                // Check if video already exists in database
                var existingVideo = await dbContext.Videos
                    .FirstOrDefaultAsync(v => v.VideoId == playlistItem.Snippet.ResourceId.VideoId, stoppingToken);

                if (existingVideo == null)
                {
                    // Add new video
                    var video = new Video
                    {
                        VideoId = playlistItem.Snippet.ResourceId.VideoId,
                        ChannelId = channel.ChannelId,
                        Title = playlistItem.Snippet.Title,
                        Description = playlistItem.Snippet.Description,
                        PublishedAt = playlistItem.Snippet.PublishedAtDateTimeOffset ?? DateTimeOffset.UtcNow,
                        DiscoveredAt = DateTimeOffset.UtcNow
                    };

                    await dbContext.Videos.AddAsync(video, stoppingToken);
                    addedVideoCount++;
                }
            }

            // If we found videos and stopped early (not first scan), don't get next page
            if (!isFirstScan && pageToken == null)
            {
                break;
            }

            pageToken = playlistVideos.NextPageToken;

            // Be respectful to the API with a small delay
            await Task.Delay(100, stoppingToken);
        } while (!string.IsNullOrEmpty(pageToken) && !stoppingToken.IsCancellationRequested);

        // Update channel's last updated time and video count
        channel.LastUpdatedAt = DateTimeOffset.UtcNow;
        channel.VideoCount = await dbContext.Videos
            .Where(v => v.ChannelId == channel.ChannelId)
            .CountAsync(stoppingToken)
            + addedVideoCount;

        await dbContext.SaveChangesAsync(stoppingToken);

        string message = isFirstScan
            ? $"Scanned channel {channel.ChannelName}: found {addedVideoCount} new videos (total: {channel.VideoCount})"
            : $"Scanned channel {channel.ChannelName}: found {addedVideoCount} new videos";

        await logService.AddLogMessageAsync(message);
        _logger.LogInformation(message);
    }
}
