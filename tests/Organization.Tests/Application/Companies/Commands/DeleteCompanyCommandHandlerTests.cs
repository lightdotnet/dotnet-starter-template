using Organization.Tests.TestSupport;
using StarterKit.Organization.Api.Application.Companies.Commands;
using StarterKit.Organization.Api.Entities;
using StarterKit.Organization.Contracts.OrgUnits;
using Xunit;

namespace Organization.Tests.Application.Companies.Commands;

public class DeleteCompanyCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReject_WhenCompanyStillHasOrgUnits()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "Acme", Code = "ACME" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        await host.Context.OrgUnits.AddAsync(
            new OrgUnit { CompanyId = company.Id, Type = OrgUnitType.Department, Name = "Eng", Code = "ENG" });
        await host.Context.SaveChangesAsync();
        var handler = new DeleteCompanyCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(new DeleteCompanyCommand(company.Id), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenCompanyStillHasEmployees()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "Acme", Code = "ACME" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        await host.Context.Employees.AddAsync(
            new Employee { CompanyId = company.Id, EmployeeCode = "E1", FirstName = "A", LastName = "B" });
        await host.Context.SaveChangesAsync();
        var handler = new DeleteCompanyCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(new DeleteCompanyCommand(company.Id), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldDelete_WhenCompanyHasNoDependents()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "Acme", Code = "ACME" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var handler = new DeleteCompanyCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(new DeleteCompanyCommand(company.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Null(await host.Context.Companies.FindAsync(company.Id));
    }
}
