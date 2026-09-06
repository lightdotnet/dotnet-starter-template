import { approvalApi } from "@/lib/server/backend-api";

const { requestJson } = approvalApi;
import { guardCall, guardResponseCall } from "@/lib/server/call-guard";
import type { ApiResponse, Result } from "@/types/api";
import type {
  ApprovalDocumentTypeDto,
  CreateApprovalDocumentTypeRequest,
} from "@/modules/approvals/types/document-type";

export function getApprovalDocumentTypes(params: { activeOnly?: boolean } = {}) {
  return guardCall(() =>
    requestJson<Result<ApprovalDocumentTypeDto[]>>("approval_document_type", {
      method: "GET",
      query: { activeOnly: params.activeOnly ? "true" : undefined },
    }),
  );
}

export function getApprovalDocumentTypeById(id: string) {
  return guardCall(() =>
    requestJson<Result<ApprovalDocumentTypeDto>>(`approval_document_type/${id}`),
  );
}

export function createApprovalDocumentType(request: CreateApprovalDocumentTypeRequest) {
  return guardCall(() =>
    requestJson<Result<string>>("approval_document_type", { method: "POST", body: request }),
  );
}

export function updateApprovalDocumentType(dto: ApprovalDocumentTypeDto) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`approval_document_type/${dto.id}`, { method: "PUT", body: dto }),
  );
}

export function deleteApprovalDocumentType(id: string) {
  return guardResponseCall(() =>
    requestJson<ApiResponse>(`approval_document_type/${id}`, { method: "DELETE" }),
  );
}
