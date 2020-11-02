namespace OpenBots.Server.Business
{
    public interface IQueueManager : IManager
    {
        bool CheckReferentialIntegrity(string id);
    }
}
