namespace Abstractions.Interfaces;

public interface IMessagingGrain:IGrainWithIntegerKey
{
    Task InsertMessageAsync(MessageModel model);

    Task<List<MessageModel>> GetAllMessages();
    Task ClearStateAsync();
}