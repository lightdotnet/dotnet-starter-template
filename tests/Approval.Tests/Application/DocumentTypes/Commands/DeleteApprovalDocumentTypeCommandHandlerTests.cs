using Approval.Tests.TestSupport;
using StarterKit.Approval.Api.Application.DocumentTypes.Commands;
using StarterKit.Approval.Api.Domain.Approvals;
using Xunit;

namespace Approval.Tests.Application.DocumentTypes.Commands;

public class DeleteApprovalDocumentTypeCommandHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenDocumentTypeDoesNotExist()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var handler = new DeleteApprovalDocumentTypeCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(new DeleteApprovalDocumentTypeCommand("missing"), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenDocumentTypeIsInUse()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var docType = new ApprovalDocumentType { Name = "Invoice", Code = "INV" };
        await host.Context.ApprovalDocumentTypes.AddAsync(docType);
        await host.Context.SaveChangesAsync();
        await host.Context.ApprovalRequests.AddAsync(new ApprovalRequest
        {
            RequestType = "Test",
            RequestId = "req-1",
            RequesterUserId = "requester",
            RequesterEmployeeId = "emp-1",
            Title = "Title",
            DocumentTypeId = docType.Id,
        });
        await host.Context.SaveChangesAsync();
        var handler = new DeleteApprovalDocumentTypeCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(new DeleteApprovalDocumentTypeCommand(docType.Id), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldDelete_WhenNotInUse()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var docType = new ApprovalDocumentType { Name = "Invoice", Code = "INV" };
        await host.Context.ApprovalDocumentTypes.AddAsync(docType);
        await host.Context.SaveChangesAsync();
        var handler = new DeleteApprovalDocumentTypeCommandHandler(host.Context);

        // Act
        var result = await handler.Handle(new DeleteApprovalDocumentTypeCommand(docType.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(host.Context.ApprovalDocumentTypes.Any(x => x.Id == docType.Id));
    }
}
