namespace NiramoyHealth.Application.DTOs;

public record PrescriptionFileDto(
    long Id,
    string StorageProvider,
    string FileStorageKey,
    string OriginalFilename,
    long FileSizeBytes,
    string MimeType,
    string DownloadUrl,
    DateTimeOffset UploadedAt
);

public record PrescriptionReviewRecordDto(
    long Id,
    long PharmacistId,
    string PharmacistName,
    string PreviousStatus,
    string NewStatus,
    string DecisionNotes,
    string VerifiedMedicinesJson,
    DateTimeOffset ReviewedAt
);

public record PrescriptionDto(
    long Id,
    Guid PublicId,
    long UserId,
    string? CustomerName,
    string? CustomerPhone,
    string PatientName,
    int? PatientAge,
    string? PatientGender,
    string? DoctorName,
    string? HospitalClinic,
    DateOnly? PrescriptionDate,
    string Status,
    string? RejectionReason,
    string? CustomerNotes,
    long? ReviewedBy,
    string? ReviewerName,
    DateTimeOffset? ReviewedAt,
    DateTimeOffset CreatedAt,
    IReadOnlyList<PrescriptionFileDto> Files,
    IReadOnlyList<PrescriptionReviewRecordDto> ReviewRecords
);

public record SubmitPrescriptionRequest(
    string PatientName,
    int? PatientAge,
    string? PatientGender,
    string? DoctorName,
    string? HospitalClinic,
    DateOnly? PrescriptionDate,
    string? CustomerNotes
);

public record ReviewPrescriptionRequest(
    string NewStatus, // "approved", "rejected", "partially_approved"
    string? DecisionNotes = null,
    string? RejectionReason = null,
    string? VerifiedMedicinesJson = null,
    string? ClinicalNotes = null
)
{
    public string EffectiveDecisionNotes => !string.IsNullOrWhiteSpace(DecisionNotes) ? DecisionNotes : (ClinicalNotes ?? "Prescription verified by pharmacist.");
};
