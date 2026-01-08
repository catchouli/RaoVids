namespace RaoVids.Services
{
    /// <summary>
    /// Base exception type for service exceptions.
    /// </summary>
    public class ServiceException
        : Exception
    {
        public ServiceException()
        {
        }

        public ServiceException(string? message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
