namespace Abstractions.Interfaces;

public interface IObserverMessagingGrain:IGrainObserver
{
    Task MessageStored();
}