using FluentValidation;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;

namespace MoneyTrace.RestBackend.Dto;

public record OperationDto(
    int Id,
    DateTime Date,
    string Title,
    OperationType Type,
    int? VendorId,
    int AccountId,
    int? DestinationAccountId,
    decimal TotalAmount,
    string Comments,
    OperationCategoryModel[] Allocation);

public sealed class OperationDtoValidator : AbstractValidator<OperationDto>
{
    public OperationDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required.")
            .Must(type => Enum.IsDefined(typeof(OperationType), type))
            .WithMessage("Type is not valid.");
    }
}

public static class OperationDtoExtensions
{
    public static OperationDto ToDto(this OperationEntity entity) =>
        new(entity.Id, entity.Date, entity.Title, entity.Type, entity.VendorId, entity.AccountId, entity.DestinationAccountId, entity.TotalAmount, entity.Comments,
            entity.Allocation.Select(c => new OperationCategoryModel(c.CategoryId, c.SubCategoryId, c.Amount)).ToArray());

    public static CreateOperationCommand ToCreateCommand(this OperationDto dto, int userId) =>
        new(userId, dto.Date, dto.Title, dto.Type, dto.VendorId, dto.AccountId, dto.DestinationAccountId, dto.TotalAmount, dto.Comments, dto.Allocation);

    public static UpdateOperationCommand ToUpdateCommand(this OperationDto dto, int userId) =>
        new(userId, dto.Id, dto.Date, dto.Title, dto.Type, dto.VendorId, dto.AccountId, dto.DestinationAccountId, dto.TotalAmount, dto.Comments, dto.Allocation);

    public static DeleteOperationCommand ToDeleteCommand(this OperationDto dto, int userId) =>
        new(userId, dto.Id);
}

