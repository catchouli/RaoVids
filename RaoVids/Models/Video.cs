namespace RaoVids.Models
{
    /// <summary>
    /// The database model for videos from tracked channels.
    /// </summary>
    public class Video
    {
        /// <summary>
        /// The video ID.
        /// </summary>
        public string VideoId { get; set; }

        /// <summary>
        /// The channel ID that contains this video.
        /// </summary>
        public string ChannelId { get; set; }

        /// <summary>
        /// The video title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The video description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// When the video was published.
        /// </summary>
        public DateTimeOffset PublishedAt { get; set; }

        /// <summary>
        /// When we discovered this video.
        /// </summary>
        public DateTimeOffset DiscoveredAt { get; set; }
    }
}
