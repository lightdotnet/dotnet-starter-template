using Mapster;
using StarterKit.Approval.Api.Data;

namespace StarterKit.Approval.Api.Application.Approvals.Queries;

internal sealed record GetMyApprovalRequestByIdQuery(string Id, string UserId) : IQuery<IResult<ApprovalRequestDto>>;

internal class GetMyApprovalRequestByIdQueryHandler(ApprovalDbContext context)
    : IQueryHandler<GetMyApprovalRequestByIdQuery, IResult<ApprovalRequestDto>>
{
    public async Task<IResult<ApprovalRequestDto>> Handle(
        GetMyApprovalRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await context.ApprovalRequests
            .AsNoTracking()
            .Include(x => x.Steps)
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        // Not found and "not yours" resolve to the same NotFound response, so a user can't
        // probe for the existence of a request they have no relation to.
        var isRelated = entity is not null
            && (entity.RequesterUserId == request.UserId
                || entity.Steps.Any(s => s.ApproverUserId == request.UserId));

        if (!isRelated)
            return Result<ApprovalRequestDto>.NotFound($"Approval request {request.Id} not found");

        return Result<ApprovalRequestDto>.Success(entity!.Adapt<ApprovalRequestDto>());
    }
}
