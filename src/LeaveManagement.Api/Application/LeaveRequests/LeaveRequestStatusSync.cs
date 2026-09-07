using StarterKit.Approval.Contracts.Approvals;
using StarterKit.Approval.Contracts.Services;
using StarterKit.LeaveManagement.Api.Data;
using StarterKit.LeaveManagement.Api.Domain.LeaveRequests;

namespace StarterKit.LeaveManagement.Api.Application.LeaveRequests;

/// <summary>
/// Approval's own finalize/pending events are internal to Approval.Api and not exposed via
/// Approval.Contracts, so LeaveManagement cannot subscribe to them. Instead, any locally
/// <c>Pending</c> row is reconciled against Approval on read (and the resolved status persisted)
/// so a decision made through Approval's own UI is reflected here without a shared event bus.
/// Terminal statuses (Approved/Rejected/Cancelled) never call back into Approval again.
/// </summary>
internal static class LeaveRequestStatusSync
{
    public static async Task ReconcileAsync(
        LeaveManagementDbContext context,
        IApprovalService approvalService,
        IReadOnlyCollection<LeaveRequest> entities,
        CancellationToken cancellationToken)
    {
        var pending = entities
            .Where(x => x.Status == LeaveRequestStatus.Pending && x.ApprovalRequestId is not null)
            .ToList();

        if (pending.Count == 0)
            return;

        var changed = false;

        foreach (var entity in pending)
        {
            var approval = await approvalService.GetByRequestAsync(
                "LeaveRequest", entity.Id, cancellationToken);

            var resolvedStatus = approval?.Status switch
            {
                ApprovalStatus.Approved => LeaveRequestStatus.Approved,
                ApprovalStatus.Rejected => LeaveRequestStatus.Rejected,
                ApprovalStatus.Cancelled => LeaveRequestStatus.Cancelled,
                _ => LeaveRequestStatus.Pending,
            };

            if (resolvedStatus != entity.Status)
            {
                entity.Status = resolvedStatus;
                changed = true;
            }
        }

        if (changed)
            await context.SaveChangesAsync(cancellationToken);
    }
}
