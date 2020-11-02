using System;

namespace OpenBots.Server.Model.Core
{
    public class NameIDPair : INameIDPair
    {
        public NameIDPair()
        { }

        public Guid? Id { get ; set ; }
        public string Name { get ; set; }
    }
}
