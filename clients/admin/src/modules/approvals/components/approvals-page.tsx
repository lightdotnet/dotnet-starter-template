import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { getMyPendingApprovals, searchApprovals } from "@/modules/approvals/api/approvals.api";
import { AllApprovalsTable } from "@/modules/approvals/components/all-approvals-table";
import { MyApprovalsTable } from "@/modules/approvals/components/my-approvals-table";
import { hasPermission } from "@/lib/server/authorization";
import { requirePermission } from "@/lib/server/require-permission";
import { APPROVALS_PERMISSIONS } from "@/modules/approvals/constants/permissions";

export async function ApprovalsPage() {
  const { session, denied } = await requirePermission(APPROVALS_PERMISSIONS.View);
  if (denied) return denied;

  const canViewAll = hasPermission(session, APPROVALS_PERMISSIONS.ViewAll);

  const mineResult = await getMyPendingApprovals({ pageSize: 50 });
  const mineError =
    !mineResult.isSuccess || !mineResult.data
      ? { title: "Unable to load your approvals", description: mineResult.message || "Please try again." }
      : undefined;

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
          Review requests routed to you, and — for testing — create ad-hoc requests against the
          generic approval engine with a multi-level approver chain.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Waiting on your decision</CardTitle>
        </CardHeader>
        <CardContent>
          <MyApprovalsTable records={mineResult.data?.records ?? []} error={mineError} />
        </CardContent>
      </Card>

      {canViewAll && (
        <Card>
          <CardHeader>
            <CardTitle>All approval requests</CardTitle>
          </CardHeader>
          <CardContent>
            <AllApprovalsTable records={allResult?.data?.records ?? []} error={allError} canCreate />
          </CardContent>
        </Card>
      )}
    </div>
  );
}
