using MoneyTrace.Application.Domain;

namespace MoneyTrace.Application.Common;

internal sealed class UserCreatedEvent(UserEntity item) : DomainEvent
{
    public UserEntity Item { get; } = item;
}