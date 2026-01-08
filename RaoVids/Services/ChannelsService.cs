using Microsoft.EntityFrameworkCore;
using RaoVids.Models;

namespace RaoVids.Services
{
    /// <summary>
    /// Service for managing known channels.
    /// </summary>
    public class ChannelsService
    {
        private readonly AppDbContext _dbContext;
        private readonly RaoVidsYoutubeService _ytService;

        public ChannelsService(AppDbContext dbContext, RaoVidsYoutubeService ytService)
        {
            _dbContext = dbContext;
            _ytService = ytService;
        }

        /// <summary>
        /// Add a channel by ID or name.
        /// </summary>
        /// <param name="channelIdOrName">The channel ID or name.</param>
        public async Task AddChannel(string channelIdOrName)
        {
            // Look up channel details using youtube service.
            var channelDetails = _ytService.GetChannelDetails(channelIdOrName);

            // Get channel ID and name.
            var channelId = channelDetails.Id;
            var channelName = channelDetails.Snippet.Title;

            if (channelId == null)
            {
                throw new ServiceException("Failed to determine channel ID for channel " + channelIdOrName);
            }

            if (channelName == null)
            {
                throw new ServiceException("Failed to determine channel ID for channel " + channelIdOrName);
            }

            // Add channel if it doesn't already exist.
            var channel = new Channel()
            {
                ChannelId = channelId,
                ChannelName = channelName,
            };

            _dbContext.Channels.Add(channel);

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new ServiceException(e.InnerException?.Message ?? e.Message, e);
            }
        }
    }
}
