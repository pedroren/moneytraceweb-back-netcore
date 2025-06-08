using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Vendors;

namespace MoneyTrace.BlazorApp.Dto;

public class VendorDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public VendorDto()
    {
    }
    public VendorDto(int id, string name, bool isEnabled)
    {
        Id = id;
        Name = name;
        IsEnabled = isEnabled;
    }
}

public static class VendorDtoExtensions
{
    public static VendorDto ToDto(this VendorEntity vendor)
    {
        return new VendorDto(vendor.Id,
            vendor.Name,
            vendor.IsEnabled);
    }

    public static CreateVendorCommand ToCreateCommand(this VendorDto vendorDto, int userId)
    {
        return new CreateVendorCommand(
            userId,
            vendorDto.Name
        );
    }
    public static UpdateVendorCommand ToUpdateCommand(this VendorDto vendorDto, int userId)
    {
        return new UpdateVendorCommand(
            userId,
            vendorDto.Id,
            vendorDto.Name,
            vendorDto.IsEnabled
        );
    }

}