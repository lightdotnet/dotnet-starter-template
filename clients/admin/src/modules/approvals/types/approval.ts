/** Mirrors `Approval.Contracts/Approvals/ApprovalStatus.cs` — serialized by name. */
export enum ApprovalStatus {
  Pending = "Pending",
  Approved = "Approved",
  Rejected = "Rejected",
  Cancelled = "Cancelled",
}

/** Mirrors `Approval.Contracts/Approvals/ApprovalStepStatus.cs` — serialized by name. */
export enum ApprovalStepStatus {
  Pending = "Pending",
  Approved = "Approved",
  Rejected = "Rejected",
  Skipped = "Skipped",
}

/** Mirrors `Approval.Contracts/Approvals/ApprovalRelation.cs` — serialized by name. */
export enum ApprovalRelation {
  All = "All",
  Requested = "Requested",
  AwaitingMyDecision = "AwaitingMyDecision",
  DecidedByMe = "DecidedByMe",
}

export interface ApprovalStepDto {
  id: string;
  approvalRequestId: string;
  level: number;
  approverUserId: string;
  approverEmployeeId: string;
  status: ApprovalStepStatus;
  comment?: string | null;
  decidedAt?: string | null;
}

export interface ApprovalRequestDto {
  id: string;
  requestType: string;
  requestId: string;
  requesterUserId: string;
  requesterEmployeeId: string;
  title: string;
  content?: string | null;
  deepLinkUrl?: string | null;
  currentLevel: number;
  status: ApprovalStatus;
  created: string;
  finalizedAt?: string | null;
  steps: ApprovalStepDto[];
}

export interface ApproverStepInput {
  level: number;
  approverUserId: string;
  approverEmployeeId: string;
}

export interface CreateApprovalRequestPayload {
  requestType: string;
  requestId: string;
  requesterUserId: string;
  requesterEmployeeId: string;
  title: string;
  content?: string;
  deepLinkUrl?: string;
  approverChain: ApproverStepInput[];
}

export interface DecideApprovalPayload {
  approved: boolean;
  comment?: string;
}

export interface ApprovalSearchParams {
  requestType?: string;
  status?: ApprovalStatus;
  searchValue?: string;
  pageNumber?: number;
  pageSize?: number;
}

export interface MyApprovalsParams {
  relation?: ApprovalRelation;
  requestType?: string;
  status?: ApprovalStatus;
  searchValue?: string;
  pageNumber?: number;
  pageSize?: number;
}
