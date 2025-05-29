using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Operations;
using MoneyTrace.Application.Features.Templates;

namespace MoneyTrace.RestBackend.Dto;

public record TemplateDto(
    int Id,
    string Title,
    OperationType Type,
    int? VendorId,
    int AccountId,
    int? DestinationAccountId,
    decimal TotalAmount,
    CategoryType CategoryType,
    OperationCategoryModel[] Allocation,
    bool IsEnabled = true);

public static class TemplateDtoExtensions
{
    public static TemplateDto ToDto(this TemplateEntity entity) =>
        new(entity.Id, entity.Title, entity.Type, entity.VendorId, entity.AccountId,entity.DestinationAccountId, entity.TotalAmount, entity.CategoryType,
            entity.Allocation.Select(c => new OperationCategoryModel(c.CategoryId, c.SubCategoryId, c.Amount)).ToArray(),
            entity.IsEnabled);

    public static CreateTemplateCommand ToCreateCommand(this TemplateDto dto, int userId) =>
        new(userId, dto.Title, dto.Type, dto.VendorId, dto.AccountId, dto.DestinationAccountId, dto.TotalAmount, dto.CategoryType, dto.Allocation);

    public static UpdateTemplateCommand ToUpdateCommand(this TemplateDto dto, int userId) =>
        new(userId, dto.Id, dto.Title, dto.Type, dto.VendorId, dto.AccountId, dto.DestinationAccountId, dto.TotalAmount, dto.CategoryType, dto.Allocation,
            dto.IsEnabled);
    public static DeleteTemplateCommand ToDeleteCommand(this TemplateDto dto, int userId) =>
        new(userId, dto.Id);
}
