namespace Abstractions.Interfaces;

public interface IMessagingGrain:IGrainWithIntegerKey
{
    Task InsertMessageAsync(MessageModel model);

    Task<List<MessageModel>> GetAllMessages();
    Task ClearStateAsync();
    Task Subscribe(IObserverMessagingGrain observerMessagingGrain);
    Task Unsubscribe(IObserverMessagingGrain observerMessagingGrain);
    Task Notify();
}