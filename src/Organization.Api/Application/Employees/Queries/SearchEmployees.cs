using Light.EntityFrameworkCore.Extensions;
using Light.Specification;
using Mapster;
using StarterKit.Organization.Api.Data;
using StarterKit.Persistence.Extensions;

namespace StarterKit.Organization.Api.Application.Employees.Queries;

internal sealed record SearchEmployeesQuery(
    EmployeeSearchRequest Request) : IQuery<PagedResult<EmployeeDto>>;

internal class SearchEmployeesQueryHandler(OrganizationDbContext context)
    : IQueryHandler<SearchEmployeesQuery, PagedResult<EmployeeDto>>
{
    public Task<PagedResult<EmployeeDto>> Handle(
        SearchEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var lookup = request.Request;

        var query = context.Employees
            .AsNoTracking()
            .WhereIf(!string.IsNullOrEmpty(lookup.CompanyId), x => x.CompanyId == lookup.CompanyId)
            .WhereIf(lookup.EmploymentStatus.HasValue, x => x.EmploymentStatus == lookup.EmploymentStatus!.Value)
            .WhereIf(!string.IsNullOrEmpty(lookup.SearchValue),
                x => x.FirstName.Contains(lookup.SearchValue!)
                    || x.LastName.Contains(lookup.SearchValue!)
                    || x.EmployeeCode.Contains(lookup.SearchValue!));

        if (!string.IsNullOrEmpty(lookup.OrgUnitId))
        {
            query = query.Where(x => x.Memberships.Any(m => m.OrgUnitId == lookup.OrgUnitId && m.EndDate == null));
        }

        return query
            .OrderBy(x => x.FirstName).ThenBy(x => x.LastName)
            .ProjectToType<EmployeeDto>()
            .ToPagedResultAsync(lookup, cancellationToken);
    }
}
