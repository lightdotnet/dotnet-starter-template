using Approval.Tests.TestSupport;
using StarterKit.Approval.Api.Application.DocumentTypes.Commands;
using StarterKit.Approval.Api.Domain.Approvals;
using StarterKit.Approval.Contracts.DocumentTypes;
using Xunit;

namespace Approval.Tests.Application.DocumentTypes.Commands;

public class CreateApprovalDocumentTypeCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldCreateDocumentType_WhenCodeIsUnique()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var handler = new CreateApprovalDocumentTypeCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new CreateApprovalDocumentTypeCommand(
                new CreateApprovalDocumentTypeRequest("Invoice", "INV", null, true)),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        var entity = await host.Context.ApprovalDocumentTypes.FindAsync(result.Data);
        Assert.NotNull(entity);
        Assert.Equal("INV", entity!.Code);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenCodeAlreadyExists()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        await host.Context.ApprovalDocumentTypes.AddAsync(
            new ApprovalDocumentType { Name = "Invoice", Code = "INV" });
        await host.Context.SaveChangesAsync();
        var handler = new CreateApprovalDocumentTypeCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(
            new CreateApprovalDocumentTypeCommand(
                new CreateApprovalDocumentTypeRequest("Invoice Duplicate", "INV", null, true)),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }
}
