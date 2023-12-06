using Abstractions.Interfaces;

namespace Client.Api;

public class MessagingGrainSubscriber:IHostedService,IObserverMessagingGrain
{
    readonly ILogger<MessagingGrainSubscriber> _logger;
    readonly IClusterClient _clusterClient;

    public MessagingGrainSubscriber(ILogger<MessagingGrainSubscriber> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var messagingGrain = _clusterClient.GetGrain<IMessagingGrain>(0);
        var observerReference = _clusterClient.CreateObjectReference<IObserverMessagingGrain>(this);

        await messagingGrain.Subscribe(observerReference);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        var messagingGrain = _clusterClient.GetGrain<IMessagingGrain>(0);
        var observerReference = _clusterClient.CreateObjectReference<IObserverMessagingGrain>(this);

        await messagingGrain.Unsubscribe(observerReference);
    }

    public Task MessageStored()
    {
        _logger.LogWarning("GRAIN 0 IS INVOKED!!!");
        return Task.CompletedTask;
    }
}