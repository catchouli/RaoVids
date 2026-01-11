using Microsoft.EntityFrameworkCore;
using RaoVids.Models;

namespace RaoVids.Services
{
    /// <summary>
    /// Service for managing known channels.
    /// </summary>
    public class ChannelService
    {
        private readonly AppDbContext _dbContext;
        private readonly RaoVidsYoutubeService _ytService;
        private readonly LogService _logService;

        public ChannelService(AppDbContext dbContext, RaoVidsYoutubeService ytService, LogService logService)
        {
            _dbContext = dbContext;
            _ytService = ytService;
            _logService = logService;
        }

        /// <summary>
        /// Get the total number of channels in the database.
        /// </summary>
        /// <returns>The channel count</returns>
        public async Task<int> GetChannelCountAsync()
        {
            return await _dbContext.Channels.CountAsync();
        }

        /// <summary>
        /// Add a channel by ID or name.
        /// </summary>
        /// <param name="channelIdOrName">The channel ID or name.</param>
        public async Task AddChannelAsync(string channelIdOrName)
        {
            // Look up channel details using youtube service.
            var channelDetails = _ytService.GetChannelDetails(channelIdOrName);

            if (channelDetails == null)
            {
                throw new ServiceException("No such channel " + channelIdOrName);
            }

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

            try
            {
                await _dbContext.Channels.AddAsync(channel);
                await _dbContext.SaveChangesAsync();

                await _logService.AddLogMessageAsync($"Added channel {channel.ChannelName} ({channel.ChannelId})");
            }
            catch (DbUpdateException e)
            {
                throw new ServiceException(e.InnerException?.Message ?? e.Message, e);
            }
        }

        /// <summary>
        /// Get all channels
        /// </summary>
        /// <returns>An enumerable of all the registered channels</returns>
        public async Task<IEnumerable<Channel>> GetAllChannelsAsync()
        {
            return await _dbContext.Channels.ToListAsync();
        }
    }
}
