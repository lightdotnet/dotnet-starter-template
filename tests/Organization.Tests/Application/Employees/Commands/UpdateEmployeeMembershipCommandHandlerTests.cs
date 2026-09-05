using Microsoft.EntityFrameworkCore;
using Organization.Tests.TestSupport;
using StarterKit.Organization.Api.Application.Employees.Commands;
using StarterKit.Organization.Api.Entities;
using StarterKit.Organization.Contracts.Employees;
using StarterKit.Organization.Contracts.OrgUnits;
using Xunit;

namespace Organization.Tests.Application.Employees.Commands;

public class UpdateEmployeeMembershipCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldClearOtherPrimaryMemberships_WhenPromotingToPrimary()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var company = new Company { Name = "A", Code = "A" };
        await host.Context.Companies.AddAsync(company);
        await host.Context.SaveChangesAsync();
        var employee = new Employee { CompanyId = company.Id, EmployeeCode = "E1", FirstName = "A", LastName = "B" };
        var unit1 = new OrgUnit { CompanyId = company.Id, Type = OrgUnitType.Department, Name = "U1", Code = "U1" };
        var unit2 = new OrgUnit { CompanyId = company.Id, Type = OrgUnitType.Department, Name = "U2", Code = "U2" };
        await host.Context.Employees.AddAsync(employee);
        await host.Context.OrgUnits.AddRangeAsync(unit1, unit2);
        await host.Context.SaveChangesAsync();
        await host.Context.EmployeeOrgUnitMemberships.AddRangeAsync(
            new EmployeeOrgUnitMembership { EmployeeId = employee.Id, OrgUnitId = unit1.Id, IsPrimary = true, StartDate = DateTimeOffset.UtcNow },
            new EmployeeOrgUnitMembership { EmployeeId = employee.Id, OrgUnitId = unit2.Id, IsPrimary = false, StartDate = DateTimeOffset.UtcNow });
        await host.Context.SaveChangesAsync();
        var handler = new UpdateEmployeeMembershipCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new UpdateEmployeeMembershipCommand(
                employee.Id, unit2.Id, new UpdateEmployeeMembershipRequest { IsPrimary = true }),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        // AsNoTracking: the primary-clearing step runs via ExecuteUpdateAsync, which bypasses the
        // change tracker, so a tracked read here would return the stale in-memory IsPrimary value.
        var memberships = host.Context.EmployeeOrgUnitMemberships
            .AsNoTracking()
            .Where(x => x.EmployeeId == employee.Id && x.EndDate == null)
            .ToList();
        Assert.Single(memberships, x => x.OrgUnitId == unit1.Id && !x.IsPrimary);
        Assert.Single(memberships, x => x.OrgUnitId == unit2.Id && x.IsPrimary);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMembershipNotActive()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var handler = new UpdateEmployeeMembershipCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new UpdateEmployeeMembershipCommand("missing", "missing", new UpdateEmployeeMembershipRequest()),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
