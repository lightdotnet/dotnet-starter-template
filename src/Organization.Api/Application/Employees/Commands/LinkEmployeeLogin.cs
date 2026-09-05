using StarterKit.Identity.Contracts.Services;
using StarterKit.Organization.Api.Data;
using StarterKit.Organization.Api.Domain.Employees;
using StarterKit.Shared.Constants;

namespace StarterKit.Organization.Api.Application.Employees.Commands;

internal sealed record LinkEmployeeLoginCommand(string EmployeeId, LinkEmployeeLoginRequest Model)
    : ICommand<IResult>;

internal class LinkEmployeeLoginCommandHandler(OrganizationDbContext context, IUserService userService)
    : ICommandHandler<LinkEmployeeLoginCommand, IResult>
{
    public async Task<IResult> Handle(
        LinkEmployeeLoginCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .Where(new EmployeeByIdSpec(request.EmployeeId))
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is null)
            return Result.NotFound($"Employee {request.EmployeeId} not found");

        if (!string.IsNullOrEmpty(employee.UserId))
            return Result.Error("Employee already has a login account.");

        var userId = request.Model.UserId;

        var userResult = await userService.GetByIdAsync(userId);

        if (!userResult.IsSuccess)
            return Result.NotFound($"User {userId} not found");

        var alreadyLinked = await context.Employees
            .AnyAsync(x => x.UserId == userId, cancellationToken);

        if (alreadyLinked)
            return Result.Error("This user account is already linked to another employee.");

        employee.UserId = userId;

        await context.SaveChangesAsync(cancellationToken);

        await userService.SetClaimAsync(userId, ClaimTypeConstants.EmployeeId, employee.Id);

        return Result.Success();
    }
}
