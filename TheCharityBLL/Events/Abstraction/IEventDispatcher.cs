namespace TheCharityBLL.Events.Abstraction
{
    public interface IEventDispatcher
    {
        Task DispatchAsync<TEvent>(TEvent @event);
    }
}
