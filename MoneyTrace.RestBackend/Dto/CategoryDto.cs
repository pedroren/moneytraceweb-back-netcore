using MoneyTrace.Application.Domain;

namespace MoneyTrace.RestBackend.Dto
{
  /// <summary>
  /// Dto for CategoryEntity.
  /// </summary>
  public record CategoryDto (int Id, string Name, CategoryType Type, bool IsEnabled, List<SubCategoryDto> SubCategories, int UserId);

/// <summary>
  /// Dto for SubCategoryEntity.
  /// </summary>
  public record SubCategoryDto (int Id, string Name, int CategoryId, bool IsEnabled);
  
  public static class CategoryDtoExtensions
  {
    public static CategoryDto ToDto(this CategoryEntity entity)
    {
      return new CategoryDto(entity.Id, entity.Name, entity.Type, entity.IsEnabled, entity.SubCategories.Select(x => x.ToDto()).ToList(), entity.UserId);
    }

    public static SubCategoryDto ToDto(this SubCategoryEntity entity)
    {
      return new SubCategoryDto(entity.Id, entity.Name, entity.CategoryId, entity.IsEnabled);
    }
  }

}