"use client";

import { useCallback, useEffect, useRef, useState } from "react";
import {
  HttpTransportType,
  HubConnectionBuilder,
  type HubConnection,
} from "@microsoft/signalr";
import { getMyNotificationsAction } from "@/features/notifications/api/get-my-notifications-action";
import { getUnreadCountAction } from "@/features/notifications/api/get-unread-count-action";
import { markNotificationReadAction } from "@/features/notifications/api/mark-notification-read-action";
import { getSignalRTokenAction } from "@/features/notifications/api/get-signalr-token-action";
import {
  NotificationStatus,
  type NotificationDto,
} from "@/features/notifications/types/notification";

const SIGNALR_HUB_URL = "/api/signalr-hub";

export function useNotifications() {
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);
  const [unreadCount, setUnreadCount] = useState(0);
  const [loading, setLoading] = useState(true);
  const connectionRef = useRef<HubConnection | null>(null);

  const refresh = useCallback(async () => {
    const [listResult, count] = await Promise.all([
      getMyNotificationsAction(),
      getUnreadCountAction(),
    ]);
    if (listResult.data) setNotifications(listResult.data.records);
    setUnreadCount(count);
  }, []);

  const markAsRead = useCallback(async (id: string) => {
    const success = await markNotificationReadAction(id);
    if (!success) return;

    setNotifications((previous) =>
      previous.map((notification) =>
        notification.id === id
          ? { ...notification, status: NotificationStatus.Read }
          : notification,
      ),
    );
    setUnreadCount(await getUnreadCountAction());
  }, []);

  useEffect(() => {
    let cancelled = false;

    void (async () => {
      await refresh();
      if (cancelled) return;
      setLoading(false);

      const tokenState = await getSignalRTokenAction();
      if (cancelled || !tokenState) return;

      const connection = new HubConnectionBuilder()
        .withUrl(SIGNALR_HUB_URL, {
          accessTokenFactory: () => tokenState.accessToken,
          transport: HttpTransportType.WebSockets,
        })
        .withAutomaticReconnect()
        .build();

      // Payload is the raw `SystemMessage` pushed by the backend (no id/status/created),
      // so re-fetch instead of merging a partial one.
      connection.on("SystemMessage", () => void refresh());

      connectionRef.current = connection;
      await connection.start();
    })();

    return () => {
      cancelled = true;
      void connectionRef.current?.stop();
      connectionRef.current = null;
    };
  }, [refresh]);

  return { notifications, unreadCount, loading, markAsRead };
}
