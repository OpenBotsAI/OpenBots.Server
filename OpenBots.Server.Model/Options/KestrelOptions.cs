namespace OpenBots.Server.Model.Options
{
    /// <summary>
    /// Options for KestrelOptions Configuration
    /// </summary>
    public class KestrelOptions
    {
        /// <summary>
        /// Configuration Name in App Settings For Kestrel
        /// </summary>
        public const string Kestrel = "Kestrel";

        public KestrelOptions()
        {
            IsEnabled = false;
            UseIISIntegration = false;
            Certificates = null;
            Port = 443;
            IPAddress = "Any";
        }

        /// <summary>
        /// Used to enable kestrel configuration in the application
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Specifies the port number to be used by application
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Sets the IP Address of the host
        /// </summary>
        public string IPAddress { get; set; }

        /// <summary>
        /// References the CertificatesOptions subsection in appsettings
        /// </summary>
        public CertificatesOptions Certificates { get; set; }

        /// <summary>
        /// Confgures Kestrel to use IIS integration
        /// </summary>
        public bool UseIISIntegration { get; set; }

    }

    /// <summary>
    /// Options for Kestrel Certificates
    /// </summary>
    public class CertificatesOptions
    {
        /// <summary>
        /// Configuration Name in App Settings For Certificates
        /// </summary>
        public const string Certificates = "Certificates";

        /// <summary>
        /// Path for certificate file
        /// </summary>
        public string Path { get; set;}

        /// <summary>
        /// Password to verify kestrel certificate
        /// </summary>
        public string Password { get; set; }
    }
}
