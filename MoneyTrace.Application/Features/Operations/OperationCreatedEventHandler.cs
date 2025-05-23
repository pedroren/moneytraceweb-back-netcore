namespace MoneyTrace.Application.Operations;

using System;
using MediatR;
using MoneyTrace.Application.Common;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Accounts;
using MoneyTrace.Application.Features.Categories;
using MoneyTrace.Application.Features.Vendors;

/// <summary>
/// After an operation is created, update the account(s) balance.
/// </summary>
/// <remarks></remarks>
internal sealed class OperationCreatedEventHandler(IMediator mediator) : INotificationHandler<DomainEventNotification<OperationCreatedEvent>>
{
    public async Task Handle(DomainEventNotification<OperationCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;
        var operation = domainEvent.Item;

        Console.WriteLine($"Operation created: {operation.Title}");
        var categoryType = operation.Allocation.FirstOrDefault().Category.Type;
        int increaseDecrease = -1;        

        // Update account(s) balance
        if (operation.Type == OperationType.Simple)
        {
          switch (categoryType)
            {
                case CategoryType.Expense:
                    if (operation.Account.Type == AccountType.Credit)   
                    {
                        increaseDecrease = 1;
                    }
                    else
                    {
                        increaseDecrease = -1;
                    }
                    break;
                case CategoryType.Income:
                    if (operation.Account.Type == AccountType.Credit)   
                    {
                        increaseDecrease = -1;
                    }
                    else
                    {
                        increaseDecrease = 1;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(categoryType), categoryType, null);
            }  
        }

        var updateAccountBalCmd = new UpdateAccountBalanceCommand(operation.UserId, operation.AccountId, operation.TotalAmount*increaseDecrease);
        var result = await mediator.Send(updateAccountBalCmd, cancellationToken);
        //TODO: check result for errors
        // Inverse update on the Balance of the destination account if it is a transfer
        if (operation.Type == OperationType.Transfer && operation.DestinationAccountId.HasValue)
        {
            var updateDestAccountBalCmd = new UpdateAccountBalanceCommand(operation.UserId, operation.DestinationAccountId.Value, operation.TotalAmount * increaseDecrease * -1);
            var result2 = await mediator.Send(updateDestAccountBalCmd, cancellationToken);
            //TODO: check result2 for errors
        }

        return;
    }
}