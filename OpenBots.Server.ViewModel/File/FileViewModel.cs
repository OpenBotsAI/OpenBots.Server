using System.IO;

namespace OpenBots.Server.ViewModel.File
{
    public class FileViewModel
    {
        public virtual string Name { get; set; }
        public virtual string ContentType { get; set; }
        public virtual string StoragePath { get; set; }
        public virtual FileStream? Content { get; set; }
    }
}
