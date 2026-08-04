import { Settings, ShieldCheck, ShieldCog } from "lucide-react";
// Imported directly (not via each feature's barrel): the barrels also re-export
// server-only code (Server Components, session-backed API calls), and this file
// is reachable from the client-side Sidebar — pulling in the full barrel would
// drag that server-only chain into the client bundle.
import { DASHBOARD_NAV_ITEM } from "@/features/dashboard/constants/nav-item";
import { USERS_NAV_ITEM } from "@/features/users/constants/nav-item";
import { ROLES_NAV_ITEM } from "@/features/roles/constants/nav-item";
import { NOTIFICATIONS_NAV_ITEM } from "@/features/notifications/constants/nav-item";
import type { NavItem } from "@/types/nav";

export const NAV_ITEMS: NavItem[] = [
  DASHBOARD_NAV_ITEM,
  {
    label: "Administration",
    href: "/administration",
    icon: ShieldCog,
    children: [
      USERS_NAV_ITEM,
      ROLES_NAV_ITEM,
      NOTIFICATIONS_NAV_ITEM,
    ],
  },
  {
    label: "Settings",
    href: "/settings",
    icon: Settings,
  },
];
