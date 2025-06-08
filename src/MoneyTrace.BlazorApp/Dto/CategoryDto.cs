using FluentValidation;
using MediatR;
using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Categories;

namespace MoneyTrace.BlazorApp.Dto;

/// <summary>
/// Dto for CategoryEntity.
/// </summary>
public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public CategoryType Type { get; set; }
    public bool IsEnabled { get; set; }
    public List<SubCategoryDto> SubCategories { get; set; }
    public int UserId { get; set; }

    public CategoryDto() { }

    public CategoryDto(int id, string name, CategoryType type, bool isEnabled, List<SubCategoryDto> subCategories, int userId)
    {
        Id = id;
        Name = name;
        Type = type;
        IsEnabled = isEnabled;
        SubCategories = subCategories;
        UserId = userId;
    }
}

/// <summary>
/// Dto for SubCategoryEntity.
/// </summary>
public class SubCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }

    public SubCategoryDto() { }

    public SubCategoryDto(int id, string name, bool isEnabled)
    {
        Id = id;
        Name = name;
        IsEnabled = isEnabled;
    }
}


public static class CategoryDtoExtensions
{
    public static CategoryDto ToDto(this CategoryEntity entity)
    {
        return new CategoryDto(entity.Id, entity.Name, entity.Type, entity.IsEnabled, entity.SubCategories.Select(x => x.ToDto()).ToList(), entity.UserId);
    }

    public static SubCategoryDto ToDto(this SubCategoryEntity entity)
    {
        return new SubCategoryDto(entity.Id, entity.Name, entity.IsEnabled);
    }

    public static CreateCategoryCommand ToCreateCommand(this CategoryDto dto, int userId)
    {
        return new CreateCategoryCommand(userId, dto.Name, dto.Type, dto.SubCategories.Select(x => x.Name).ToArray());
    }
    

    public static UpdateSubCategoryCommand ToUpdateSubCommand(this SubCategoryDto dto)
    {
        return new UpdateSubCategoryCommand(dto.Id, dto.Name, dto.IsEnabled);
    }
    public static UpdateCategoryCommand ToUpdateCommand(this CategoryDto dto, int userId)
    {
        return new UpdateCategoryCommand(userId, dto.Id, dto.Name, dto.Type, dto.IsEnabled,
            dto.SubCategories.Select(x => x.ToUpdateSubCommand()).ToArray());
    }
}

