"use client";

import { useState } from "react";
import {
  autoUpdate,
  flip,
  offset,
  shift,
  size,
  useClick,
  useDismiss,
  useFloating,
  useRole,
  type Placement,
} from "@floating-ui/react";

/** Hard cap on the floating panel's height, regardless of how much viewport space is available. */
const MAX_PANEL_HEIGHT = 320;

export interface UseFloatingPopoverOptions {
  open?: boolean;
  onOpenChange?: (open: boolean) => void;
  placement?: Placement;
  /** Constrains the floating panel to at least the trigger's width. Defaults to true. */
  matchTriggerWidth?: boolean;
  role?: "listbox" | "dialog" | "menu";
  /**
   * Whether clicking the reference element toggles it open/closed. Button
   * triggers (EntitySelect) want this; text-input triggers (SearchSelect,
   * AsyncSelect, MultiSelect) open via onFocus instead and must disable this
   * — otherwise the click that follows the opening focus event immediately
   * toggles the panel closed again. Defaults to true.
   */
  enableClick?: boolean;
  /** Caps the floating panel's height. Defaults to 320px. */
  maxPanelHeight?: number;
}

/**
 * Single source of truth for "a trigger + a positioned panel": wraps
 * useFloating's positioning middleware with the click/dismiss/role
 * interaction trio every select-family component and the command palette
 * needs. Component-specific behavior (keyboard nav, typeahead) composes on
 * top via useListbox.
 */
export function useFloatingPopover({
  open: controlledOpen,
  onOpenChange,
  placement = "bottom-start",
  matchTriggerWidth = true,
  role = "listbox",
  enableClick = true,
  maxPanelHeight = MAX_PANEL_HEIGHT,
}: UseFloatingPopoverOptions = {}) {
  const [uncontrolledOpen, setUncontrolledOpen] = useState(false);
  const open = controlledOpen ?? uncontrolledOpen;

  function setOpen(next: boolean) {
    onOpenChange?.(next);
    if (controlledOpen === undefined) setUncontrolledOpen(next);
  }

  const floating = useFloating({
    open,
    onOpenChange: setOpen,
    placement,
    whileElementsMounted: autoUpdate,
    middleware: [
      offset(4),
      flip({ padding: 8 }),
      shift({ padding: 8 }),
      size({
        padding: 8,
        apply({ availableHeight, elements, rects }) {
          Object.assign(elements.floating.style, {
            maxHeight: `${Math.min(availableHeight, maxPanelHeight)}px`,
            ...(matchTriggerWidth ? { minWidth: `${rects.reference.width}px` } : {}),
          });
        },
      }),
    ],
  });

  // Deliberately not merged into `useInteractions` here: the consuming
  // component also has list-navigation/typeahead interaction hooks (from
  // useListbox) that must be combined into a *single* useInteractions call
  // so getReferenceProps/getFloatingProps/getItemProps merge every
  // handler correctly. Returning the raw hook results lets the caller own
  // that one merge point.
  const click = useClick(floating.context, { enabled: enableClick });
  const dismiss = useDismiss(floating.context);
  const roleInteraction = useRole(floating.context, { role });

  return {
    open,
    setOpen,
    ...floating,
    click,
    dismiss,
    role: roleInteraction,
  };
}
