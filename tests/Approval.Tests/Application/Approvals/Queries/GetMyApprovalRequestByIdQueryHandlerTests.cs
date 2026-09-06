using Approval.Tests.TestSupport;
using StarterKit.Approval.Api.Application.Approvals.Queries;
using StarterKit.Approval.Api.Entities;
using StarterKit.Approval.Contracts.Approvals;
using Xunit;

namespace Approval.Tests.Application.Approvals.Queries;

public class GetMyApprovalRequestByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var handler = new GetMyApprovalRequestByIdQueryHandler(host.Context);

        // Act
        var result = await handler.Handle(new GetMyApprovalRequestByIdQuery("missing", "user-1"), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenUserIsNeitherRequesterNorApprover()
    {
        // Arrange
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
        var handler = new GetMyApprovalRequestByIdQueryHandler(host.Context);

        // Act
        var result = await handler.Handle(new GetMyApprovalRequestByIdQuery(entity.Id, "unrelated-user"), CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReturnRequest_WhenUserIsTheRequester()
    {
        // Arrange
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
        var handler = new GetMyApprovalRequestByIdQueryHandler(host.Context);

        // Act
        var result = await handler.Handle(new GetMyApprovalRequestByIdQuery(entity.Id, "requester"), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(entity.Id, result.Data.Id);
    }

    [Fact]
    public async Task Handle_ShouldReturnRequest_WhenUserIsAnApproverOnSomeStep()
    {
        // Arrange
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
            Steps =
            [
                new ApprovalStep { Level = 1, ApproverUserId = "approver-1", ApproverEmployeeId = "approver-1" },
                new ApprovalStep { Level = 2, ApproverUserId = "approver-2", ApproverEmployeeId = "approver-2" },
            ],
        };
        await host.Context.ApprovalRequests.AddAsync(entity);
        await host.Context.SaveChangesAsync();
        var handler = new GetMyApprovalRequestByIdQueryHandler(host.Context);

        // Act
        var result = await handler.Handle(new GetMyApprovalRequestByIdQuery(entity.Id, "approver-2"), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(entity.Id, result.Data.Id);
    }
}
