"use server";

import { randomUUID } from "crypto";
import { revalidatePath } from "next/cache";
import { resolveSession } from "@/modules/identity/user-profile";
import { createApprovalRequest } from "@/modules/approvals/api/approvals.api";
import type { ApproverStepInput } from "@/modules/approvals/types/approval";

export interface CreateApprovalRequestState {
  error?: string;
  success?: boolean;
}

export interface CreateApprovalRequestInput {
  requestType: string;
  title: string;
  approverUserIds: string[];
}

/**
 * Test/admin harness for the generic Approval engine — creates a request as
 * the current user, with the picked approvers as the chain (level = row
 * order). A real request type (e.g. Leave) resolves its chain server-side
 * via `IApprovalService` instead of going through this HTTP endpoint.
 *
 * `approverEmployeeId` has no real employee behind it here — Approval never
 * validates it (it's opaque bookkeeping), so the picked user's id is reused
 * as a placeholder rather than adding a second picker per row just for this
 * test screen.
 */
export async function createApprovalRequestAction(
  input: CreateApprovalRequestInput,
): Promise<CreateApprovalRequestState> {
  const session = await resolveSession();
  if (!session || !session.profile) {
    return { error: "Your session has expired. Please sign in again." };
  }

  if (!input.title.trim()) {
    return { error: "Title is required." };
  }

  if (input.approverUserIds.length === 0) {
    return { error: "At least one approver level is required." };
  }

  const approverChain: ApproverStepInput[] = input.approverUserIds.map((userId, index) => ({
    level: index + 1,
    approverUserId: userId,
    approverEmployeeId: userId,
  }));

  const requesterId = session.profile.id;

  const result = await createApprovalRequest({
    requestType: input.requestType.trim() || "Test",
    requestId: randomUUID(),
    requesterUserId: requesterId,
    requesterEmployeeId: requesterId,
    title: input.title.trim(),
    approverChain,
  });

  if (!result.isSuccess) {
    return { error: result.message || "Failed to create approval request." };
  }

  revalidatePath("/approvals");
  return { success: true };
}
