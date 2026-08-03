import type { LucideIcon } from "lucide-react";

export interface NavItem {
  label: string;
  href: string;
  icon?: LucideIcon;
  /** Gates visibility of this item (and, if it has children, the recursion into them). */
  permission?: string;
  children?: NavItem[];
}
