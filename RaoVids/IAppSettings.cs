namespace RaoVids
{
    /// <summary>
    /// Application settings.
    /// </summary>
    public interface IAppSettings
    {
        /// <summary>
        /// A database connection string in the format
        /// "Host=myserver; Database=mydb; Username=mylogin; Password=mypass".
        /// </summary>
        string DatabaseConnString { get; }

        /// <summary>
        /// API key for youtube data API.
        /// </summary>
        string YoutubeDataApiKey { get; }
    }
}
