using Organization.Tests.TestSupport;
using StarterKit.Organization.Api.Application.Employees.Queries;
using StarterKit.Organization.Api.Domain.Companies;
using StarterKit.Organization.Api.Domain.Employees;
using StarterKit.Organization.Api.Domain.OrgUnits;
using StarterKit.Organization.Contracts.Employees;
using StarterKit.Organization.Contracts.OrgUnits;
using Xunit;

namespace Organization.Tests.Application.Employees.Queries;

public class SearchEmployeesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldFilterByOrgUnit_UsingOnlyActiveMemberships()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var unit = new OrgUnit { CompanyId = company.Id, Type = OrgUnitType.Department, Name = "U", Code = "U" };
        var activeMember = new Employee { CompanyId = company.Id, EmployeeCode = "E1", FirstName = "Active", LastName = "Member" };
        var formerMember = new Employee { CompanyId = company.Id, EmployeeCode = "E2", FirstName = "Former", LastName = "Member" };
        var nonMember = new Employee { CompanyId = company.Id, EmployeeCode = "E3", FirstName = "Non", LastName = "Member" };
        await host.Context.OrgUnits.AddAsync(unit);
        await host.Context.Employees.AddRangeAsync(activeMember, formerMember, nonMember);
        await host.Context.SaveChangesAsync();
        await host.Context.EmployeeOrgUnitMemberships.AddRangeAsync(
            new EmployeeOrgUnitMembership { EmployeeId = activeMember.Id, OrgUnitId = unit.Id, StartDate = DateTimeOffset.UtcNow },
            new EmployeeOrgUnitMembership
            {
                EmployeeId = formerMember.Id, OrgUnitId = unit.Id,
                StartDate = DateTimeOffset.UtcNow.AddDays(-10), EndDate = DateTimeOffset.UtcNow,
            });
        await host.Context.SaveChangesAsync();
        var handler = new SearchEmployeesQueryHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new SearchEmployeesQuery(new EmployeeSearchRequest { OrgUnitId = unit.Id, PageNumber = 1, PageSize = 10 }),
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Data.TotalRecords);
        Assert.Equal(activeMember.Id, result.Data.Records.Single().Id);
    }

    [Fact]
    public async Task Handle_ShouldFilterByCompanyAndEmploymentStatus()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var companyA = new Company { Name = "A", Code = "A" };
        var companyB = new Company { Name = "B", Code = "B" };
        await host.Context.Companies.AddRangeAsync(companyA, companyB);
        await host.Context.SaveChangesAsync();
        await host.Context.Employees.AddRangeAsync(
            new Employee { CompanyId = companyA.Id, EmployeeCode = "E1", FirstName = "Active", LastName = "A", EmploymentStatus = EmploymentStatus.Active },
            new Employee { CompanyId = companyA.Id, EmployeeCode = "E2", FirstName = "Terminated", LastName = "A", EmploymentStatus = EmploymentStatus.Terminated },
            new Employee { CompanyId = companyB.Id, EmployeeCode = "E3", FirstName = "Active", LastName = "B", EmploymentStatus = EmploymentStatus.Active });
        await host.Context.SaveChangesAsync();
        var handler = new SearchEmployeesQueryHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new SearchEmployeesQuery(new EmployeeSearchRequest
            {
                CompanyId = companyA.Id, EmploymentStatus = EmploymentStatus.Active, PageNumber = 1, PageSize = 10,
            }),
            CancellationToken.None);

        // Assert
        Assert.Equal(1, result.Data.TotalRecords);
        Assert.Equal("Active", result.Data.Records.Single().FirstName);
    }
}
