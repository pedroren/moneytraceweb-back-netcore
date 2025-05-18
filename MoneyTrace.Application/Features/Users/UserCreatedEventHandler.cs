namespace MoneyTrace.Application.Common;

using System;
using MediatR;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Accounts;
using MoneyTrace.Application.Features.Categories;

internal sealed class UserCreatedEventHandler(IMediator mediator) : INotificationHandler<DomainEventNotification<UserCreatedEvent>>
{
    public async Task Handle(DomainEventNotification<UserCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        Console.WriteLine($"User created: {domainEvent.Item.Name}");

        // Create default accounts and categories for new user

        //New accounts
        await CreateUserDefAccounts(domainEvent, cancellationToken);
        await CreateUserDefCategories(domainEvent, cancellationToken);

        return;
    }

    private async Task CreateUserDefCategories(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var category1 = new CreateCategoryCommand(domainEvent.Item.Id, "Food", CategoryType.Expense, ["Groceries", "Restaurants"]);
        var task1 = mediator.Send(category1, cancellationToken);
        var category2 = new CreateCategoryCommand(domainEvent.Item.Id, "Entertainment", CategoryType.Expense, ["Movies", "Games"]);
        var task2 = mediator.Send(category2, cancellationToken);
        var category3 = new CreateCategoryCommand(domainEvent.Item.Id, "Salary", CategoryType.Income, ["Salary"]);
        var task3 = mediator.Send(category3, cancellationToken);
        var category4 = new CreateCategoryCommand(domainEvent.Item.Id, "Utilities", CategoryType.Expense, ["Electricity", "Water"]);
        var task4 = mediator.Send(category4, cancellationToken);
        var category5 = new CreateCategoryCommand(domainEvent.Item.Id, "Rent", CategoryType.Expense, ["Rent"]);
        var task5 = mediator.Send(category5, cancellationToken);
        var category6 = new CreateCategoryCommand(domainEvent.Item.Id, "Other", CategoryType.Expense, ["Other"]);
        var task6 = mediator.Send(category6, cancellationToken);
        var category7 = new CreateCategoryCommand(domainEvent.Item.Id, "Other Income", CategoryType.Income, ["Other"]);
        var task7 = mediator.Send(category7, cancellationToken);

        await Task.WhenAll(task1, task2, task3, task4, task5, task6, task7);
    }

    private async Task CreateUserDefAccounts(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var account1 = new CreateAccountCommand(domainEvent.Item.Id, "Cash", "Cash account", 0, AccountType.Debit);
        var task1 = mediator.Send(account1, cancellationToken);
        var account2 = new CreateAccountCommand(domainEvent.Item.Id, "Chequing", "Bank account", 0, AccountType.Debit);
        var task2 = mediator.Send(account2, cancellationToken);
        var account3 = new CreateAccountCommand(domainEvent.Item.Id, "Credit Card", "Credit card account", 0, AccountType.Credit);
        var task3 = mediator.Send(account3, cancellationToken);
        var account4 = new CreateAccountCommand(domainEvent.Item.Id, "Savings", "Savings account", 0, AccountType.Debit);
        var task4 = mediator.Send(account4, cancellationToken);
        await Task.WhenAll(task1, task2, task3, task4);
    }
}