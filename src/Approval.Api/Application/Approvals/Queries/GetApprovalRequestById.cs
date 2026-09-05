using Mapster;
using StarterKit.Approval.Api.Data;

namespace StarterKit.Approval.Api.Application.Approvals.Queries;

internal sealed record GetApprovalRequestByIdQuery(string Id) : IQuery<IResult<ApprovalRequestDto>>;

internal class GetApprovalRequestByIdQueryHandler(ApprovalDbContext context)
    : IQueryHandler<GetApprovalRequestByIdQuery, IResult<ApprovalRequestDto>>
{
    public async Task<IResult<ApprovalRequestDto>> Handle(
        GetApprovalRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var dto = await context.ApprovalRequests
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .ProjectToType<ApprovalRequestDto>()
            .SingleOrDefaultAsync(cancellationToken);

        if (dto is null)
            return Result<ApprovalRequestDto>.NotFound($"Approval request {request.Id} not found");

        return Result<ApprovalRequestDto>.Success(dto);
    }
}
