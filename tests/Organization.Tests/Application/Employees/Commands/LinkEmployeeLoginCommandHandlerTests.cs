using Light.Contracts;
using Moq;
using Organization.Tests.TestSupport;
using StarterKit.Identity.Contracts;
using StarterKit.Identity.Contracts.Services;
using StarterKit.Organization.Api.Application.Employees.Commands;
using StarterKit.Organization.Api.Entities;
using StarterKit.Organization.Contracts.Employees;
using Xunit;

namespace Organization.Tests.Application.Employees.Commands;

public class LinkEmployeeLoginCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReject_WhenEmployeeAlreadyHasLogin()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var employee = new Employee
        {
            CompanyId = company.Id, EmployeeCode = "E1", FirstName = "A", LastName = "B", UserId = "existing-user",
        };
        await host.Context.Employees.AddAsync(employee);
        await host.Context.SaveChangesAsync();
        var userServiceMock = new Mock<IUserService>();
        var handler = new LinkEmployeeLoginCommandHandler(host.Context, userServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new LinkEmployeeLoginCommand(employee.Id, new LinkEmployeeLoginRequest { UserId = "user-2" }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenUserIdAlreadyLinkedToAnotherEmployee()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var employee = new Employee { CompanyId = company.Id, EmployeeCode = "E1", FirstName = "A", LastName = "B" };
        var otherEmployee = new Employee
        {
            CompanyId = company.Id, EmployeeCode = "E2", FirstName = "C", LastName = "D", UserId = "user-1",
        };
        await host.Context.Employees.AddRangeAsync(employee, otherEmployee);
        await host.Context.SaveChangesAsync();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.GetByIdAsync("user-1"))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = "user-1", UserName = "jane" }));
        var handler = new LinkEmployeeLoginCommandHandler(host.Context, userServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new LinkEmployeeLoginCommand(employee.Id, new LinkEmployeeLoginRequest { UserId = "user-1" }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldLink_WhenUserExistsAndIsUnlinked()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var employee = new Employee { CompanyId = company.Id, EmployeeCode = "E1", FirstName = "A", LastName = "B" };
        await host.Context.Employees.AddAsync(employee);
        await host.Context.SaveChangesAsync();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.GetByIdAsync("user-1"))
            .ReturnsAsync(Result<UserDto>.Success(new UserDto { Id = "user-1", UserName = "jane" }));
        var handler = new LinkEmployeeLoginCommandHandler(host.Context, userServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new LinkEmployeeLoginCommand(employee.Id, new LinkEmployeeLoginRequest { UserId = "user-1" }),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var updated = await host.Context.Employees.FindAsync(employee.Id);
        Assert.Equal("user-1", updated!.UserId);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var employee = new Employee { CompanyId = company.Id, EmployeeCode = "E1", FirstName = "A", LastName = "B" };
        await host.Context.Employees.AddAsync(employee);
        await host.Context.SaveChangesAsync();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.GetByIdAsync("missing"))
            .ReturnsAsync(Result<UserDto>.NotFound("User missing not found"));
        var handler = new LinkEmployeeLoginCommandHandler(host.Context, userServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new LinkEmployeeLoginCommand(employee.Id, new LinkEmployeeLoginRequest { UserId = "missing" }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
