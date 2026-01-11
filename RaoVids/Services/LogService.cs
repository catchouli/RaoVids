using Microsoft.EntityFrameworkCore;
using RaoVids.Models;

namespace RaoVids.Services
{
    /// <summary>
    /// Service for updating the log.
    /// </summary>
    public class LogService
    {
        private readonly AppDbContext _dbContext;

        public LogService(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <summary>
        /// Add a new log message. Don't add these too often, as it will slow down the application
        /// and bloat the database. The application log should be used for most logs, just
        /// information the user needs to see should be added here, and it will be displayed on
        /// the log page.
        /// </summary>
        /// <param name="message">The log message to add.</param>
        public async Task AddLogMessageAsync(string message)
        {
            try
            {
                var logMessage = new LogMessage
                {
                    Time = DateTimeOffset.UtcNow,
                    Message = message
                };

                await _dbContext.LogMessages.AddAsync(logMessage);
                await _dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException e)
            {
                throw new ServiceException(e.InnerException?.Message ?? e.Message, e);
            }
        }

        public async Task<IEnumerable<LogMessage>> GetLogMessagesAsync(int maxResults)
        {
            return await _dbContext.LogMessages
                .OrderByDescending(m => m.Time)
                .Take(maxResults)
                .ToListAsync();
        }
    }
}
