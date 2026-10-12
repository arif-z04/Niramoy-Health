using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Api.Controllers;

[Authorize]
public class PrescriptionsController : BaseApiController
{
    private readonly IPrescriptionService _prescriptionService;
    private readonly IFileStorageService _storageService;

    public PrescriptionsController(IPrescriptionService prescriptionService, IFileStorageService storageService)
    {
        _prescriptionService = prescriptionService;
        _storageService = storageService;
    }

    [HttpPost]
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitPrescription(
        [FromForm] SubmitPrescriptionRequest request,
        [FromForm] List<IFormFile>? files,
        [FromForm] IFormFile? file,
        CancellationToken ct)
    {
        var allFiles = files != null ? new List<IFormFile>(files) : new List<IFormFile>();
        if (file != null && !allFiles.Contains(file))
        {
            allFiles.Add(file);
        }

        if (allFiles.Count == 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Files", new[] { "At least one prescription file or image is required." } }
            });
        }

        var payloads = new List<FileUploadPayload>();
        foreach (var f in allFiles)
        {
            payloads.Add(new FileUploadPayload(
                FileName: f.FileName,
                ContentType: f.ContentType,
                Length: f.Length,
                Stream: f.OpenReadStream()
            ));
        }

        var result = await _prescriptionService.SubmitPrescriptionAsync(RequiredUserId, request, payloads, ct);
        return OkResponse(result, "Prescription submitted for verification. Initial status: submitted.");
    }

    [HttpGet]
    public async Task<IActionResult> GetMyPrescriptions(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var activePage = page > 1 ? page : pageNumber;
        var result = await _prescriptionService.GetUserPrescriptionsAsync(RequiredUserId, activePage, pageSize, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPrescriptionById(long id, CancellationToken ct)
    {
        var result = await _prescriptionService.GetPrescriptionByIdAsync(RequiredUserId, id, IsStaffUser, ct);
        return OkResponse(result);
    }

    [HttpGet("{id:long}/files/{fileId:long}")]
    public async Task<IActionResult> DownloadFile(long id, long fileId, CancellationToken ct)
    {
        var prescription = await _prescriptionService.GetPrescriptionByIdAsync(RequiredUserId, id, IsStaffUser, ct);
        var fileInfo = prescription.Files.FirstOrDefault(f => f.Id == fileId);
        if (fileInfo == null)
        {
            throw new NotFoundException("PrescriptionFile", fileId);
        }

        var stream = await _storageService.GetFileAsync(fileInfo.FileStorageKey, ct);
        if (stream == null)
        {
            throw new NotFoundException("File", fileInfo.FileStorageKey);
        }

        return File(stream, fileInfo.MimeType, fileInfo.OriginalFilename);
    }

    [Authorize(Roles = $"{RoleNames.Pharmacist},{RoleNames.Admin},{RoleNames.SuperAdmin}")]
    [HttpGet("queue")]
    public async Task<IActionResult> GetPendingQueue(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var activePage = page > 1 ? page : pageNumber;
        var result = await _prescriptionService.GetPendingPrescriptionsQueueAsync(activePage, pageSize, ct);
        return OkResponse(result);
    }

    [Authorize(Roles = $"{RoleNames.Pharmacist},{RoleNames.Admin},{RoleNames.SuperAdmin}")]
    [HttpPost("{id:long}/review")]
    public async Task<IActionResult> ReviewPrescription(
        long id,
        [FromBody] ReviewPrescriptionRequest request,
        CancellationToken ct)
    {
        var result = await _prescriptionService.ReviewPrescriptionAsync(RequiredUserId, id, request, ct);
        return OkResponse(result, $"Prescription status updated to {result.Status}.");
    }
}
