namespace MoneyTrace.Application.Common;

using System;
using ErrorOr;
using MediatR;
using Microsoft.Extensions.Logging;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Accounts;
using MoneyTrace.Application.Features.Categories;
using MoneyTrace.Application.Features.Vendors;

internal sealed class UserCreatedEventHandler(IMediator mediator, ILogger<UserCreatedEventHandler> logger) : INotificationHandler<DomainEventNotification<UserCreatedEvent>>
{
    public async Task Handle(DomainEventNotification<UserCreatedEvent> notification, CancellationToken cancellationToken)
    {
        var domainEvent = notification.DomainEvent;

        Console.WriteLine($"User created: {domainEvent.Item.Name}");

        // Create default accounts and categories for new user
        var createAccTask = await CreateUserDefAccounts(domainEvent, cancellationToken);
        var createCatTask = await CreateUserDefCategories(domainEvent, cancellationToken);
        var createVendTask = await CreateUserDefVendors(domainEvent, cancellationToken);

        // Check for errors in account creation
        if (createAccTask.Any(errorOrAccount => errorOrAccount.IsError))
        {
            var errors = createAccTask.Where(errorOrAccount => errorOrAccount.IsError)
                                       .Select(errorOrAccount => errorOrAccount.Errors)
                                       .SelectMany(errors => errors);
            logger.LogError($"Errors creating accounts: {string.Join(", ", errors.Select(e => e.Description))}");
            //return;
        }
        // Check for errors in category creation
        if (createCatTask.Any(errorOrCategory => errorOrCategory.IsError))
        {
            var errors = createCatTask.Where(errorOrCategory => errorOrCategory.IsError)
                                       .Select(errorOrCategory => errorOrCategory.Errors)
                                       .SelectMany(errors => errors);
            logger.LogError($"Errors creating categories: {string.Join(", ", errors.Select(e => e.Description))}");
            //return;
        }
        // Check for errors in vendor creation
        if (createVendTask.Any(errorOrVendor => errorOrVendor.IsError))
        {
            var errors = createVendTask.Where(errorOrVendor => errorOrVendor.IsError)
                                        .Select(errorOrVendor => errorOrVendor.Errors)
                                        .SelectMany(errors => errors);
            logger.LogError($"Errors creating vendors: {string.Join(", ", errors.Select(e => e.Description))}");
            //return;
        }
        
        return;
    }

    private async Task<ErrorOr<CategoryEntity>[]> CreateUserDefCategories(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var taskList = new List<Task<ErrorOr<CategoryEntity>>>();

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

        return await Task.WhenAll(taskList);
    }

    private async Task<ErrorOr<AccountEntity>[]> CreateUserDefAccounts(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var taskList = new List<Task<ErrorOr<AccountEntity>>>();

        var account1 = new CreateAccountCommand(domainEvent.Item.Id, "Cash", "Cash account", 0, AccountType.Debit);
        taskList.Add(mediator.Send(account1, cancellationToken));
        var account2 = new CreateAccountCommand(domainEvent.Item.Id, "Chequing", "Bank account", 0, AccountType.Debit);
        taskList.Add(mediator.Send(account2, cancellationToken));
        var account3 = new CreateAccountCommand(domainEvent.Item.Id, "Credit Card", "Credit card account", 0, AccountType.Credit);
        taskList.Add(mediator.Send(account3, cancellationToken));
        var account4 = new CreateAccountCommand(domainEvent.Item.Id, "Savings", "Savings account", 0, AccountType.Debit);
        taskList.Add(mediator.Send(account4, cancellationToken));

        return await Task.WhenAll(taskList);
    }

    private async Task<ErrorOr<VendorEntity>[]> CreateUserDefVendors(UserCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        var taskList = new List<Task<ErrorOr<VendorEntity>>>();

        var vendor0 = new CreateVendorCommand(domainEvent.Item.Id, "Other");
        taskList.Add(mediator.Send(vendor0, cancellationToken));
        var vendor1 = new CreateVendorCommand(domainEvent.Item.Id, "Amazon");
        taskList.Add(mediator.Send(vendor1, cancellationToken));
        var vendor2 = new CreateVendorCommand(domainEvent.Item.Id, "Walmart");
        taskList.Add(mediator.Send(vendor2, cancellationToken));
        var vendor3 = new CreateVendorCommand(domainEvent.Item.Id, "Costco");
        taskList.Add(mediator.Send(vendor3, cancellationToken));

        return await Task.WhenAll(taskList);
    }
}