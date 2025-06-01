namespace MoneyTrace.Application.Common;

using MediatR;
using MoneyTrace.Application.Domain;

public class DomainEventNotification<TDomainEvent> : INotification
    where TDomainEvent : DomainEvent
{
    public DomainEventNotification(TDomainEvent domainEvent)
    {
        DomainEvent = domainEvent;
    }

    public TDomainEvent DomainEvent { get; }
}