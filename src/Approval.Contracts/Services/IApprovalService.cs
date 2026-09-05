using StarterKit.Approval.Contracts.Approvals;

namespace StarterKit.Approval.Contracts.Services;

/// <summary>
/// The seam other modules use to plug into the generic approval engine — the only way
/// Approval is meant to be consumed cross-module (mirrors <c>IUserService</c>/<c>INotificationService</c>).
/// </summary>
public interface IApprovalService
{
    /// <summary>
    /// Creates a new approval request with an already-resolved approver chain and notifies
    /// the first level's approver.
    /// </summary>
    Task<IResult<string>> CreateAsync(CreateApprovalRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a decision for the current level by <paramref name="decidedByUserId"/>. Advances
    /// to the next level (and notifies its approver) on approval, or finalizes the request as
    /// rejected. Notifies the original requester once the request reaches a final state.
    /// </summary>
    Task<IResult> DecideAsync(
        string approvalRequestId,
        string decidedByUserId,
        bool approved,
        string? comment,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a still-pending request — used when the source record (e.g. a leave request)
    /// is deleted before it's been decided.
    /// </summary>
    Task<IResult> CancelAsync(string approvalRequestId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Looks up the approval request tied to a given source record, for the owning module to
    /// check current status/level (e.g. to block edits once approved).
    /// </summary>
    Task<ApprovalRequestDto?> GetByRequestAsync(
        string requestType, string requestId, CancellationToken cancellationToken = default);
}
