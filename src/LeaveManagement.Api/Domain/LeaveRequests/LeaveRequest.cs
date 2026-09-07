using StarterKit.Shared.Entities;

namespace StarterKit.LeaveManagement.Api.Domain.LeaveRequests;

public class LeaveRequest : AuditableEntity
{
    public string UserId { get; set; } = null!;

    public string EmployeeId { get; set; } = null!;

    public LeaveType LeaveType { get; set; }

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public string? Reason { get; set; }

    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    public string? ApprovalRequestId { get; set; }
}
