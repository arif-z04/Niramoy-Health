using Microsoft.EntityFrameworkCore;
using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;
using NiramoyHealth.Application.Interfaces;
using NiramoyHealth.Domain.Enums;
using NiramoyHealth.Domain.Entities;
using NiramoyHealth.Domain.Exceptions;

namespace NiramoyHealth.Application.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly INiramoyDbContext _db;
    private readonly IFileStorageService _storageService;

    public PrescriptionService(INiramoyDbContext db, IFileStorageService storageService)
    {
        _db = db;
        _storageService = storageService;
    }

    public async Task<PrescriptionDto> SubmitPrescriptionAsync(
        long userId,
        SubmitPrescriptionRequest request,
        IReadOnlyList<FileUploadPayload> files,
        CancellationToken ct = default)
    {
        if (files == null || files.Count == 0)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Files", new[] { "At least one prescription file or image must be uploaded." } }
            });
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new NotFoundException(nameof(User), userId);

        var prescription = new Prescription
        {
            PublicId = Guid.NewGuid(),
            UserId = userId,
            PatientName = request.PatientName.Trim(),
            PatientAge = request.PatientAge,
            PatientGender = request.PatientGender?.Trim().ToLowerInvariant(),
            DoctorName = request.DoctorName?.Trim(),
            HospitalClinic = request.HospitalClinic?.Trim(),
            PrescriptionDate = request.PrescriptionDate,
            Status = PrescriptionStatuses.Submitted, // ALWAYS default to submitted, never auto-approved
            CustomerNotes = request.CustomerNotes?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync(ct);

        foreach (var file in files)
        {
            if (file.Length > 10 * 1024 * 1024)
            {
                throw new ValidationException(new Dictionary<string, string[]>
                {
                    { "File", new[] { $"File '{file.FileName}' exceeds the maximum allowed size of 10 MB." } }
                });
            }

            var storageKey = await _storageService.SaveFileAsync(file.Stream, file.FileName, "prescriptions", ct);

            var prescriptionFile = new PrescriptionFile
            {
                PrescriptionId = prescription.Id,
                StorageProvider = "local",
                FileStorageKey = storageKey,
                OriginalFilename = file.FileName,
                FileSizeBytes = file.Length,
                MimeType = file.ContentType,
                IsSecureStorage = true,
                UploadedAt = DateTimeOffset.UtcNow
            };

            _db.PrescriptionFiles.Add(prescriptionFile);
            prescription.Files.Add(prescriptionFile);
        }

        await _db.SaveChangesAsync(ct);

        prescription.User = user;
        return MapToPrescriptionDto(prescription);
    }

    public async Task<PagedResult<PrescriptionDto>> GetUserPrescriptionsAsync(long userId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 50);

        var query = _db.Prescriptions
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Reviewer)
            .Include(p => p.Files)
            .Include(p => p.ReviewRecords)
            .ThenInclude(r => r.Pharmacist)
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(MapToPrescriptionDto).ToList();
        return new PagedResult<PrescriptionDto>(dtos, total, page, size);
    }

    public async Task<PrescriptionDto> GetPrescriptionByIdAsync(long userId, long prescriptionId, bool isStaff = false, CancellationToken ct = default)
    {
        var query = _db.Prescriptions
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Reviewer)
            .Include(p => p.Files)
            .Include(p => p.ReviewRecords)
            .ThenInclude(r => r.Pharmacist)
            .Where(p => p.Id == prescriptionId);

        if (!isStaff)
        {
            query = query.Where(p => p.UserId == userId);
        }

        var prescription = await query.FirstOrDefaultAsync(ct);
        if (prescription == null)
        {
            throw new NotFoundException(nameof(Prescription), prescriptionId);
        }

        return MapToPrescriptionDto(prescription);
    }

    public async Task<PagedResult<PrescriptionDto>> GetPendingPrescriptionsQueueAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var page = Math.Max(1, pageNumber);
        var size = Math.Clamp(pageSize, 1, 50);

        var query = _db.Prescriptions
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Files)
            .Include(p => p.ReviewRecords)
            .Where(p => p.Status == PrescriptionStatuses.Submitted || p.Status == PrescriptionStatuses.UnderReview)
            .OrderBy(p => p.CreatedAt);

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * size).Take(size).ToListAsync(ct);

        var dtos = items.Select(MapToPrescriptionDto).ToList();
        return new PagedResult<PrescriptionDto>(dtos, total, page, size);
    }

    public async Task<PrescriptionDto> ReviewPrescriptionAsync(
        long pharmacistId,
        long prescriptionId,
        ReviewPrescriptionRequest request,
        CancellationToken ct = default)
    {
        var prescription = await _db.Prescriptions
            .Include(p => p.User)
            .Include(p => p.Files)
            .Include(p => p.ReviewRecords)
            .FirstOrDefaultAsync(p => p.Id == prescriptionId, ct);

        if (prescription == null)
        {
            throw new NotFoundException(nameof(Prescription), prescriptionId);
        }

        var normalizedNewStatus = request.NewStatus.Trim().ToLowerInvariant();
        if (normalizedNewStatus != PrescriptionStatuses.Approved &&
            normalizedNewStatus != PrescriptionStatuses.Rejected &&
            normalizedNewStatus != PrescriptionStatuses.PartiallyApproved)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "NewStatus", new[] { "Allowed statuses: approved, rejected, partially_approved." } }
            });
        }

        if (normalizedNewStatus == PrescriptionStatuses.Rejected && string.IsNullOrWhiteSpace(request.RejectionReason))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "RejectionReason", new[] { "Rejection reason is mandatory when rejecting a prescription." } }
            });
        }

        var previousStatus = prescription.Status;
        prescription.Status = normalizedNewStatus;
        prescription.RejectionReason = normalizedNewStatus == PrescriptionStatuses.Rejected ? request.RejectionReason : null;
        prescription.ReviewedBy = pharmacistId;
        prescription.ReviewedAt = DateTimeOffset.UtcNow;
        prescription.UpdatedAt = DateTimeOffset.UtcNow;

        var reviewRecord = new PrescriptionReviewRecord
        {
            PrescriptionId = prescription.Id,
            PharmacistId = pharmacistId,
            PreviousStatus = previousStatus,
            NewStatus = normalizedNewStatus,
            DecisionNotes = request.EffectiveDecisionNotes,
            VerifiedMedicinesJson = string.IsNullOrWhiteSpace(request.VerifiedMedicinesJson) ? "[]" : request.VerifiedMedicinesJson,
            ReviewedAt = DateTimeOffset.UtcNow
        };

        _db.PrescriptionReviewRecords.Add(reviewRecord);
        await _db.SaveChangesAsync(ct);

        var reviewer = await _db.Users.FirstOrDefaultAsync(u => u.Id == pharmacistId, ct);
        prescription.Reviewer = reviewer;

        return MapToPrescriptionDto(prescription);
    }

    private static PrescriptionDto MapToPrescriptionDto(Prescription p)
    {
        var fileDtos = p.Files.Select(f => new PrescriptionFileDto(
            Id: f.Id,
            StorageProvider: f.StorageProvider,
            FileStorageKey: f.FileStorageKey,
            OriginalFilename: f.OriginalFilename,
            FileSizeBytes: f.FileSizeBytes,
            MimeType: f.MimeType,
            DownloadUrl: $"/api/v1/prescriptions/{p.Id}/files/{f.Id}",
            UploadedAt: f.UploadedAt
        )).ToList();

        var reviewDtos = p.ReviewRecords.Select(r => new PrescriptionReviewRecordDto(
            Id: r.Id,
            PharmacistId: r.PharmacistId,
            PharmacistName: r.Pharmacist?.FullName ?? "Pharmacist",
            PreviousStatus: r.PreviousStatus,
            NewStatus: r.NewStatus,
            DecisionNotes: r.DecisionNotes,
            VerifiedMedicinesJson: r.VerifiedMedicinesJson,
            ReviewedAt: r.ReviewedAt
        )).ToList();

        return new PrescriptionDto(
            Id: p.Id,
            PublicId: p.PublicId,
            UserId: p.UserId,
            CustomerName: p.User?.FullName,
            CustomerPhone: p.User?.PhoneNumber,
            PatientName: p.PatientName,
            PatientAge: p.PatientAge,
            PatientGender: p.PatientGender,
            DoctorName: p.DoctorName,
            HospitalClinic: p.HospitalClinic,
            PrescriptionDate: p.PrescriptionDate,
            Status: p.Status,
            RejectionReason: p.RejectionReason,
            CustomerNotes: p.CustomerNotes,
            ReviewedBy: p.ReviewedBy,
            ReviewerName: p.Reviewer?.FullName,
            ReviewedAt: p.ReviewedAt,
            CreatedAt: p.CreatedAt,
            Files: fileDtos,
            ReviewRecords: reviewDtos
        );
    }
}
