namespace OpenBots.Server.Model.Options
{
    public class KestrelOptions
    {
        public const string Kestrel = "Kestrel";

        public KestrelOptions()
        {
            IsEnabled = false;
            UseIISIntegration = false;
            Certificates = null;
            Port = 443;
            IPAddress = "Any";
        }

        public bool IsEnabled { get; set; }
        public int Port { get; set; }
        public string IPAddress { get; set; }
        public CertificatesOptions Certificates { get; set; }

        public bool UseIISIntegration { get; set; }

    }

    public class CertificatesOptions
    {
        public const string Certificates = "Certificates";
        
        public string Path { get; set;}
        public string Password { get; set; }
    }
}
