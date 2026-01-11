using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RaoVids.Models
{
    /// <summary>
    /// The database model for log messages.
    /// </summary>
    public class LogMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// The time of the log message.
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { get; set; }
    }
}
