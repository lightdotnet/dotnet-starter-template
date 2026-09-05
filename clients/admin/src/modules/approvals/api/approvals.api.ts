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

export function getMyPendingApprovals(params: MyApprovalsParams = {}) {
  return guardCall(() =>
    requestJson<PagedResult<ApprovalRequestDto>>("approval/mine", {
      method: "GET",
      query: {
        pageNumber: String(params.pageNumber ?? 1),
        pageSize: String(params.pageSize ?? 20),
      },
    }),
  );
}

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
  return guardCall(() => requestJson<Result<ApprovalRequestDto>>(`approval/${id}`));
}

export function createApprovalRequest(request: CreateApprovalRequestPayload) {
  return guardCall(() =>
    requestJson<Result<string>>("approval", { method: "POST", body: request }),
  );
}

export function decideApproval(id: string, request: DecideApprovalPayload) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`approval/${id}/decide`, { method: "PUT", body: request }),
  );
}
