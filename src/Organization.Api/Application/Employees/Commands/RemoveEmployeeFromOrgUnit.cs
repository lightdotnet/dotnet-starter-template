using StarterKit.Organization.Api.Data;
using StarterKit.Shared;

namespace StarterKit.Organization.Api.Application.Employees.Commands;

internal sealed record RemoveEmployeeFromOrgUnitCommand(string EmployeeId, string OrgUnitId) : ICommand<IResult>;

internal class RemoveEmployeeFromOrgUnitCommandHandler(OrganizationDbContext context, IDateTime clock)
    : ICommandHandler<RemoveEmployeeFromOrgUnitCommand, IResult>
{
    public async Task<IResult> Handle(
        RemoveEmployeeFromOrgUnitCommand request,
        CancellationToken cancellationToken)
    {
        var membership = await context.EmployeeOrgUnitMemberships
            .FirstOrDefaultAsync(
                x => x.EmployeeId == request.EmployeeId && x.OrgUnitId == request.OrgUnitId && x.EndDate == null,
                cancellationToken);

        if (membership is null)
            return Result.NotFound("Membership not found");

        membership.EndDate = clock.UtcNow;
        membership.IsPrimary = false;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
