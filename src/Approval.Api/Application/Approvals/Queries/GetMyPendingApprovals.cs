using Mapster;
using StarterKit.Approval.Api.Data;
using StarterKit.Persistence.Extensions;
using StarterKit.Shared;

namespace StarterKit.Approval.Api.Application.Approvals.Queries;

internal sealed record GetMyPendingApprovalsQuery(string ApproverUserId, PageQuery Page)
    : IQuery<PagedResult<ApprovalRequestDto>>;

internal class GetMyPendingApprovalsQueryHandler(ApprovalDbContext context)
    : IQueryHandler<GetMyPendingApprovalsQuery, PagedResult<ApprovalRequestDto>>
{
    public Task<PagedResult<ApprovalRequestDto>> Handle(
        GetMyPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        return context.ApprovalRequests
            .AsNoTracking()
            .Where(x => x.Status == ApprovalStatus.Pending
                && x.Steps.Any(s => s.Level == x.CurrentLevel
                    && s.ApproverUserId == request.ApproverUserId
                    && s.Status == ApprovalStepStatus.Pending))
            .OrderBy(x => x.Created)
            .ProjectToType<ApprovalRequestDto>()
            .ToPagedResultAsync(request.Page);
    }
}
