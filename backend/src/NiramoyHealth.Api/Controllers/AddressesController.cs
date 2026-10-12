using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;

namespace NiramoyHealth.Api.Controllers;

public class AddressesController : BaseApiController
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetMyAddresses(CancellationToken ct)
    {
        var addresses = await _addressService.GetUserAddressesAsync(RequiredUserId, ct);
        return OkResponse(addresses);
    }

    [Authorize]
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetAddressById(long id, CancellationToken ct)
    {
        var address = await _addressService.GetAddressByIdAsync(RequiredUserId, id, ct);
        return OkResponse(address);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request, CancellationToken ct)
    {
        var address = await _addressService.CreateAddressAsync(RequiredUserId, request, ct);
        return OkResponse(address, "Address created successfully.");
    }

    [Authorize]
    [HttpPut("{id:long}")]
    public async Task<IActionResult> UpdateAddress(long id, [FromBody] UpdateAddressRequest request, CancellationToken ct)
    {
        var address = await _addressService.UpdateAddressAsync(RequiredUserId, id, request, ct);
        return OkResponse(address, "Address updated successfully.");
    }

    [Authorize]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> DeleteAddress(long id, CancellationToken ct)
    {
        await _addressService.DeleteAddressAsync(RequiredUserId, id, ct);
        return OkResponse(true, "Address deleted successfully.");
    }

    [HttpGet("divisions")]
    public async Task<IActionResult> GetDivisions(CancellationToken ct)
    {
        var divisions = await _addressService.GetDivisionsAsync(ct);
        return OkResponse(divisions);
    }

    [HttpGet("districts")]
    public async Task<IActionResult> GetDistricts([FromQuery] int? divisionId, CancellationToken ct)
    {
        var districts = await _addressService.GetDistrictsAsync(divisionId, ct);
        return OkResponse(districts);
    }

    [HttpGet("upazilas")]
    public async Task<IActionResult> GetUpazilas([FromQuery] int? districtId, CancellationToken ct)
    {
        var upazilas = await _addressService.GetUpazilasAsync(districtId, ct);
        return OkResponse(upazilas);
    }
}
