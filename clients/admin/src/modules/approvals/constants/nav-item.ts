import { CheckCheck } from "lucide-react";
import type { NavItem } from "@/types/nav";

/** No `permission` gate: every authenticated user can have approvals routed
 * to them, so the nav item — like the page itself — only requires a session. */
export const APPROVALS_NAV_ITEM: NavItem = {
  label: "Approvals",
  href: "/approvals",
  icon: CheckCheck,
};
