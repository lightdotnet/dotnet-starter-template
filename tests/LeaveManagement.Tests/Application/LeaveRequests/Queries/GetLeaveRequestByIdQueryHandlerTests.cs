using LeaveManagement.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Moq;
using StarterKit.Approval.Contracts.Approvals;
using StarterKit.Approval.Contracts.Services;
using StarterKit.LeaveManagement.Api.Application.LeaveRequests.Queries;
using StarterKit.LeaveManagement.Api.Domain.LeaveRequests;
using StarterKit.LeaveManagement.Contracts.LeaveRequests;
using Xunit;

namespace LeaveManagement.Tests.Application.LeaveRequests.Queries;

public class GetLeaveRequestByIdQueryHandlerTests
{
    private static LeaveRequest MakeEntity(
        string employeeId,
        LeaveRequestStatus status,
        string? approvalRequestId = null) => new()
    {
        UserId = "user-1",
        EmployeeId = employeeId,
        LeaveType = LeaveType.Annual,
        StartDate = DateTimeOffset.UtcNow,
        EndDate = DateTimeOffset.UtcNow.AddDays(1),
        Status = status,
        ApprovalRequestId = approvalRequestId,
    };

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        // Arrange
        using var host = new LeaveManagementTestHost();
        var approvalServiceMock = new Mock<IApprovalService>();
        var handler = new GetLeaveRequestByIdQueryHandler(host.Context, approvalServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new GetLeaveRequestByIdQuery("missing", "employee-1", false),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReject_WhenNotOwnerAndCannotManage()
    {
        // Arrange
        using var host = new LeaveManagementTestHost();
        var entity = MakeEntity("employee-1", LeaveRequestStatus.Pending);
        await host.Context.LeaveRequests.AddAsync(entity, TestContext.Current.CancellationToken);
        await host.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var approvalServiceMock = new Mock<IApprovalService>();
        var handler = new GetLeaveRequestByIdQueryHandler(host.Context, approvalServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new GetLeaveRequestByIdQuery(entity.Id, "employee-2", false),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_ShouldReturnDto_WhenOwner()
    {
        // Arrange
        using var host = new LeaveManagementTestHost();
        var entity = MakeEntity("employee-1", LeaveRequestStatus.Pending);
        await host.Context.LeaveRequests.AddAsync(entity, TestContext.Current.CancellationToken);
        await host.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var approvalServiceMock = new Mock<IApprovalService>();
        var handler = new GetLeaveRequestByIdQueryHandler(host.Context, approvalServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new GetLeaveRequestByIdQuery(entity.Id, "employee-1", false),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(entity.Id, result.Data.Id);
    }

    [Fact]
    public async Task Handle_ShouldReconcileStatus_WhenApprovalResolved()
    {
        // Arrange
        using var host = new LeaveManagementTestHost();
        var entity = MakeEntity("employee-1", LeaveRequestStatus.Pending, "approval-1");
        await host.Context.LeaveRequests.AddAsync(entity, TestContext.Current.CancellationToken);
        await host.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var approvalServiceMock = new Mock<IApprovalService>();
        approvalServiceMock
            .Setup(s => s.GetByRequestAsync("LeaveRequest", entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApprovalRequestDto { Status = ApprovalStatus.Approved });
        var handler = new GetLeaveRequestByIdQueryHandler(host.Context, approvalServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new GetLeaveRequestByIdQuery(entity.Id, "employee-1", false),
            TestContext.Current.CancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(LeaveRequestStatus.Approved, result.Data.Status);
        var persisted = await host.Context.LeaveRequests
            .AsNoTracking()
            .FirstAsync(x => x.Id == entity.Id, TestContext.Current.CancellationToken);
        Assert.Equal(LeaveRequestStatus.Approved, persisted.Status);
    }
}
