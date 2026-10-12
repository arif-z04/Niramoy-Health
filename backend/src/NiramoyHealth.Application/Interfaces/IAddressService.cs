using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public interface IAddressService
{
    Task<IReadOnlyList<AddressDto>> GetUserAddressesAsync(long userId, CancellationToken ct = default);
    Task<AddressDto> GetAddressByIdAsync(long userId, long addressId, CancellationToken ct = default);
    Task<AddressDto> CreateAddressAsync(long userId, CreateAddressRequest request, CancellationToken ct = default);
    Task<AddressDto> UpdateAddressAsync(long userId, long addressId, UpdateAddressRequest request, CancellationToken ct = default);
    Task DeleteAddressAsync(long userId, long addressId, CancellationToken ct = default);
    Task<IReadOnlyList<DivisionDto>> GetDivisionsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(int? divisionId, CancellationToken ct = default);
    Task<IReadOnlyList<UpazilaDto>> GetUpazilasAsync(int? districtId, CancellationToken ct = default);
}
