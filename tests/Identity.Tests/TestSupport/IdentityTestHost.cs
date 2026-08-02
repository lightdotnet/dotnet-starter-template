using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StarterKit.Identity.Api.Data;
using StarterKit.Identity.Api.Entities;
using StarterKit.Shared;

namespace Identity.Tests.TestSupport;

/// <summary>
/// Wires a Sqlite in-memory <see cref="IdentityDbContext"/> plus a real ASP.NET Identity stack
/// (mirroring <c>Identity.Api/DependencyInjection.cs</c>), so services under test exercise real
/// EF Core/Identity behavior (password hashing, claim/role sync, IQueryable translation) instead
/// of hand-mocked stand-ins.
/// </summary>
public sealed class IdentityTestHost : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _provider;

    public IdentityTestHost(FakeCurrentUser? currentUser = null, FakeDateTime? dateTime = null)
    {
        CurrentUser = currentUser ?? new FakeCurrentUser();
        DateTime = dateTime ?? new FakeDateTime();

        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddDataProtection();
        services.AddSingleton<ICurrentUser>(CurrentUser);
        services.AddSingleton<IDateTime>(DateTime);
        services.AddDbContext<IdentityDbContext>(options => options.UseSqlite(_connection));

        services
            .AddIdentityCore<User>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 3;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = false;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<IdentityDbContext>()
            .AddDefaultTokenProviders();

        _provider = services.BuildServiceProvider();

        Context = _provider.GetRequiredService<IdentityDbContext>();
        Context.Database.EnsureCreated();

        UserManager = _provider.GetRequiredService<UserManager<User>>();
        RoleManager = _provider.GetRequiredService<RoleManager<Role>>();
    }

    public FakeCurrentUser CurrentUser { get; }

    public FakeDateTime DateTime { get; }

    public IdentityDbContext Context { get; }

    public UserManager<User> UserManager { get; }

    public RoleManager<Role> RoleManager { get; }

    public void Dispose()
    {
        _provider.Dispose();
        _connection.Dispose();
    }
}
