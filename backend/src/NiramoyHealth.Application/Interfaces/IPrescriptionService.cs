using NiramoyHealth.Application.Common;
using NiramoyHealth.Application.DTOs;

namespace NiramoyHealth.Application.Interfaces;

public record FileUploadPayload(
    string FileName,
    string ContentType,
    long Length,
    Stream Stream
);

public interface IPrescriptionService
{
    Task<PrescriptionDto> SubmitPrescriptionAsync(long userId, SubmitPrescriptionRequest request, IReadOnlyList<FileUploadPayload> files, CancellationToken ct = default);
    Task<PagedResult<PrescriptionDto>> GetUserPrescriptionsAsync(long userId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<PrescriptionDto> GetPrescriptionByIdAsync(long userId, long prescriptionId, bool isStaff = false, CancellationToken ct = default);
    Task<PagedResult<PrescriptionDto>> GetPendingPrescriptionsQueueAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<PrescriptionDto> ReviewPrescriptionAsync(long pharmacistId, long prescriptionId, ReviewPrescriptionRequest request, CancellationToken ct = default);
}
