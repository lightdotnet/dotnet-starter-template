import { cn } from "@/lib/shared/utils";
import { NotificationStatus, type NotificationDto } from "@/modules/notifications/types/notification";

interface NotificationListProps {
  notifications: NotificationDto[];
  selectedId: string | null;
  onSelect: (notification: NotificationDto) => void;
}

export function NotificationList({
  notifications,
  selectedId,
  onSelect,
}: NotificationListProps) {
  return (
    <ul className="flex flex-col divide-y divide-border overflow-y-auto md:h-full">
      {notifications.map((notification) => (
        <li key={notification.id}>
          <button
            type="button"
            onClick={() => onSelect(notification)}
            className={cn(
              "flex w-full flex-col gap-1 px-4 py-3 text-left transition-colors hover:bg-muted/50",
              selectedId === notification.id && "bg-muted",
            )}
          >
            <div className="flex items-center gap-2">
              {notification.status === NotificationStatus.None && (
                <span className="size-1.5 shrink-0 rounded-full bg-primary" aria-hidden />
              )}
              <span
                className={cn(
                  "truncate text-sm text-foreground",
                  notification.status === NotificationStatus.None && "font-semibold",
                )}
              >
                {notification.title}
              </span>
            </div>
            {notification.fromName && (
              <span className="truncate text-xs text-muted-foreground">
                From {notification.fromName}
              </span>
            )}
            <span className="text-[0.65rem] text-muted-foreground">
              {new Date(notification.created).toLocaleString()}
            </span>
          </button>
        </li>
      ))}
    </ul>
  );
}
