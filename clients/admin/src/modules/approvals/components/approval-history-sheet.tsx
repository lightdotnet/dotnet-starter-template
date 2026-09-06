"use client";

import { Badge } from "@/components/ui/badge";
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from "@/components/ui/sheet";
import { cn } from "@/lib/shared/utils";
import { ApprovalStatus, ApprovalStepStatus, type ApprovalRequestDto } from "@/modules/approvals/types/approval";

interface ApprovalHistorySheetProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  request: ApprovalRequestDto | null;
  /** userId -> display name, resolved once by the page. */
  userNamesById: Map<string, string>;
}

const STEP_STATUS_VARIANT: Record<ApprovalStepStatus, "default" | "outline" | "destructive" | "secondary"> = {
  [ApprovalStepStatus.Pending]: "secondary",
  [ApprovalStepStatus.Approved]: "default",
  [ApprovalStepStatus.Rejected]: "destructive",
  [ApprovalStepStatus.Skipped]: "outline",
};

/** Timeline dot fill per step outcome — awaiting decision gets its own pulsing ring
 * rather than reusing the "pending" (not-yet-reached) styling, so the current
 * step in the chain is visually distinct at a glance. */
function dotClasses(status: ApprovalStepStatus, isAwaiting: boolean) {
  if (isAwaiting) return "border-amber-500 bg-background animate-pulse";
  switch (status) {
    case ApprovalStepStatus.Approved:
      return "border-green-600 bg-green-600 dark:border-green-500 dark:bg-green-500";
    case ApprovalStepStatus.Rejected:
      return "border-destructive bg-destructive";
    case ApprovalStepStatus.Skipped:
      return "border-border bg-muted-foreground/30";
    default:
      return "border-border bg-background";
  }
}

/** Read-only side panel showing a request's full approver chain — each level's
 * status, who decided it, when, and their comment/reason — so a viewer can see
 * exactly where a request currently sits without opening the decide dialog. */
export function ApprovalHistorySheet({ open, onOpenChange, request, userNamesById }: ApprovalHistorySheetProps) {
  const steps = [...(request?.steps ?? [])].sort((a, b) => a.level - b.level);
  const requesterName = request && (userNamesById.get(request.requesterUserId) ?? request.requesterUserId);

  return (
    <Sheet open={open} onOpenChange={onOpenChange}>
      <SheetContent className="w-full sm:max-w-md">
        <SheetHeader>
          <SheetTitle>{request?.title}</SheetTitle>
        </SheetHeader>

        <div className="flex flex-col gap-4 overflow-y-auto px-4 pb-4">
          {requesterName && <p className="text-sm text-muted-foreground">Requester: {requesterName}</p>}

          {request?.content && (
            <p className="whitespace-pre-wrap break-words rounded-md border bg-muted/50 p-3 text-sm">
              {request.content}
            </p>
          )}

          <div className="flex flex-col">
            {steps.map((step, index) => {
              const isAwaiting =
                request?.status === ApprovalStatus.Pending &&
                step.level === request.currentLevel &&
                step.status === ApprovalStepStatus.Pending;
              const isLast = index === steps.length - 1;

              return (
                <div key={step.id} className="flex gap-3">
                  <div className="flex flex-col items-center">
                    <span
                      className={cn("mt-1.5 size-3 shrink-0 rounded-full border-2", dotClasses(step.status, isAwaiting))}
                    />
                    {!isLast && <span className="w-px flex-1 bg-border" />}
                  </div>

                  <div className="flex flex-col gap-1 pb-5">
                    <div className="flex items-center gap-2">
                      <span className="text-sm font-medium">Level {step.level}</span>
                      <Badge variant={STEP_STATUS_VARIANT[step.status]}>
                        {isAwaiting ? "Awaiting decision" : step.status}
                      </Badge>
                    </div>
                    <span className="text-xs text-muted-foreground">
                      Approver: {userNamesById.get(step.approverUserId) ?? step.approverEmployeeId}
                    </span>
                    {step.decidedAt && (
                      <span className="text-xs text-muted-foreground">
                        Decided: {new Date(step.decidedAt).toLocaleString()}
                      </span>
                    )}
                    {step.comment && (
                      <p className="mt-1 whitespace-pre-wrap break-words rounded-md border bg-muted/50 p-2 text-sm">
                        {step.comment}
                      </p>
                    )}
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      </SheetContent>
    </Sheet>
  );
}
