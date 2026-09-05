"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Check, ClipboardCheck, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
  DataTable,
  type DataTableColumn,
  type DataTableErrorState,
} from "@/components/shared/data-table";
import { DecideApprovalDialog } from "@/modules/approvals/components/decide-approval-dialog";
import type { ApprovalRequestDto } from "@/modules/approvals/types/approval";

interface MyApprovalsTableProps {
  records: ApprovalRequestDto[];
  error?: DataTableErrorState;
}

export function MyApprovalsTable({ records, error }: MyApprovalsTableProps) {
  const router = useRouter();
  const [selected, setSelected] = useState<ApprovalRequestDto | null>(null);
  const [approved, setApproved] = useState(true);
  const [dialogOpen, setDialogOpen] = useState(false);

  function openDecision(request: ApprovalRequestDto, willApprove: boolean) {
    setSelected(request);
    setApproved(willApprove);
    setDialogOpen(true);
  }

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
      cell: (request) => `#${request.currentLevel}`,
    },
    {
      id: "created",
      header: "Requested",
      cell: (request) => new Date(request.created).toLocaleString(),
    },
    {
      id: "actions",
      header: "",
      hideable: false,
      cell: (request) => (
        <div className="flex justify-end gap-2">
          <Button size="sm" variant="outline" onClick={() => openDecision(request, false)}>
            <X className="size-4" />
            Reject
          </Button>
          <Button size="sm" onClick={() => openDecision(request, true)}>
            <Check className="size-4" />
            Approve
          </Button>
        </div>
      ),
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
        onRefresh={() => router.refresh()}
        emptyState={{
          icon: ClipboardCheck,
          title: "Nothing waiting on you",
          description: "Requests routed to you for approval will show up here.",
        }}
      />
      <DecideApprovalDialog
        open={dialogOpen}
        onOpenChange={setDialogOpen}
        request={selected}
        approved={approved}
        onDecided={() => router.refresh()}
      />
    </>
  );
}
