import { approvalApi } from "@/lib/server/backend-api";

const { requestJson } = approvalApi;
import { guardCall, guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse, PagedResult, Result } from "@/types/api";
import type {
  ApprovalRequestDto,
  ApprovalSearchParams,
  CreateApprovalRequestPayload,
  DecideApprovalPayload,
  MyApprovalsParams,
} from "@/modules/approvals/types/approval";

/** Self-service — requests the current user is related to (as requester or approver), scoped
 * server-side by `UserApprovalController`. `params.relation` narrows to one slice (requested by
 * me / awaiting my decision / decided by me); omitted, it returns all of them. */
export function getMyApprovals(params: MyApprovalsParams = {}) {
  return guardCall(() =>
    requestJson<PagedResult<ApprovalRequestDto>>("user_approval", {
      method: "GET",
      query: {
        relation: params.relation,
        requestType: params.requestType,
        status: params.status,
        searchValue: params.searchValue,
        pageNumber: String(params.pageNumber ?? 1),
        pageSize: String(params.pageSize ?? 20),
      },
    }),
  );
}

/** Admin — every request, unrestricted. Requires `approval.requests.view_all`. */
export function searchApprovals(params: ApprovalSearchParams = {}) {
  return guardCall(() =>
    requestJson<PagedResult<ApprovalRequestDto>>("approval", {
      method: "GET",
      query: {
        requestType: params.requestType,
        status: params.status,
        searchValue: params.searchValue,
        pageNumber: String(params.pageNumber ?? 1),
        pageSize: String(params.pageSize ?? 20),
      },
    }),
  );
}

export function getApprovalById(id: string) {
  return guardCall(() => requestJson<Result<ApprovalRequestDto>>(`user_approval/${id}`));
}

/** Self-service create — the backend overrides `requesterUserId` to the caller regardless of
 * what's sent, so this always creates as the current user. */
export function createApprovalRequest(request: CreateApprovalRequestPayload) {
  return guardCall(() =>
    requestJson<Result<string>>("user_approval", { method: "POST", body: request }),
  );
}

/** Admin/test harness create — arbitrary requester + approver chain. Requires `approval.requests.view_all`. */
export function createTestApprovalRequest(request: CreateApprovalRequestPayload) {
  return guardCall(() =>
    requestJson<Result<string>>("approval", { method: "POST", body: request }),
  );
}

export function decideApproval(id: string, request: DecideApprovalPayload) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`user_approval/${id}/decide`, { method: "PUT", body: request }),
  );
}
