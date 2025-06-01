using MoneyTrace.Application.Domain;

namespace MoneyTrace.RestBackend.Dto;

/// <summary>
/// Dto for UserEntity.
/// </summary>
public record UserDto(int Id, string Name, string Email, string DateFormat, string TimeZone);

public static class UserEntityDtoExtensions
{
    public static UserDto ToDto(this UserEntity user)
    {
        return new UserDto(user.Id, user.Name, user.Email, user.DateFormat, user.TimeZone);
    }
    public static UserEntity ToEntity(this UserDto userDto)
    {
        return new UserEntity
        {
            Id = userDto.Id,
            Name = userDto.Name,
            Email = userDto.Email,
            DateFormat = userDto.DateFormat,
            TimeZone = userDto.TimeZone
        };
    }
    public static void UpdateFromDto(this UserEntity user, UserDto userDto)
    {
        user.Name = userDto.Name;
        user.Email = userDto.Email;
        user.DateFormat = userDto.DateFormat;
        user.TimeZone = userDto.TimeZone;
    }
}
