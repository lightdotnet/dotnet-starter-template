import type { LucideIcon } from "lucide-react";
import type { Button } from "@/components/ui/button";

export interface DataTableColumn<TData> {
  id: string;
  header: React.ReactNode;
  cell: (row: TData) => React.ReactNode;
  /** Applied to both the `th` and `td` for this column. */
  className?: string;
  /** Whether this column can be hidden via the columns menu. Defaults to `true`. */
  hideable?: boolean;
}

export interface DataTableAction {
  key: string;
  label: string;
  icon?: LucideIcon;
  onClick: () => void;
  variant?: React.ComponentProps<typeof Button>["variant"];
}

export interface DataTableEmptyState {
  icon?: LucideIcon;
  title: string;
  description?: string;
}

export interface DataTableErrorState {
  title: string;
  description?: string;
}
