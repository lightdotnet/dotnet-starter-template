using StarterKit.Shared.Entities;

namespace StarterKit.Approval.Api.Events;

internal sealed record ApprovalStepPendingEvent(
    string ApprovalRequestId,
    string Title,
    string? DeepLinkUrl,
    string ApproverUserId,
    string RequesterUserId) : DomainEvent;
