import type { NavItem } from "@/types/nav";

/**
 * Drops nodes whose `permission` the caller lacks, and drops groups left with
 * no visible children — never renders a group heading with nothing under it.
 */
export function buildVisibleMenu(
  items: NavItem[],
  can: (permission: string) => boolean,
): NavItem[] {
  return items.reduce<NavItem[]>((visible, item) => {
    if (item.permission && !can(item.permission)) {
      return visible;
    }

    if (!item.children) {
      visible.push(item);
      return visible;
    }

    const children = buildVisibleMenu(item.children, can);

    if (children.length === 0) {
      return visible;
    }

    visible.push({ ...item, children });
    return visible;
  }, []);
}
