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

public class SearchLeaveRequestsQueryHandlerTests
{
    private static LeaveRequest MakeEntity(
        string employeeId,
        LeaveType leaveType,
        LeaveRequestStatus status,
        string? approvalRequestId = null) => new()
    {
        UserId = "user-" + employeeId,
        EmployeeId = employeeId,
        LeaveType = leaveType,
        StartDate = DateTimeOffset.UtcNow,
        EndDate = DateTimeOffset.UtcNow.AddDays(1),
        Status = status,
        ApprovalRequestId = approvalRequestId,
    };

    [Fact]
    public async Task Handle_ShouldScopeToOwnEmployee_WhenCannotManage()
    {
        // Arrange
        using var host = new LeaveManagementTestHost();
        await host.Context.LeaveRequests.AddRangeAsync(
            MakeEntity("employee-1", LeaveType.Annual, LeaveRequestStatus.Pending),
            MakeEntity("employee-2", LeaveType.Annual, LeaveRequestStatus.Pending));
        await host.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var approvalServiceMock = new Mock<IApprovalService>();
        var handler = new SearchLeaveRequestsQueryHandler(host.Context, approvalServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new SearchLeaveRequestsQuery(
                new LeaveRequestSearchRequest { EmployeeId = "employee-2" }, "employee-1", false),
            TestContext.Current.CancellationToken);

        // Assert
        var record = Assert.Single(result.Data.Records);
        Assert.Equal("employee-1", record.EmployeeId);
    }

    [Fact]
    public async Task Handle_ShouldHonorEmployeeFilter_WhenCanManage()
    {
        // Arrange
        using var host = new LeaveManagementTestHost();
        await host.Context.LeaveRequests.AddRangeAsync(
            MakeEntity("employee-1", LeaveType.Annual, LeaveRequestStatus.Pending),
            MakeEntity("employee-2", LeaveType.Annual, LeaveRequestStatus.Pending));
        await host.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var approvalServiceMock = new Mock<IApprovalService>();
        var handler = new SearchLeaveRequestsQueryHandler(host.Context, approvalServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new SearchLeaveRequestsQuery(
                new LeaveRequestSearchRequest { EmployeeId = "employee-2" }, "employee-1", true),
            TestContext.Current.CancellationToken);

        // Assert
        var record = Assert.Single(result.Data.Records);
        Assert.Equal("employee-2", record.EmployeeId);
    }

    [Fact]
    public async Task Handle_ShouldFilterByLeaveType()
    {
        // Arrange
        using var host = new LeaveManagementTestHost();
        await host.Context.LeaveRequests.AddRangeAsync(
            MakeEntity("employee-1", LeaveType.Annual, LeaveRequestStatus.Pending),
            MakeEntity("employee-1", LeaveType.Sick, LeaveRequestStatus.Pending));
        await host.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var approvalServiceMock = new Mock<IApprovalService>();
        var handler = new SearchLeaveRequestsQueryHandler(host.Context, approvalServiceMock.Object);

        // Act
        var result = await handler.Handle(
            new SearchLeaveRequestsQuery(
                new LeaveRequestSearchRequest { LeaveType = LeaveType.Sick }, "employee-1", false),
            TestContext.Current.CancellationToken);

        // Assert
        var record = Assert.Single(result.Data.Records);
        Assert.Equal(LeaveType.Sick, record.LeaveType);
    }

    [Fact]
    public async Task Handle_ShouldReconcilePendingRows_BeforeApplyingStatusFilter()
    {
        // Arrange
        using var host = new LeaveManagementTestHost();
        var entity = MakeEntity("employee-1", LeaveType.Annual, LeaveRequestStatus.Pending, "approval-1");
        await host.Context.LeaveRequests.AddAsync(entity, TestContext.Current.CancellationToken);
        await host.Context.SaveChangesAsync(TestContext.Current.CancellationToken);
        var approvalServiceMock = new Mock<IApprovalService>();
        approvalServiceMock
            .Setup(s => s.GetByRequestAsync("LeaveRequest", entity.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ApprovalRequestDto { Status = ApprovalStatus.Approved });
        var handler = new SearchLeaveRequestsQueryHandler(host.Context, approvalServiceMock.Object);

        // Act
        var pendingResult = await handler.Handle(
            new SearchLeaveRequestsQuery(
                new LeaveRequestSearchRequest { Status = LeaveRequestStatus.Pending }, "employee-1", false),
            TestContext.Current.CancellationToken);

        // Assert — reconciled to Approved before the Status=Pending filter runs, so it's excluded
        Assert.Empty(pendingResult.Data.Records);
        var persisted = await host.Context.LeaveRequests
            .AsNoTracking()
            .FirstAsync(x => x.Id == entity.Id, TestContext.Current.CancellationToken);
        Assert.Equal(LeaveRequestStatus.Approved, persisted.Status);
    }
}
