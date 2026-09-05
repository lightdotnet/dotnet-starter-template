"use client";

import { useState } from "react";
import { Plus, X } from "lucide-react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useGuardedAction } from "@/hooks/use-guarded-action";
import { ApproverSelect } from "@/modules/approvals/components/approver-select";
import { createApprovalRequestAction } from "@/modules/approvals/api/create-approval-request-action";
import { getDisplayName } from "@/lib/shared/user-display";
import type { UserDto } from "@/modules/identity/users";

interface ApproverRow {
  key: number;
  user: UserDto | null;
}

let nextRowKey = 1;
function emptyRow(): ApproverRow {
  return { key: nextRowKey++, user: null };
}

interface CreateApprovalRequestDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onCreated: () => void;
}

/**
 * Test harness for the generic Approval engine: creates a request as the
 * current user with a hand-picked, ordered chain of approvers — level N is
 * simply row N. Exercises the same multi-level "advance on approve" logic a
 * real request type (e.g. Leave) will drive automatically once it exists.
 */
export function CreateApprovalRequestDialog({
  open,
  onOpenChange,
  onCreated,
}: CreateApprovalRequestDialogProps) {
  const [pending, run] = useGuardedAction();
  const [requestType, setRequestType] = useState("Test");
  const [title, setTitle] = useState("");
  const [rows, setRows] = useState<ApproverRow[]>([emptyRow()]);
  const [error, setError] = useState<string | undefined>();

  function reset() {
    setRequestType("Test");
    setTitle("");
    setRows([emptyRow()]);
    setError(undefined);
  }

  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen) reset();
    onOpenChange(nextOpen);
  }

  function addRow() {
    setRows((previous) => [...previous, emptyRow()]);
  }

  function removeRow(key: number) {
    setRows((previous) => previous.filter((row) => row.key !== key));
  }

  function setRowUser(key: number, user: UserDto) {
    setRows((previous) => previous.map((row) => (row.key === key ? { ...row, user } : row)));
  }

  function handleSubmit() {
    setError(undefined);

    if (!title.trim()) {
      setError("Title is required.");
      return;
    }

    if (rows.some((row) => !row.user)) {
      setError("Every approver level needs a selected user.");
      return;
    }

    run(
      () =>
        createApprovalRequestAction({
          requestType,
          title,
          approverUserIds: rows.map((row) => row.user!.id),
        }),
      "Approval request created.",
      () => {
        onCreated();
        handleOpenChange(false);
      },
    );
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent
        className="sm:max-w-lg"
        onPointerDownOutside={(event) => event.preventDefault()}
      >
        <DialogHeader>
          <DialogTitle>Create test approval request</DialogTitle>
        </DialogHeader>

        <div className="flex flex-col gap-4">
          {error && (
            <Alert variant="destructive">
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          )}

          <div className="grid grid-cols-2 gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="requestType">Request type</Label>
              <Input
                id="requestType"
                value={requestType}
                onChange={(event) => setRequestType(event.target.value)}
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="title">Title</Label>
              <Input id="title" value={title} onChange={(event) => setTitle(event.target.value)} required />
            </div>
          </div>

          <div className="flex flex-col gap-2">
            <Label>Approver chain</Label>
            {rows.map((row, index) => (
              <div key={row.key} className="flex items-center gap-2">
                <span className="w-14 shrink-0 text-sm text-muted-foreground">Level {index + 1}</span>
                <div className="flex-1">
                  <ApproverSelect
                    value={row.user?.id ?? ""}
                    onValueChange={(user) => setRowUser(row.key, user)}
                  />
                </div>
                <Button
                  type="button"
                  variant="ghost"
                  size="icon"
                  disabled={rows.length === 1}
                  onClick={() => removeRow(row.key)}
                  aria-label="Remove level"
                >
                  <X className="size-4" />
                </Button>
              </div>
            ))}
            <Button type="button" variant="outline" size="sm" className="self-start" onClick={addRow}>
              <Plus className="size-4" />
              Add level
            </Button>
            {rows.some((row) => row.user) && (
              <p className="text-xs text-muted-foreground">
                Chain: {rows.filter((row) => row.user).map((row) => getDisplayName(row.user!)).join(" → ")}
              </p>
            )}
          </div>
        </div>

        <DialogFooter>
          <DialogClose asChild>
            <Button type="button" variant="outline">
              Cancel
            </Button>
          </DialogClose>
          <Button type="button" loading={pending} onClick={handleSubmit}>
            Create
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
