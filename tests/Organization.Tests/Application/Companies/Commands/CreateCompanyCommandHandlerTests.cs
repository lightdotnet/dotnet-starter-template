using Organization.Tests.TestSupport;
using StarterKit.Organization.Api.Application.Companies.Commands;
using StarterKit.Organization.Api.Entities;
using StarterKit.Organization.Contracts.Companies;
using Xunit;

namespace Organization.Tests.Application.Companies.Commands;

public class CreateCompanyCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReject_WhenCodeAlreadyExists()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        await host.Context.Companies.AddAsync(new Company { Name = "Acme", Code = "ACME" });
        await host.Context.SaveChangesAsync();
        var handler = new CreateCompanyCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new CreateCompanyCommand(new CreateCompanyRequest { Name = "Acme Corp", Code = "ACME" }),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldCreate_WhenCodeIsUnique()
    {
        // Arrange
        using var host = new OrganizationTestHost();
        var handler = new CreateCompanyCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new CreateCompanyCommand(new CreateCompanyRequest { Name = "Acme", Code = "ACME" }),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(await host.Context.Companies.FindAsync(result.Data));
    }
}
