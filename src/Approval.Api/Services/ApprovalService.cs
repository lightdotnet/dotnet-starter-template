using Mapster;
using StarterKit.Approval.Api.Data;
using StarterKit.Approval.Api.Domain.Approvals;
using StarterKit.Approval.Contracts.Services;
using StarterKit.Shared;

namespace StarterKit.Approval.Api.Services;

internal class ApprovalService(
    ApprovalDbContext context,
    IPublisher publisher,
    IDateTime clock) : IApprovalService
{
    public async Task<IResult<string>> CreateAsync(
        CreateApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ApproverChain.Count == 0)
            return Result<string>.Error("At least one approver level is required.");

        if (request.DocumentTypeId is not null
            && !await context.ApprovalDocumentTypes
                .AnyAsync(x => x.Id == request.DocumentTypeId, cancellationToken))
        {
            return Result<string>.Error($"Approval document type {request.DocumentTypeId} not found");
        }

        var orderedChain = request.ApproverChain.OrderBy(x => x.Level).ToList();

        var entity = new ApprovalRequest
        {
            RequestType = request.RequestType,
            RequestId = request.RequestId,
            RequesterUserId = request.RequesterUserId,
            RequesterEmployeeId = request.RequesterEmployeeId,
            RequesterName = request.RequesterName,
            Title = request.Title,
            Content = request.Content,
            DeepLinkUrl = request.DeepLinkUrl,
            DocumentTypeId = request.DocumentTypeId,
            CurrentLevel = orderedChain[0].Level,
            Status = ApprovalStatus.Pending,
            Steps = [.. orderedChain.Select(step => new ApprovalStep
            {
                Level = step.Level,
                ApproverUserId = step.ApproverUserId,
                ApproverEmployeeId = step.ApproverEmployeeId,
                ApproverName = step.ApproverName,
                Status = ApprovalStepStatus.Pending,
            })]
        };

        await context.ApprovalRequests.AddAsync(entity, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var firstStep = entity.Steps.First(s => s.Level == entity.CurrentLevel);

        await publisher.Publish(
            new ApprovalStepPendingEvent(
                entity.Id,
                entity.Title,
                entity.DeepLinkUrl,
                firstStep.ApproverUserId,
                entity.RequesterUserId),
            cancellationToken);

        return Result<string>.Success(entity.Id);
    }

    public async Task<IResult> DecideAsync(
        string approvalRequestId,
        string decidedByUserId,
        bool approved,
        string? comment,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.ApprovalRequests
            .Include(x => x.Steps)
            .FirstOrDefaultAsync(x => x.Id == approvalRequestId, cancellationToken);

        if (entity is null)
            return Result.NotFound($"Approval request {approvalRequestId} not found");

        if (entity.Status != ApprovalStatus.Pending)
            return Result.Error("This approval request has already been finalized.");

        var step = entity.Steps.SingleOrDefault(s => s.Level == entity.CurrentLevel);

        if (step is null)
            return Result.Error("Current approval step could not be resolved.");

        if (step.ApproverUserId != decidedByUserId)
            return Result.Error("You are not the assigned approver for this step.");

        if (!approved && string.IsNullOrWhiteSpace(comment))
            return Result.Error("A reason is required when rejecting a request.");

        step.Comment = comment;
        step.DecidedAt = clock.AuditTime;

        ApprovalStep? nextStep = null;

        if (!approved)
        {
            step.Status = ApprovalStepStatus.Rejected;
            entity.Status = ApprovalStatus.Rejected;
            entity.FinalizedAt = clock.AuditTime;
        }
        else
        {
            step.Status = ApprovalStepStatus.Approved;

            nextStep = entity.Steps
                .Where(s => s.Level > entity.CurrentLevel)
                .OrderBy(s => s.Level)
                .FirstOrDefault();

            if (nextStep is null)
            {
                entity.Status = ApprovalStatus.Approved;
                entity.FinalizedAt = clock.AuditTime;
            }
            else
            {
                entity.CurrentLevel = nextStep.Level;
            }
        }

        await context.SaveChangesAsync(cancellationToken);

        if (entity.Status != ApprovalStatus.Pending)
        {
            await publisher.Publish(
                new ApprovalFinalizedEvent(
                    entity.Id, entity.Title, entity.DeepLinkUrl, entity.RequesterUserId, decidedByUserId, entity.Status),
                cancellationToken);
        }
        else if (nextStep is not null)
        {
            await publisher.Publish(
                new ApprovalStepPendingEvent(
                    entity.Id, entity.Title, entity.DeepLinkUrl, nextStep.ApproverUserId, entity.RequesterUserId),
                cancellationToken);
        }

        return Result.Success();
    }

    public async Task<IResult> CancelAsync(
        string approvalRequestId,
        CancellationToken cancellationToken = default)
    {
        var entity = await context.ApprovalRequests
            .FirstOrDefaultAsync(x => x.Id == approvalRequestId, cancellationToken);

        if (entity is null)
            return Result.NotFound($"Approval request {approvalRequestId} not found");

        if (entity.Status != ApprovalStatus.Pending)
            return Result.Error("Only a pending approval request can be cancelled.");

        entity.Status = ApprovalStatus.Cancelled;
        entity.FinalizedAt = clock.AuditTime;

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public Task<ApprovalRequestDto?> GetByRequestAsync(
        string requestType,
        string requestId,
        CancellationToken cancellationToken = default)
    {
        // (RequestType, RequestId) is an opaque reference to an object owned by the calling module,
        // not a unique key here - a caller can legitimately raise a new request for the same object
        // after an earlier one was rejected or cancelled. Return the most recent match rather than
        // throwing when more than one exists.
        return context.ApprovalRequests
            .AsNoTracking()
            .Where(x => x.RequestType == requestType && x.RequestId == requestId)
            .OrderByDescending(x => x.Created)
            .ProjectToType<ApprovalRequestDto>()
            .FirstOrDefaultAsync(cancellationToken);
    }
}
