using StarterKit.Identity.Contracts;
using StarterKit.Identity.Contracts.Services;
using StarterKit.Organization.Api.Data;

namespace StarterKit.Organization.Api.Application.Employees.Commands;

internal sealed record CreateEmployeeLoginCommand(string EmployeeId, CreateEmployeeLoginRequest Model)
    : ICommand<IResult<string>>;

internal class CreateEmployeeLoginCommandHandler(OrganizationDbContext context, IUserService userService)
    : ICommandHandler<CreateEmployeeLoginCommand, IResult<string>>
{
    public async Task<IResult<string>> Handle(
        CreateEmployeeLoginCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await context.Employees
            .FirstOrDefaultAsync(x => x.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            return Result<string>.NotFound($"Employee {request.EmployeeId} not found");

        if (!string.IsNullOrEmpty(employee.UserId))
            return Result<string>.Error("Employee already has a login account.");

        var model = request.Model;

        var userResult = await userService.CreateAsync(new CreateUserRequest
        {
            UserName = model.UserName,
            Password = model.Password,
            Email = model.Email,
            PhoneNumber = model.PhoneNumber,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
        });

        if (!userResult.IsSuccess)
            return userResult;

        employee.UserId = userResult.Data;

        await context.SaveChangesAsync(cancellationToken);

        return userResult;
    }
}
