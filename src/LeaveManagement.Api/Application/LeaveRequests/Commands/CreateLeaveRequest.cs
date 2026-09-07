using StarterKit.Approval.Contracts.Approvals;
using StarterKit.Approval.Contracts.Services;
using StarterKit.LeaveManagement.Api.Data;
using StarterKit.LeaveManagement.Api.Domain.LeaveRequests;
using StarterKit.Organization.Contracts.Services;

namespace StarterKit.LeaveManagement.Api.Application.LeaveRequests.Commands;

internal sealed record CreateLeaveRequestCommand(
    CreateLeaveRequest Model,
    string RequesterUserId,
    string? RequesterEmployeeId) : ICommand<IResult<string>>;

internal class CreateLeaveRequestCommandHandler(
    LeaveManagementDbContext context,
    IOrgDirectoryService orgDirectoryService,
    IApprovalService approvalService)
    : ICommandHandler<CreateLeaveRequestCommand, IResult<string>>
{
    public async Task<IResult<string>> Handle(
        CreateLeaveRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.RequesterEmployeeId))
            return Result<string>.Error("Your account is not linked to an employee record.");

        var model = request.Model;

        if (model.EndDate < model.StartDate)
            return Result<string>.Error("End date cannot be before start date.");

        var candidates = await orgDirectoryService.GetApproverCandidatesAsync(
            request.RequesterEmployeeId, cancellationToken);

        if (candidates.Count == 0)
            return Result<string>.Error("No approver could be determined for this employee's department.");

        var approver = candidates.FirstOrDefault(x => x.EmployeeId == model.ApproverEmployeeId);

        if (approver is null)
            return Result<string>.Error("Invalid approver selection.");

        var entity = new LeaveRequest
        {
            UserId = request.RequesterUserId,
            EmployeeId = request.RequesterEmployeeId,
            LeaveType = model.LeaveType,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Reason = model.Reason,
            Status = LeaveRequestStatus.Pending,
        };

        await context.LeaveRequests.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        // The JWT carries no name claims, so the requester's display name is resolved from their
        // Organization employee record instead — same source as the approver's own name above.
        var requesterName = await orgDirectoryService.GetEmployeeNameAsync(
            request.RequesterEmployeeId, cancellationToken);

        var approvalResult = await approvalService.CreateAsync(
            new CreateApprovalRequest(
                RequestType: "LeaveRequest",
                RequestId: entity.Id,
                RequesterUserId: request.RequesterUserId,
                RequesterEmployeeId: request.RequesterEmployeeId,
                RequesterName: requesterName,
                Title: $"{model.LeaveType} leave request",
                Content: model.Reason,
                DeepLinkUrl: $"/leave-requests/{entity.Id}",
                DocumentTypeId: null,
                ApproverChain:
                [
                    new ApproverStepInput(1, approver.UserId, approver.EmployeeId, approver.Name),
                ]),
            cancellationToken);

        if (!approvalResult.IsSuccess)
        {
            context.LeaveRequests.Remove(entity);
            await context.SaveChangesAsync(cancellationToken);
            return approvalResult;
        }

        entity.ApprovalRequestId = approvalResult.Data;
        await context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success(entity.Id);
    }
}
