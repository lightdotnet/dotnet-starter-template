using Approval.Tests.TestSupport;
using StarterKit.Approval.Api.Application.Approvals.Queries;
using StarterKit.Approval.Api.Entities;
using StarterKit.Approval.Contracts.Approvals;
using Xunit;

namespace Approval.Tests.Application.Approvals.Queries;

public class GetApprovalRequestByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var handler = new GetApprovalRequestByIdQueryHandler(host.Context);

        // Act
        var result = await handler.Handle(new GetApprovalRequestByIdQuery("missing"), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReturnRequest_RegardlessOfWhoIsAsking()
    {
        // Arrange: admin surface - no relation check, unlike GetMyApprovalRequestByIdQuery.
        using var host = new ApprovalTestHost();
        var entity = new ApprovalRequest
        {
            RequestType = "Test",
            RequestId = "req-1",
            RequesterUserId = "requester",
            RequesterEmployeeId = "requester",
            Title = "Title",
            Status = ApprovalStatus.Pending,
            CurrentLevel = 1,
            Steps = [new ApprovalStep { Level = 1, ApproverUserId = "approver-1", ApproverEmployeeId = "approver-1" }],
        };
        await host.Context.ApprovalRequests.AddAsync(entity);
        await host.Context.SaveChangesAsync();
        var handler = new GetApprovalRequestByIdQueryHandler(host.Context);

        // Act
        var result = await handler.Handle(new GetApprovalRequestByIdQuery(entity.Id), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(entity.Id, result.Data.Id);
    }
}
