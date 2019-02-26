using System.Diagnostics;

namespace PrintPrince.Services
{
    /// <summary>
    /// Utility class for logging information to the Event Log.
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Writes an event log entry.
        /// </summary>
        /// <remarks>
        /// The entry will be made to the Application log with source ".NET Runtime" and ID 1000.
        /// </remarks>
        /// <param name="message">Message to log.</param>
        /// <param name="type">Type of entry.</param>
        public static void Log(string message, EventLogEntryType type)
        {
            EventLog.WriteEntry(".NET Runtime", $"Print Prince [{DomainManager.UserName}]: {message}", type, 1000);
        }
    }
}
