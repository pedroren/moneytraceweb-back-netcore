using MoneyTrace.Application.Domain;
using MoneyTrace.Application.Features.Vendors;

namespace MoneyTrace.RestBackend.Dto;

public record VendorDto(
    int Id,
    string Name,
    bool IsEnabled
);

public static class VendorDtoExtensions
{
    public static VendorDto ToDto(this VendorEntity vendor)
    {
        return new VendorDto(
            vendor.Id,
            vendor.Name,
            vendor.IsEnabled
        );
    }

}