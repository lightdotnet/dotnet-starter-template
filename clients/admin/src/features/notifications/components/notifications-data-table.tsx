"use client";

import { useState, useTransition } from "react";
import { usePathname, useRouter, useSearchParams } from "next/navigation";
import { Bell as BellIcon, Send } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { NativeSelect } from "@/components/ui/native-select";
import {
  DataTable,
  type DataTableAction,
  type DataTableColumn,
  type DataTableErrorState,
} from "@/components/shared/data-table";
import { SendNotificationDialog } from "@/features/notifications/components/send-notification-dialog";
import { UserSelect } from "@/features/notifications/components/user-select";
import { NotificationStatus, type NotificationDto } from "@/features/notifications/types/notification";
import { getDisplayName } from "@/lib/shared/user-display";
import type { UserDto } from "@/types/user";

interface NotificationsDataTableProps {
  records: NotificationDto[];
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  totalRecords: number;
  error?: DataTableErrorState;
  canSend?: boolean;
  users: UserDto[];
  toUserId?: string;
  status?: NotificationStatus;
}

const STATUS_FILTER_OPTIONS = [
  { value: NotificationStatus.None, label: "Unread" },
  { value: NotificationStatus.Read, label: "Read" },
  { value: NotificationStatus.Archived, label: "Archived" },
];

const STATUS_LABEL: Record<NotificationStatus, string> = {
  [NotificationStatus.None]: "Unread",
  [NotificationStatus.Read]: "Read",
  [NotificationStatus.Archived]: "Archived",
};

const STATUS_VARIANT: Record<NotificationStatus, "default" | "secondary" | "outline"> = {
  [NotificationStatus.None]: "default",
  [NotificationStatus.Read]: "secondary",
  [NotificationStatus.Archived]: "outline",
};

export function NotificationsDataTable({
  records,
  pageNumber,
  pageSize,
  totalPages,
  totalRecords,
  error,
  canSend,
  users,
  toUserId,
  status,
}: NotificationsDataTableProps) {
  const router = useRouter();
  const pathname = usePathname();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();
  const [sendOpen, setSendOpen] = useState(false);
  const [sendDialogKey, setSendDialogKey] = useState(0);

  const usersById = new Map(users.map((user) => [user.id, user]));

  function navigate(nextParams: Record<string, string | undefined>) {
    const params = new URLSearchParams(searchParams.toString());
    for (const [key, value] of Object.entries(nextParams)) {
      if (value) params.set(key, value);
      else params.delete(key);
    }

    startTransition(() => {
      router.push(`${pathname}?${params.toString()}`);
    });
  }

  function userLabel(userId: string) {
    const user = usersById.get(userId);
    return user ? getDisplayName(user) : userId;
  }

  const actions: DataTableAction[] | undefined = canSend
    ? [
        {
          key: "send",
          label: "Send notification",
          icon: Send,
          onClick: () => {
            setSendDialogKey((key) => key + 1);
            setSendOpen(true);
          },
        },
      ]
    : undefined;

  const columns: DataTableColumn<NotificationDto>[] = [
    {
      id: "to",
      header: "To",
      cell: (notification) => userLabel(notification.toUserId),
    },
    {
      id: "from",
      header: "From",
      cell: (notification) => notification.fromName ?? notification.fromUserId,
    },
    {
      id: "title",
      header: "Title",
      cell: (notification) => notification.title,
    },
    {
      id: "message",
      header: "Message",
      cell: (notification) => notification.message ?? "",
      className: "max-w-sm whitespace-normal break-words",
    },
    {
      id: "status",
      header: "Status",
      cell: (notification) => (
        <Badge variant={STATUS_VARIANT[notification.status]}>
          {STATUS_LABEL[notification.status]}
        </Badge>
      ),
    },
    {
      id: "created",
      header: "Created",
      cell: (notification) => new Date(notification.created).toLocaleString(),
    },
  ];

  return (
    <>
      <div className="flex flex-wrap items-center gap-2 pb-4">
        <NativeSelect
          className="w-40"
          aria-label="Filter by status"
          placeholder="All statuses"
          value={status ?? ""}
          onChange={(value) =>
            navigate({
              status: value === "" ? undefined : value,
              page: undefined,
            })
          }
          options={STATUS_FILTER_OPTIONS}
        />

        <UserSelect
          users={users}
          value={toUserId ?? ""}
          onValueChange={(value) =>
            navigate({
              toUserId: value === "" ? undefined : value,
              page: undefined,
            })
          }
          triggerClassName="w-56"
          ariaLabel="Filter by recipient"
          placeholder="Search recipients"
          onClear={() => navigate({ toUserId: undefined, page: undefined })}
        />
      </div>

      <DataTable
        columns={columns}
        data={records}
        rowKey={(notification) => notification.id}
        isLoading={isPending}
        actions={actions}
        onRefresh={() => startTransition(() => router.refresh())}
        pageNumber={pageNumber}
        pageSize={pageSize}
        totalPages={totalPages}
        totalRecords={totalRecords}
        onPageChange={(page) => navigate({ page: String(page) })}
        error={error}
        emptyState={{
          icon: BellIcon,
          title: "No notifications found",
          description: "Nothing has been sent yet.",
        }}
      />
      {canSend && (
        <SendNotificationDialog
          key={`send-${sendDialogKey}`}
          open={sendOpen}
          onOpenChange={setSendOpen}
          users={users}
          onSent={() => router.refresh()}
        />
      )}
    </>
  );
}
