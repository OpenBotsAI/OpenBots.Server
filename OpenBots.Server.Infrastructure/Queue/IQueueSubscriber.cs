namespace OpenBots.Server.Infrastructure
{
    public interface IQueueSubscriber
    {
        void Init();
        void Register();
    }
}