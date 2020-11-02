namespace OpenBots.Server.Web.Hubs
{
    public interface IHubManager
    {
        public void StartNewRecurringJob(string scheduleSerializeObject);        
    }
}
