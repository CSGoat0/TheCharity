namespace TheCharityBLL.Events.Abstraction
{
    public interface IEventHandler<TEvent>
    {
        Task HandleAsync(TEvent @event);
    }
}
