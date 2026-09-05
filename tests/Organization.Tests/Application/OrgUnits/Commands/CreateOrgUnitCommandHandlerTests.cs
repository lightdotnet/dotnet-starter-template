using Organization.Tests.TestSupport;
using StarterKit.Organization.Api.Application.OrgUnits.Commands;
using StarterKit.Organization.Api.Entities;
using StarterKit.Organization.Contracts.OrgUnits;
using Xunit;

namespace Organization.Tests.Application.OrgUnits.Commands;

public class CreateOrgUnitCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenCompanyDoesNotExist()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var handler = new CreateOrgUnitCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new CreateOrgUnitCommand(new CreateOrgUnitRequest
            {
                CompanyId = "missing",
                Type = OrgUnitType.Department,
                Name = "Engineering",
                Code = "ENG",
            }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenParentBelongsToDifferentCompany()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var companyA = new Company { Name = "A", Code = "A" };
        var companyB = new Company { Name = "B", Code = "B" };
        await host.Context.Companies.AddRangeAsync(companyA, companyB);
        await host.Context.SaveChangesAsync();
        var parentInB = new OrgUnit { CompanyId = companyB.Id, Type = OrgUnitType.Department, Name = "B-root", Code = "B-ROOT" };
        await host.Context.OrgUnits.AddAsync(parentInB);
        await host.Context.SaveChangesAsync();
        var handler = new CreateOrgUnitCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new CreateOrgUnitCommand(new CreateOrgUnitRequest
            {
                CompanyId = companyA.Id,
                ParentId = parentInB.Id,
                Type = OrgUnitType.Team,
                Name = "Sub",
                Code = "SUB",
            }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenCodeAlreadyExistsInSameCompany()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        await host.Context.OrgUnits.AddAsync(
            new OrgUnit { CompanyId = company.Id, Type = OrgUnitType.Department, Name = "Eng", Code = "ENG" });
        await host.Context.SaveChangesAsync();
        var handler = new CreateOrgUnitCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new CreateOrgUnitCommand(new CreateOrgUnitRequest
            {
                CompanyId = company.Id,
                Type = OrgUnitType.Department,
                Name = "Engineering Duplicate",
                Code = "ENG",
            }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldAllow_SameCodeAcrossDifferentCompanies()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var companyA = new Company { Name = "A", Code = "A" };
        var companyB = new Company { Name = "B", Code = "B" };
        await host.Context.Companies.AddRangeAsync(companyA, companyB);
        await host.Context.SaveChangesAsync();
        await host.Context.OrgUnits.AddAsync(
            new OrgUnit { CompanyId = companyA.Id, Type = OrgUnitType.Department, Name = "Eng", Code = "ENG" });
        await host.Context.SaveChangesAsync();
        var handler = new CreateOrgUnitCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new CreateOrgUnitCommand(new CreateOrgUnitRequest
            {
                CompanyId = companyB.Id,
                Type = OrgUnitType.Department,
                Name = "Eng",
                Code = "ENG",
            }),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
    }
}
