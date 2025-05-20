namespace MoneyTrace.Application.Common;

using System;
using MediatR;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Accounts;
using MoneyTrace.Application.Features.Categories;
using MoneyTrace.Application.Features.Vendors;

internal sealed class UserCreatedEventHandler(IMediator mediator) : INotificationHandler<DomainEventNotification<UserCreatedEvent>>
{
    public async Task Handle(DomainEventNotification<UserCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        Console.WriteLine($"User created: {domainEvent.Item.Name}");

        // Create default accounts and categories for new user
        await CreateUserDefAccounts(domainEvent, cancellationToken);
        await CreateUserDefCategories(domainEvent, cancellationToken);
        await CreateUserDefVendors(domainEvent, cancellationToken);

        return;
    }

    private async Task CreateUserDefCategories(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var taskList = new List<Task>();

        var category1 = new CreateCategoryCommand(domainEvent.Item.Id, "Food", CategoryType.Expense, ["Groceries", "Restaurants"]);
        taskList.Add(mediator.Send(category1, cancellationToken));
        var category2 = new CreateCategoryCommand(domainEvent.Item.Id, "Entertainment", CategoryType.Expense, ["Movies", "Games"]);
        taskList.Add(mediator.Send(category2, cancellationToken));
        var category3 = new CreateCategoryCommand(domainEvent.Item.Id, "Salary", CategoryType.Income, ["Salary"]);
        taskList.Add(mediator.Send(category3, cancellationToken));
        var category4 = new CreateCategoryCommand(domainEvent.Item.Id, "Utilities", CategoryType.Expense, ["Electricity", "Water"]);
        taskList.Add(mediator.Send(category4, cancellationToken));
        var category5 = new CreateCategoryCommand(domainEvent.Item.Id, "Rent", CategoryType.Expense, ["Rent"]);
        taskList.Add(mediator.Send(category5, cancellationToken));
        var category6 = new CreateCategoryCommand(domainEvent.Item.Id, "Other", CategoryType.Expense, ["Other"]);
        taskList.Add(mediator.Send(category6, cancellationToken));
        var category7 = new CreateCategoryCommand(domainEvent.Item.Id, "Other Income", CategoryType.Income, ["Other"]);
        taskList.Add(mediator.Send(category7, cancellationToken));

        await Task.WhenAll(taskList);
    }

    private async Task CreateUserDefAccounts(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var taskList = new List<Task>();

        var account1 = new CreateAccountCommand(domainEvent.Item.Id, "Cash", "Cash account", 0, AccountType.Debit);
        taskList.Add(mediator.Send(account1, cancellationToken));
        var account2 = new CreateAccountCommand(domainEvent.Item.Id, "Chequing", "Bank account", 0, AccountType.Debit);
        taskList.Add(mediator.Send(account2, cancellationToken));
        var account3 = new CreateAccountCommand(domainEvent.Item.Id, "Credit Card", "Credit card account", 0, AccountType.Credit);
        taskList.Add(mediator.Send(account3, cancellationToken));
        var account4 = new CreateAccountCommand(domainEvent.Item.Id, "Savings", "Savings account", 0, AccountType.Debit);
        taskList.Add(mediator.Send(account4, cancellationToken));

        await Task.WhenAll(taskList);
    }

    private async Task CreateUserDefVendors(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var taskList = new List<Task>();

        var vendor0 = new CreateVendorCommand(domainEvent.Item.Id, "Other");
        taskList.Add(mediator.Send(vendor0, cancellationToken));
        var vendor1 = new CreateVendorCommand(domainEvent.Item.Id, "Amazon");
        taskList.Add(mediator.Send(vendor1, cancellationToken));
        var vendor2 = new CreateVendorCommand(domainEvent.Item.Id, "Walmart");
        taskList.Add(mediator.Send(vendor2, cancellationToken));
        var vendor3 = new CreateVendorCommand(domainEvent.Item.Id, "Costco");
        taskList.Add(mediator.Send(vendor3, cancellationToken));


        await Task.WhenAll(taskList);
    }
}