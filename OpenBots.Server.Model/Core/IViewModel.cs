namespace OpenBots.Server.Model.Core
{
    public interface IViewModel<T, TViewModel>
         where T : class, new()
         where TViewModel : class, new()
    {
        TViewModel Map(T entity);
    }
}
