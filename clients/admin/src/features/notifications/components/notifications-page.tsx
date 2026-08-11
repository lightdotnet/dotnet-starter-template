import { redirect } from "next/navigation";
import { ShieldOff } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import {
  Empty,
  EmptyDescription,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { resolveSession } from "@/features/user-profile";
import { getNotifications } from "@/features/notifications/api/notifications.api";
import { NotificationsDataTable } from "@/features/notifications/components/notifications-data-table";
import { hasPermission } from "@/lib/server/authorization";
import { NOTIFICATIONS_PERMISSIONS } from "@/features/notifications/constants/permissions";
import { NotificationStatus } from "@/features/notifications/types/notification";

const PAGE_SIZE = 10;

interface NotificationsPageProps {
  searchParams: Promise<{ page?: string; toUserId?: string; status?: string }>;
}

function parseStatusFilter(value: string | undefined): NotificationStatus | undefined {
  return value && Object.values(NotificationStatus).includes(value as NotificationStatus)
    ? (value as NotificationStatus)
    : undefined;
}

export async function NotificationsPage({ searchParams }: NotificationsPageProps) {
  const session = await resolveSession();
  if (!session) {
    redirect("/login");
  }

  if (!hasPermission(session, session.profile?.userName, NOTIFICATIONS_PERMISSIONS.Read)) {
    return (
      <Empty>
        <EmptyMedia variant="icon">
          <ShieldOff />
        </EmptyMedia>
        <EmptyTitle>Access denied</EmptyTitle>
        <EmptyDescription>
          You don&apos;t have the {NOTIFICATIONS_PERMISSIONS.Read} permission.
        </EmptyDescription>
      </Empty>
    );
  }

  const canSend = hasPermission(
    session,
    session.profile?.userName,
    NOTIFICATIONS_PERMISSIONS.Send,
  );

  const { page, toUserId, status } = await searchParams;
  const pageNumber = Math.max(Number(page) || 1, 1);
  const statusFilter = parseStatusFilter(status);

  const result = await getNotifications({
    pageNumber,
    pageSize: PAGE_SIZE,
    toUserId,
    status: statusFilter,
  });

  const error =
    !result.isSuccess || !result.data
      ? {
          title: "Unable to load notifications",
          description: result.message || "Please try again.",
        }
      : undefined;
  const paged = result.data;

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Notifications</h1>
        <p className="text-sm text-muted-foreground">
          Review notifications sent across the workspace.
        </p>
      </div>

      <Card>
        <CardContent>
          <NotificationsDataTable
            records={paged?.records ?? []}
            pageNumber={paged?.pageNumber ?? pageNumber}
            pageSize={paged?.pageSize ?? PAGE_SIZE}
            totalPages={paged?.totalPages ?? 1}
            totalRecords={paged?.totalRecords ?? 0}
            error={error}
            canSend={canSend}
            toUserId={toUserId}
            status={statusFilter}
          />
        </CardContent>
      </Card>
    </div>
  );
}
