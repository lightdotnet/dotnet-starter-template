import {
  LayoutDashboard,
  Users,
  ShieldCheck,
  KeyRound,
  Settings,
} from "lucide-react";
import type { NavItem } from "@/types/nav";

export const NAV_ITEMS: NavItem[] = [
  {
    label: "Dashboard",
    href: "/",
    icon: LayoutDashboard,
  },
  {
    label: "Identity",
    href: "/identity",
    icon: ShieldCheck,
    children: [
      { label: "Users", href: "/identity/users", icon: Users },
      { label: "Roles", href: "/identity/roles", icon: KeyRound },
    ],
  },
  {
    label: "Settings",
    href: "/settings",
    icon: Settings,
  },
];
