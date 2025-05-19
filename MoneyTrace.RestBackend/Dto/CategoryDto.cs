using FluentValidation;
using MediatR;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Categories;

namespace MoneyTrace.RestBackend.Dto;

/// <summary>
/// Dto for CategoryEntity.
/// </summary>
public record CategoryDto(int Id, string Name, string Type, bool IsEnabled, List<SubCategoryDto> SubCategories, int UserId);

/// <summary>
/// Dto for SubCategoryEntity.
/// </summary>
public record SubCategoryDto(int Id, string Name, bool IsEnabled);


public sealed class CategoryDtoValidator : AbstractValidator<CategoryDto>
{
    public CategoryDtoValidator()
    {
        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage("Type is required.")
            .Must(type => Enum.IsDefined(typeof(CategoryType), type))
            .WithMessage("Type is not valid.");
    }
}

public static class CategoryDtoExtensions
{
    public static CategoryDto ToDto(this CategoryEntity entity)
    {
        return new CategoryDto(entity.Id, entity.Name, entity.Type.ToString(), entity.IsEnabled, entity.SubCategories.Select(x => x.ToDto()).ToList(), entity.UserId);
    }

    public static SubCategoryDto ToDto(this SubCategoryEntity entity)
    {
        return new SubCategoryDto(entity.Id, entity.Name, entity.IsEnabled);
    }

    public static CreateCategoryCommand ToCreateCommand(this CategoryDto dto, int userId)
    {
        new CategoryDtoValidator().ValidateAndThrow(dto);
        return new CreateCategoryCommand(userId, dto.Name, Enum.Parse<CategoryType>(dto.Type), dto.SubCategories.Select(x => x.Name).ToArray());
    }

    public static UpdateSubCategoryCommand ToUpdateSubCommand(this SubCategoryDto dto)
    {
        return new UpdateSubCategoryCommand(dto.Id, dto.Name, dto.IsEnabled);
    }
    public static UpdateCategoryCommand ToUpdateCommand(this CategoryDto dto, int userId)
    {
        new CategoryDtoValidator().ValidateAndThrow(dto);
        return new UpdateCategoryCommand(userId, dto.Id, dto.Name, Enum.Parse<CategoryType>(dto.Type), dto.IsEnabled,
            dto.SubCategories.Select(x => x.ToUpdateSubCommand()).ToArray());
    }
}

