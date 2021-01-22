namespace OpenBots.Server.Model.Options
{
    /// <summary>
    /// Options for IPFencing Configuration
    /// </summary>
    /// <remarks>
    /// IPFencing manages the HTTP requests that are allowed to be made to the server based on IP and Headers
    /// </remarks>
    public class IPFencingOptions
    {
        public const string IPFencing = "IPFencing";

        /// <summary>
        /// Determines when IPFencing rules will be applied 
        /// </summary>
        /// <remarks>This value can be set to Disabled, LoginOnly, or EveryRequest</remarks>
        public string IPFencingCheck { get; set; }
    }
}
