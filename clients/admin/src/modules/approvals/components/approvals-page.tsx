import { redirect } from "next/navigation";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { getMyApprovals, searchApprovals } from "@/modules/approvals/api/approvals.api";
import { AllApprovalsTable } from "@/modules/approvals/components/all-approvals-table";
import {
  ApprovalHistoryTable,
  type ApprovalOwnerRole,
} from "@/modules/approvals/components/approval-history-table";
import { MyApprovalsTable } from "@/modules/approvals/components/my-approvals-table";
import { hasPermission } from "@/lib/server/authorization";
import { resolveSession } from "@/modules/identity/user-profile";
import { getAllUsers } from "@/modules/identity/users/api/users.api";
import { getDisplayName } from "@/lib/shared/user-display";
import { APPROVALS_PERMISSIONS } from "@/modules/approvals/constants/permissions";
import { ApprovalRelation } from "@/modules/approvals/types/approval";

export async function ApprovalsPage() {
  const session = await resolveSession();
  if (!session || !session.profile) {
    redirect("/login");
  }

  const myId = session.profile.id;
  const canViewAll = hasPermission(session, APPROVALS_PERMISSIONS.ViewAll);

  const usersResult = await getAllUsers();
  const userNamesById = new Map(
    (usersResult.data ?? []).map((user) => [user.id, getDisplayName(user)]),
  );

  const [awaitingResult, myRequestsResult] = await Promise.all([
    getMyApprovals({ relation: ApprovalRelation.AwaitingMyDecision, pageSize: 50 }),
    getMyApprovals({ relation: ApprovalRelation.All, pageSize: 50 }),
  ]);

  const awaitingError =
    !awaitingResult.isSuccess || !awaitingResult.data
      ? { title: "Unable to load your approvals", description: awaitingResult.message || "Please try again." }
      : undefined;

  const myRequestsError =
    !myRequestsResult.isSuccess || !myRequestsResult.data
      ? { title: "Unable to load your requests", description: myRequestsResult.message || "Please try again." }
      : undefined;

  // `relation=All` also includes requests where the current user is an approver at a future
  // step they haven't reached yet — narrow down to just "I created it" or "I decided a step on
  // it" (the two lists being merged here), and tag each row with which of those apply.
  const rolesById = new Map<string, ApprovalOwnerRole[]>();
  const myRequests = (myRequestsResult.data?.records ?? []).filter((request) => {
    const roles: ApprovalOwnerRole[] = [];
    if (request.requesterUserId === myId) roles.push("requester");
    if (request.steps.some((step) => step.approverUserId === myId && step.decidedAt)) roles.push("decided");
    if (roles.length > 0) rolesById.set(request.id, roles);
    return roles.length > 0;
  });

  const allResult = canViewAll ? await searchApprovals({ pageSize: 50 }) : null;
  const allError =
    allResult && (!allResult.isSuccess || !allResult.data)
      ? { title: "Unable to load approval requests", description: allResult.message || "Please try again." }
      : undefined;

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Approvals</h1>
        <p className="text-sm text-muted-foreground">
          Create requests, review what&apos;s waiting on you, and track requests you&apos;ve created
          or already decided.
        </p>
      </div>

      <Tabs defaultValue="awaiting">
        <TabsList>
          <TabsTrigger value="awaiting">Waiting on your decision</TabsTrigger>
          <TabsTrigger value="mine">My requests</TabsTrigger>
          {canViewAll && <TabsTrigger value="all">All requests</TabsTrigger>}
        </TabsList>

        <TabsContent value="awaiting">
          <MyApprovalsTable
            records={awaitingResult.data?.records ?? []}
            error={awaitingError}
            userNamesById={userNamesById}
          />
        </TabsContent>

        <TabsContent value="mine">
          <ApprovalHistoryTable
            records={myRequests}
            error={myRequestsError}
            userNamesById={userNamesById}
            rolesById={rolesById}
          />
        </TabsContent>

        {canViewAll && (
          <TabsContent value="all">
            <AllApprovalsTable
              records={allResult?.data?.records ?? []}
              error={allError}
              canCreate
              userNamesById={userNamesById}
            />
          </TabsContent>
        )}
      </Tabs>
    </div>
  );
}
