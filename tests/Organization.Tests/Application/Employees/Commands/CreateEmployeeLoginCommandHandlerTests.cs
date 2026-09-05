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

public class CreateEmployeeLoginCommandHandlerTests
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
        var handler = new CreateEmployeeLoginCommandHandler(host.Context, userServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new CreateEmployeeLoginCommand(employee.Id, new CreateEmployeeLoginRequest { UserName = "jane" }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        userServiceMock.Verify(s => s.CreateAsync(It.IsAny<CreateUserRequest>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldSetEmployeeUserId_WhenUserServiceSucceeds()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var employee = new Employee { CompanyId = company.Id, EmployeeCode = "E1", FirstName = "Jane", LastName = "Doe" };
        await host.Context.Employees.AddAsync(employee);
        await host.Context.SaveChangesAsync();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateUserRequest>()))
            .ReturnsAsync(Result<string>.Success("new-user-id"));
        var handler = new CreateEmployeeLoginCommandHandler(host.Context, userServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new CreateEmployeeLoginCommand(employee.Id, new CreateEmployeeLoginRequest { UserName = "jane" }),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new-user-id", result.Data);
        var updated = await host.Context.Employees.FindAsync(employee.Id);
        Assert.Equal("new-user-id", updated!.UserId);
    }

    [Fact]
    public async Task Handle_ShouldNotSetUserId_WhenUserServiceFails()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var employee = new Employee { CompanyId = company.Id, EmployeeCode = "E1", FirstName = "Jane", LastName = "Doe" };
        await host.Context.Employees.AddAsync(employee);
        await host.Context.SaveChangesAsync();
        var userServiceMock = new Mock<IUserService>();
        userServiceMock
            .Setup(s => s.CreateAsync(It.IsAny<CreateUserRequest>()))
            .ReturnsAsync(Result<string>.Error("Username already taken"));
        var handler = new CreateEmployeeLoginCommandHandler(host.Context, userServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new CreateEmployeeLoginCommand(employee.Id, new CreateEmployeeLoginRequest { UserName = "jane" }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        var updated = await host.Context.Employees.FindAsync(employee.Id);
        Assert.Null(updated!.UserId);
    }
}
