using System.IO;

namespace OpenBots.Server.ViewModel
{
    public class FileObjectViewModel 
    {
        public virtual string Name { get; set; }        
        public virtual string ContentType { get; set; }
        public virtual string StoragePath { get; set; }
        public virtual FileStream BlobStream {get; set; }
    }
}