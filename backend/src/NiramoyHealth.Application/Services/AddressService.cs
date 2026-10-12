using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class AddressService : IAddressService
{
    private readonly INiramoyDbContext _db;

    public AddressService(INiramoyDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<AddressDto>> GetUserAddressesAsync(long userId, CancellationToken ct = default)
    {
        var addresses = await _db.CustomerAddresses
            .Include(a => a.Division)
            .Include(a => a.District)
            .Include(a => a.Upazila)
            .Where(a => a.UserId == userId && a.IsActive)
            .OrderByDescending(a => a.IsDefault)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return addresses.Select(MapToAddressDto).ToList();
    }

    public async Task<AddressDto> GetAddressByIdAsync(long userId, long addressId, CancellationToken ct = default)
    {
        var address = await _db.CustomerAddresses
            .Include(a => a.Division)
            .Include(a => a.District)
            .Include(a => a.Upazila)
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive, ct);

        if (address == null)
        {
            throw new NotFoundException(nameof(CustomerAddress), addressId);
        }

        return MapToAddressDto(address);
    }

    public async Task<AddressDto> CreateAddressAsync(long userId, CreateAddressRequest request, CancellationToken ct = default)
    {
        if (request.IsDefault)
        {
            var existingDefaults = await _db.CustomerAddresses
                .Where(a => a.UserId == userId && a.IsDefault)
                .ToListAsync(ct);
            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
            }
        }

        var division = await _db.Divisions.FirstOrDefaultAsync(d => d.Id == request.DivisionId, ct)
            ?? throw new NotFoundException(nameof(Division), request.DivisionId);

        var district = await _db.Districts.FirstOrDefaultAsync(d => d.Id == request.DistrictId && d.DivisionId == request.DivisionId, ct)
            ?? throw new NotFoundException(nameof(District), request.DistrictId);

        Upazila? upazila = null;
        if (request.UpazilaId.HasValue)
        {
            upazila = await _db.Upazilas.FirstOrDefaultAsync(u => u.Id == request.UpazilaId.Value && u.DistrictId == request.DistrictId, ct)
                ?? throw new NotFoundException(nameof(Upazila), request.UpazilaId.Value);
        }

        var newAddress = new CustomerAddress
        {
            UserId = userId,
            AddressLabel = request.AddressLabel,
            RecipientName = request.RecipientName,
            RecipientPhone = request.RecipientPhone,
            DivisionId = request.DivisionId,
            DistrictId = request.DistrictId,
            UpazilaId = request.UpazilaId,
            AreaDetails = request.AreaDetails,
            PostalCode = request.PostalCode,
            DeliveryInstructions = request.DeliveryInstructions,
            IsDefault = request.IsDefault,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.CustomerAddresses.Add(newAddress);
        await _db.SaveChangesAsync(ct);

        newAddress.Division = division;
        newAddress.District = district;
        newAddress.Upazila = upazila;

        return MapToAddressDto(newAddress);
    }

    public async Task<AddressDto> UpdateAddressAsync(long userId, long addressId, UpdateAddressRequest request, CancellationToken ct = default)
    {
        var address = await _db.CustomerAddresses
            .Include(a => a.Division)
            .Include(a => a.District)
            .Include(a => a.Upazila)
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive, ct);

        if (address == null)
        {
            throw new NotFoundException(nameof(CustomerAddress), addressId);
        }

        if (request.IsDefault && !address.IsDefault)
        {
            var otherDefaults = await _db.CustomerAddresses
                .Where(a => a.UserId == userId && a.IsDefault && a.Id != addressId)
                .ToListAsync(ct);
            foreach (var addr in otherDefaults)
            {
                addr.IsDefault = false;
            }
        }

        var division = await _db.Divisions.FirstOrDefaultAsync(d => d.Id == request.DivisionId, ct)
            ?? throw new NotFoundException(nameof(Division), request.DivisionId);

        var district = await _db.Districts.FirstOrDefaultAsync(d => d.Id == request.DistrictId && d.DivisionId == request.DivisionId, ct)
            ?? throw new NotFoundException(nameof(District), request.DistrictId);

        Upazila? upazila = null;
        if (request.UpazilaId.HasValue)
        {
            upazila = await _db.Upazilas.FirstOrDefaultAsync(u => u.Id == request.UpazilaId.Value && u.DistrictId == request.DistrictId, ct)
                ?? throw new NotFoundException(nameof(Upazila), request.UpazilaId.Value);
        }

        address.AddressLabel = request.AddressLabel;
        address.RecipientName = request.RecipientName;
        address.RecipientPhone = request.RecipientPhone;
        address.DivisionId = request.DivisionId;
        address.DistrictId = request.DistrictId;
        address.UpazilaId = request.UpazilaId;
        address.AreaDetails = request.AreaDetails;
        address.PostalCode = request.PostalCode;
        address.DeliveryInstructions = request.DeliveryInstructions;
        address.IsDefault = request.IsDefault;
        address.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync(ct);

        address.Division = division;
        address.District = district;
        address.Upazila = upazila;

        return MapToAddressDto(address);
    }

    public async Task DeleteAddressAsync(long userId, long addressId, CancellationToken ct = default)
    {
        var address = await _db.CustomerAddresses
            .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId && a.IsActive, ct);

        if (address == null)
        {
            throw new NotFoundException(nameof(CustomerAddress), addressId);
        }

        address.IsActive = false;
        address.UpdatedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<DivisionDto>> GetDivisionsAsync(CancellationToken ct = default)
    {
        var divisions = await _db.Divisions
            .OrderBy(d => d.Name)
            .ToListAsync(ct);

        return divisions.Select(d => new DivisionDto(d.Id, d.Code, d.Name, d.NameBn)).ToList();
    }

    public async Task<IReadOnlyList<DistrictDto>> GetDistrictsAsync(int? divisionId, CancellationToken ct = default)
    {
        var query = _db.Districts.AsQueryable();
        if (divisionId.HasValue)
        {
            query = query.Where(d => d.DivisionId == divisionId.Value);
        }

        var districts = await query.OrderBy(d => d.Name).ToListAsync(ct);
        return districts.Select(d => new DistrictDto(d.Id, d.DivisionId, d.Code, d.Name, d.NameBn)).ToList();
    }

    public async Task<IReadOnlyList<UpazilaDto>> GetUpazilasAsync(int? districtId, CancellationToken ct = default)
    {
        var query = _db.Upazilas.AsQueryable();
        if (districtId.HasValue)
        {
            query = query.Where(u => u.DistrictId == districtId.Value);
        }

        var upazilas = await query.OrderBy(u => u.Name).ToListAsync(ct);
        return upazilas.Select(u => new UpazilaDto(u.Id, u.DistrictId, u.Name, u.NameBn, u.PostCode)).ToList();
    }

    private static AddressDto MapToAddressDto(CustomerAddress a) =>
        new(
            Id: a.Id,
            UserId: a.UserId,
            AddressLabel: a.AddressLabel,
            RecipientName: a.RecipientName,
            RecipientPhone: a.RecipientPhone,
            DivisionId: a.DivisionId,
            DivisionName: a.Division?.Name ?? "",
            DivisionNameBn: a.Division?.NameBn ?? "",
            DistrictId: a.DistrictId,
            DistrictName: a.District?.Name ?? "",
            DistrictNameBn: a.District?.NameBn ?? "",
            UpazilaId: a.UpazilaId,
            UpazilaName: a.Upazila?.Name,
            UpazilaNameBn: a.Upazila?.NameBn,
            AreaDetails: a.AreaDetails,
            PostalCode: a.PostalCode,
            DeliveryInstructions: a.DeliveryInstructions,
            IsDefault: a.IsDefault,
            IsActive: a.IsActive
        );
}
