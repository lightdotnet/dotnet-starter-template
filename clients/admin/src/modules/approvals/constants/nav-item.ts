import { CheckCheck } from "lucide-react";
import { APPROVALS_PERMISSIONS } from "./permissions";
import type { NavItem } from "@/types/nav";

export const APPROVALS_NAV_ITEM: NavItem = {
  label: "Approvals",
  href: "/approvals",
  icon: CheckCheck,
  permission: APPROVALS_PERMISSIONS.View,
};
