using StarterKit.Organization.Api.Data;

namespace StarterKit.Organization.Api.Application.Employees.Commands;

internal sealed record UnlinkEmployeeLoginCommand(string EmployeeId) : ICommand<IResult>;

internal class UnlinkEmployeeLoginCommandHandler(OrganizationDbContext context)
    : ICommandHandler<UnlinkEmployeeLoginCommand, IResult>
{
    public async Task<IResult> Handle(
        UnlinkEmployeeLoginCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            return Result.NotFound($"Employee {request.EmployeeId} not found");

        employee.UserId = null;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
