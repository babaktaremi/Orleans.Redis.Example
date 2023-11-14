using Abstractions;
using Abstractions.Interfaces;
using Orleans.Providers;
using Orleans.Runtime;

namespace Grains;

[StorageProvider(ProviderName = "Redis")]
public class MessagingGrain:Grain,IMessagingGrain
{
    private readonly IPersistentState<List<MessageModel>> _messages;

    public MessagingGrain(
        [PersistentState(stateName:"message",storageName:"Redis")]
        IPersistentState<List<MessageModel>> messages)
    {
        _messages = messages;
    }


    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await _messages.ReadStateAsync();
    }


    public async Task InsertMessageAsync(MessageModel model)
    {
        _messages.State.Add(model);
        await _messages.WriteStateAsync();
    }

    public Task<List<MessageModel>> GetAllMessages()
    {
        return Task.FromResult(_messages.State);
    }
}