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
    CategoryType? CategoryType,
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

public record GetOperationByCriteriaQueryDto(
    DateTime StartDate,
    DateTime EndDate,
    int? AccountId,
    int? CategoryId,
    int? VendorId,
    OperationType? Type,
    int PageIdx = 1,
    int PageSize = 10);

public static class OperationDtoExtensions
{
    public static OperationDto ToDto(this OperationEntity entity) =>
        new(entity.Id, entity.Date, entity.Title, entity.Type, entity.VendorId, entity.AccountId, entity.DestinationAccountId, entity.TotalAmount, entity.Comments,
            entity.CategoryType, entity.Allocation.Select(c => new OperationCategoryModel(c.CategoryId, c.SubCategoryId, c.Amount)).ToArray());

    public static CreateOperationCommand ToCreateCommand(this OperationDto dto, int userId) =>
        new(userId, dto.Date, dto.Title, dto.Type, dto.VendorId, dto.AccountId, dto.DestinationAccountId, dto.TotalAmount, dto.Comments,
        dto.CategoryType, dto.Allocation);

    public static UpdateOperationCommand ToUpdateCommand(this OperationDto dto, int userId) =>
        new(userId, dto.Id, dto.Date, dto.Title, dto.Type, dto.VendorId, dto.AccountId, dto.DestinationAccountId, dto.TotalAmount, dto.Comments,
        dto.CategoryType, dto.Allocation);

    public static DeleteOperationCommand ToDeleteCommand(this OperationDto dto, int userId) =>
        new(userId, dto.Id);

    public static GetOperationByCriteriaQuery ToGetOperationByCriteriaCommand(this GetOperationByCriteriaQueryDto dto, int userId) =>
        new(userId, dto.StartDate, dto.EndDate, dto.AccountId, dto.CategoryId, dto.VendorId, dto.Type, dto.PageIdx, dto.PageSize);

    public static OperationDto ToDto(this CreateOperationCommand command)
    {
        return new OperationDto(
            0, // Id will be set by the database
            command.Date,
            command.Title.Trim(),
            command.Type,
            command.VendorId,
            command.AccountId,
            command.DestinationAccountId,
            command.TotalAmount,
            command.Comments,
            command.CategoryType,
            command.Allocation);
    }
}

