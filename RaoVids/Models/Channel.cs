namespace RaoVids.Models
{
    /// <summary>
    /// The database model for channels we're tracking.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// The channel ID.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// The channel name.
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// The last time the channel was updated (for tracking new videos.)
        /// </summary>
        public DateTimeOffset LastUpdatedAt { get; set; }

        /// <summary>
        /// The video count - updated when we search for new videos, this saves us having to look
        /// up the count using the database every time it's displayed.
        /// </summary>
        public int VideoCount { get; set; }
    }
}
