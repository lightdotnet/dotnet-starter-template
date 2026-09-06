using Approval.Tests.TestSupport;
using Light.Mediator;
using Moq;
using StarterKit.Approval.Api.Entities;
using StarterKit.Approval.Api.Events;
using StarterKit.Approval.Api.Services;
using StarterKit.Approval.Contracts.Approvals;
using StarterKit.Approval.Contracts.Services;
using Xunit;

namespace Approval.Tests.Services;

public class ApprovalServiceTests
{
    private static CreateApprovalRequest NewRequest(params (int Level, string ApproverUserId)[] chain) =>
        new(
            RequestType: "Test",
            RequestId: "req-1",
            RequesterUserId: "requester-1",
            RequesterEmployeeId: "emp-1",
            Title: "Title",
            Content: "Content",
            DeepLinkUrl: null,
            ApproverChain: chain
                .Select(c => new ApproverStepInput(c.Level, c.ApproverUserId, c.ApproverUserId))
                .ToList());

    [Fact]
    public async Task CreateAsync_ShouldReject_WhenChainIsEmpty()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var publisherMock = new Mock<IPublisher>();
        var service = new ApprovalService(host.Context, publisherMock.Object, host.DateTime);

        // Act
        var result = await service.CreateAsync(NewRequest());

        // Assert
        Assert.False(result.IsSuccess);
        publisherMock.Verify(
            p => p.Publish(It.IsAny<ApprovalStepPendingEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePendingRequest_AndNotifyFirstApprover()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var publisherMock = new Mock<IPublisher>();
        var service = new ApprovalService(host.Context, publisherMock.Object, host.DateTime);

        // Act
        var result = await service.CreateAsync(NewRequest((1, "approver-1"), (2, "approver-2")));

        // Assert
        Assert.True(result.IsSuccess);
        var entity = await host.Context.ApprovalRequests.FindAsync(result.Data);
        Assert.NotNull(entity);
        Assert.Equal(ApprovalStatus.Pending, entity.Status);
        Assert.Equal(1, entity.CurrentLevel);
        publisherMock.Verify(
            p => p.Publish(
                It.Is<ApprovalStepPendingEvent>(e =>
                    e.ApprovalRequestId == result.Data && e.ApproverUserId == "approver-1"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static async Task<(ApprovalTestHost Host, string RequestId)> SeedTwoLevelRequestAsync()
    {
        var host = new ApprovalTestHost();
        var publisherMock = new Mock<IPublisher>();
        var service = new ApprovalService(host.Context, publisherMock.Object, host.DateTime);
        var created = await service.CreateAsync(NewRequest((1, "approver-1"), (2, "approver-2")));
        return (host, created.Data!);
    }

    [Fact]
    public async Task DecideAsync_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);

        // Act
        var result = await service.DecideAsync("missing", "approver-1", true, null);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DecideAsync_ShouldReject_WhenCallerIsNotTheAssignedApprover()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);

        // Act
        var result = await service.DecideAsync(requestId, "someone-else", true, null);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DecideAsync_ShouldRequireComment_WhenRejecting()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);

        // Act
        var result = await service.DecideAsync(requestId, "approver-1", false, null);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DecideAsync_ShouldAdvanceLevel_AndNotifyNextApprover_WhenApprovedAndMoreLevelsRemain()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var publisherMock = new Mock<IPublisher>();
        var service = new ApprovalService(host.Context, publisherMock.Object, host.DateTime);

        // Act
        var result = await service.DecideAsync(requestId, "approver-1", true, null);

        // Assert
        Assert.True(result.IsSuccess);
        var entity = await host.Context.ApprovalRequests.FindAsync(requestId);
        Assert.Equal(ApprovalStatus.Pending, entity!.Status);
        Assert.Equal(2, entity.CurrentLevel);
        publisherMock.Verify(
            p => p.Publish(
                It.Is<ApprovalStepPendingEvent>(e => e.ApproverUserId == "approver-2"),
                It.IsAny<CancellationToken>()),
            Times.Once);
        publisherMock.Verify(
            p => p.Publish(It.IsAny<ApprovalFinalizedEvent>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DecideAsync_ShouldFinalizeAsApproved_WhenLastLevelApproves()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var publisherMock = new Mock<IPublisher>();
        var service = new ApprovalService(host.Context, publisherMock.Object, host.DateTime);
        await service.DecideAsync(requestId, "approver-1", true, null);

        // Act
        var result = await service.DecideAsync(requestId, "approver-2", true, "Looks good");

        // Assert
        Assert.True(result.IsSuccess);
        var entity = await host.Context.ApprovalRequests.FindAsync(requestId);
        Assert.Equal(ApprovalStatus.Approved, entity!.Status);
        Assert.NotNull(entity.FinalizedAt);
        publisherMock.Verify(
            p => p.Publish(
                It.Is<ApprovalFinalizedEvent>(e => e.Status == ApprovalStatus.Approved && e.DecidedByUserId == "approver-2"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DecideAsync_ShouldFinalizeAsRejected_AndStopAdvancing()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var publisherMock = new Mock<IPublisher>();
        var service = new ApprovalService(host.Context, publisherMock.Object, host.DateTime);

        // Act
        var result = await service.DecideAsync(requestId, "approver-1", false, "Not compliant");

        // Assert
        Assert.True(result.IsSuccess);
        var entity = await host.Context.ApprovalRequests.FindAsync(requestId);
        Assert.Equal(ApprovalStatus.Rejected, entity!.Status);
        Assert.Equal(1, entity.CurrentLevel);
        publisherMock.Verify(
            p => p.Publish(
                It.Is<ApprovalFinalizedEvent>(e => e.Status == ApprovalStatus.Rejected),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DecideAsync_ShouldReject_WhenRequestAlreadyFinalized()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);
        await service.DecideAsync(requestId, "approver-1", false, "Not compliant");

        // Act
        var result = await service.DecideAsync(requestId, "approver-2", true, null);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CancelAsync_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);

        // Act
        var result = await service.CancelAsync("missing");

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CancelAsync_ShouldReject_WhenRequestIsNotPending()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);
        await service.DecideAsync(requestId, "approver-1", false, "Not compliant");

        // Act
        var result = await service.CancelAsync(requestId);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task CancelAsync_ShouldCancel_WhenRequestIsPending()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);

        // Act
        var result = await service.CancelAsync(requestId);

        // Assert
        Assert.True(result.IsSuccess);
        var entity = await host.Context.ApprovalRequests.FindAsync(requestId);
        Assert.Equal(ApprovalStatus.Cancelled, entity!.Status);
        Assert.NotNull(entity.FinalizedAt);
    }

    [Fact]
    public async Task GetByRequestAsync_ShouldReturnNull_WhenNoMatchExists()
    {
        // Arrange
        using var host = new ApprovalTestHost();
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);

        // Act
        var dto = await service.GetByRequestAsync("Leave", "missing");

        // Assert
        Assert.Null(dto);
    }

    [Fact]
    public async Task GetByRequestAsync_ShouldReturnDto_WhenMatchExists()
    {
        // Arrange
        var (host, requestId) = await SeedTwoLevelRequestAsync();
        using var _ = host;
        var service = new ApprovalService(host.Context, Mock.Of<IPublisher>(), host.DateTime);

        // Act
        var dto = await service.GetByRequestAsync("Test", "req-1");

        // Assert
        Assert.NotNull(dto);
        Assert.Equal(requestId, dto!.Id);
    }
}
