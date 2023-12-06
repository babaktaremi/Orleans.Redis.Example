using Abstractions;
using Abstractions.Interfaces;
using Microsoft.Extensions.Logging;
using Orleans.Providers;
using Orleans.Runtime;
using Orleans.Utilities;

namespace Grains;

[StorageProvider(ProviderName = "Redis")]
public class MessagingGrain:Grain,IMessagingGrain
{
    private readonly IPersistentState<List<MessageModel>> _messages;
    private readonly ILogger<MessagingGrain> _logger;
    private readonly ObserverManager<IObserverMessagingGrain> _observerManager;

    public MessagingGrain(
        [PersistentState(stateName:"message",storageName:"Redis")]
        IPersistentState<List<MessageModel>> messages, ILogger<MessagingGrain> logger)
    {
        _messages = messages;
        _logger = logger;
        _observerManager = new ObserverManager<IObserverMessagingGrain>
            (TimeSpan.FromMinutes(60), _logger);
    }


    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await _messages.ReadStateAsync();
        _logger.LogWarning("Grain Activated. ID: {id}",this.GetPrimaryKeyLong());
    }


    public async Task InsertMessageAsync(MessageModel model)
    {
        _messages.State.Add(model);
        await _messages.WriteStateAsync();

        await Notify();
    }

    public Task<List<MessageModel>> GetAllMessages()
    {
        return Task.FromResult(_messages.State);
    }

    public async Task ClearStateAsync()
    {
        await _messages.ClearStateAsync();
    }

    public Task Subscribe(IObserverMessagingGrain observerMessagingGrain)
    {
       _observerManager.Subscribe(observerMessagingGrain,observerMessagingGrain);
       return Task.CompletedTask;
    }

    public Task Unsubscribe(IObserverMessagingGrain observerMessagingGrain)
    {
       _observerManager.Unsubscribe(observerMessagingGrain);
       return Task.CompletedTask;
    }

    public async Task Notify()
    {
        await _observerManager.Notify(grain => grain.MessageStored());
    }
}