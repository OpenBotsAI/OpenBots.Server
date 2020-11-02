namespace OpenBots.Server.Model.Configuration
{
    /// <summary>
    /// GlobalQueueMaxRetryCountOptions Configuration model
    /// </summary>
    public class GlobalQueueMaxRetryCountOptions
    {
        /// <summary>
        /// Configuration Name
        /// </summary>
        public const string GlobalQueueMaxRetryCount = "Queue.Global";

        /// <summary>
        /// Max Number of Retries
        /// </summary>
        public int DefaultMaxRetryCount { get; set; }
    }
}
