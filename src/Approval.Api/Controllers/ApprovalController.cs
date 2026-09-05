using Light.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StarterKit.Approval.Api.Application.Approvals.Commands;
using StarterKit.Approval.Api.Application.Approvals.Queries;
using StarterKit.Approval.Contracts.Authorization;
using StarterKit.Infrastructure.Endpoints;
using StarterKit.Shared;

namespace StarterKit.Approval.Api.Controllers;

[ApiExplorerSettings(GroupName = "approval")]
[MustHavePermission(ApprovalPermissions.Requests.View)]
public class ApprovalController(ICurrentUser currentUser) : VersionedApiController
{
    private readonly string _currentUserId = currentUser.UserId
        ?? throw new ArgumentNullException(nameof(currentUser.UserId));

    [HttpGet("mine")]
    public async Task<IActionResult> GetMineAsync([FromQuery] PageQuery request)
    {
        return Ok(await Mediator.Send(new GetMyPendingApprovalsQuery(_currentUserId, request)));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAsync([FromRoute] string id)
    {
        return Ok(await Mediator.Send(new GetApprovalRequestByIdQuery(id)));
    }

    [HttpPut("{id}/decide")]
    public async Task<IActionResult> DecideAsync([FromRoute] string id, [FromBody] DecideApprovalRequest request)
    {
        return Ok(await Mediator.Send(
            new DecideApprovalStepCommand(id, _currentUserId, request.Approved, request.Comment)));
    }

    [HttpGet]
    [MustHavePermission(ApprovalPermissions.Requests.ViewAll)]
    public async Task<IActionResult> SearchAsync([FromQuery] ApprovalRequestSearchRequest request)
    {
        return Ok(await Mediator.Send(new SearchApprovalRequestsQuery(request)));
    }

    /// <summary>
    /// Creates an approval request directly over HTTP. Normal request types (Leave, etc.) are
    /// expected to create theirs via <see cref="IApprovalService"/> in-process instead — this
    /// endpoint exists for ad-hoc/admin-triggered requests and exercising the engine directly.
    /// </summary>
    [HttpPost]
    [MustHavePermission(ApprovalPermissions.Requests.ViewAll)]
    public async Task<IActionResult> PostAsync([FromBody] CreateApprovalRequest request)
    {
        return Ok(await Mediator.Send(new CreateApprovalRequestCommand(request)));
    }
}
