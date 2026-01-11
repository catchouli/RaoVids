using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;

namespace RaoVids.Services;

/// <summary>
/// A service that wraps Google's YouTube API and implements functionality to request channel
/// videos and such.
/// </summary>
public class RaoVidsYoutubeService
{
    private readonly YouTubeService _apiService;

    /// <summary>
    /// Construct a new RaoVids YouTube service.
    /// </summary>
    public RaoVidsYoutubeService(YouTubeService apiService)
    {
        _apiService = apiService;
    }

    /// <summary>
    /// Get a channel's details.
    /// </summary>
    /// <param name="usernameOrId">The channel username or ID</param>
    /// <param name="parts">The parts to request (default "snippet,contentDetails")</param>
    /// <returns>The found channel details, or null if channel was not found</returns>
    public Channel GetChannelDetails(string usernameOrId, string parts = "snippet,contentDetails")
    {
        Channel? foundChannel = null;

        // Try looking up channel by ID.
        try
        {
            // Get channel details.
            var channelRequest = _apiService.Channels.List("snippet,contentDetails");
            channelRequest.Id = usernameOrId;

            var channelResult = channelRequest.Execute();

            if (channelResult.Items != null && channelResult.Items.Count != 0)
            {
                foundChannel = channelResult.Items[0];
            }
        }
        catch
        {
            // Ignore errors here, we'll try username next.
            // Technically we could be getting other errors, and ideally we should handle them,
            // but it's probably not that important for this as long as it works.
        }

        // Try looking up channel by username if not found already by ID.
        // Annoyingly this introduces some repetition, but the youtube API can't take both at once.
        if (foundChannel == null)
        {
            try
            {
                // Get channel details.
                var channelRequest = _apiService.Channels.List(parts);
                channelRequest.ForUsername = usernameOrId;

                var channelResult = channelRequest.Execute();

                if (channelResult.Items != null && channelResult.Items.Count != 0)
                {
                    foundChannel = channelResult.Items[0];
                }
            }
            catch
            {
                // Ignore errors here too.
            }
        }

        // Finally, try channel URL (sigh).
        if (foundChannel == null)
        {
            try
            {
                // Get channel details.
                var channelRequest = _apiService.Channels.List(parts);
                channelRequest.ForHandle = usernameOrId;

                var channelResult = channelRequest.Execute();

                if (channelResult.Items != null && channelResult.Items.Count != 0)
                {
                    foundChannel = channelResult.Items[0];
                }
            }
            catch
            {
                // Ignore errors here too.
            }
        }

        return foundChannel;
    }

    /// <summary>
    /// Get the videos in a playlist.
    /// </summary>
    /// <param name="playlistId">The playlist ID</param>
    public PlaylistItemListResponse GetPlaylistVideos(string playlistId, string? pageToken = null, int maxResults = 50)
    {
        var playlistItemsRequest = _apiService.PlaylistItems.List("id,snippet");
        playlistItemsRequest.PlaylistId = playlistId;
        playlistItemsRequest.PageToken = pageToken;
        playlistItemsRequest.MaxResults = maxResults;

        var result = playlistItemsRequest.Execute();

        return result;
    }
}
