"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { useGuardedAction } from "@/hooks/use-guarded-action";
import { decideApprovalAction } from "@/modules/approvals/api/decide-approval-action";
import type { ApprovalRequestDto } from "@/modules/approvals/types/approval";

interface DecideApprovalDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  request: ApprovalRequestDto | null;
  approved: boolean;
  onDecided: () => void;
}

export function DecideApprovalDialog({
  open,
  onOpenChange,
  request,
  approved,
  onDecided,
}: DecideApprovalDialogProps) {
  const [pending, run] = useGuardedAction();
  const [comment, setComment] = useState("");

  function handleOpenChange(nextOpen: boolean) {
    if (!nextOpen) setComment("");
    onOpenChange(nextOpen);
  }

  function handleConfirm() {
    if (!request) return;

    run(
      () => decideApprovalAction(request.id, approved, comment),
      approved ? "Request approved." : "Request rejected.",
      () => {
        handleOpenChange(false);
        onDecided();
      },
    );
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent onPointerDownOutside={(event) => event.preventDefault()}>
        <DialogHeader>
          <DialogTitle>{approved ? "Approve request" : "Reject request"}</DialogTitle>
          <DialogDescription>
            &quot;{request?.title ?? "This request"}&quot; — level {request?.currentLevel} of your
            approval chain.
          </DialogDescription>
        </DialogHeader>

        <div className="flex flex-col gap-1.5">
          <Label htmlFor="comment">Comment (optional)</Label>
          <Textarea
            id="comment"
            value={comment}
            onChange={(event) => setComment(event.target.value)}
            rows={3}
          />
        </div>

        <DialogFooter>
          <Button disabled={pending} onClick={() => handleOpenChange(false)} type="button" variant="outline">
            Cancel
          </Button>
          <Button
            disabled={pending}
            loading={pending}
            onClick={handleConfirm}
            type="button"
            variant={approved ? "default" : "destructive"}
          >
            {approved ? "Approve" : "Reject"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
