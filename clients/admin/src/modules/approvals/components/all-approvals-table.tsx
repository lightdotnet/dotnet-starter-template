"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { ListChecks, Plus } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import {
  DataTable,
  type DataTableAction,
  type DataTableColumn,
  type DataTableErrorState,
} from "@/components/shared/data-table";
import { CreateApprovalRequestDialog } from "@/modules/approvals/components/create-approval-request-dialog";
import { ApprovalStatus, type ApprovalRequestDto } from "@/modules/approvals/types/approval";

interface AllApprovalsTableProps {
  records: ApprovalRequestDto[];
  error?: DataTableErrorState;
  canCreate?: boolean;
}

const STATUS_VARIANT: Record<ApprovalStatus, "default" | "outline" | "destructive" | "secondary"> = {
  [ApprovalStatus.Pending]: "secondary",
  [ApprovalStatus.Approved]: "default",
  [ApprovalStatus.Rejected]: "destructive",
  [ApprovalStatus.Cancelled]: "outline",
};

/** Read-only, most-recent-50 view — lets you watch a chain's status/level advance across decisions without a full search/pagination UI (this is an admin verification view, not a primary workflow). */
export function AllApprovalsTable({ records, error, canCreate }: AllApprovalsTableProps) {
  const router = useRouter();
  const [createOpen, setCreateOpen] = useState(false);
  const [createDialogKey, setCreateDialogKey] = useState(0);

  const actions: DataTableAction[] | undefined = canCreate
    ? [
        {
          key: "create",
          label: "Create test request",
          icon: Plus,
          onClick: () => {
            setCreateDialogKey((key) => key + 1);
            setCreateOpen(true);
          },
        },
      ]
    : undefined;

  const columns: DataTableColumn<ApprovalRequestDto>[] = [
    {
      id: "title",
      header: "Title",
      hideable: false,
      cell: (request) => (
        <div className="flex flex-col">
          <span className="font-medium">{request.title}</span>
          <span className="text-xs text-muted-foreground">{request.requestType}</span>
        </div>
      ),
    },
    {
      id: "level",
      header: "Level",
      cell: (request) => `${request.currentLevel} / ${request.steps.length}`,
    },
    {
      id: "status",
      header: "Status",
      cell: (request) => <Badge variant={STATUS_VARIANT[request.status]}>{request.status}</Badge>,
    },
    {
      id: "created",
      header: "Requested",
      cell: (request) => new Date(request.created).toLocaleString(),
    },
  ];

  return (
    <>
      <DataTable
        columns={columns}
        data={records}
        rowKey={(request) => request.id}
        totalRecords={records.length}
        error={error}
        actions={actions}
        onRefresh={() => router.refresh()}
        emptyState={{
          icon: ListChecks,
          title: "No approval requests yet",
          description: "Create a test request to see it here.",
        }}
      />
      <CreateApprovalRequestDialog
        key={`create-${createDialogKey}`}
        open={createOpen}
        onOpenChange={setCreateOpen}
        onCreated={() => router.refresh()}
      />
    </>
  );
}
