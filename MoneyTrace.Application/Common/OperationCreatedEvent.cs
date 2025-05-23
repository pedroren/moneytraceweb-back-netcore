namespace MoneyTrace.Application.Common;

using MoneyTrace.Application.Domain;

internal sealed class OperationCreatedEvent(OperationEntity item) : DomainEvent
{
    public OperationEntity Item { get; } = item;
}