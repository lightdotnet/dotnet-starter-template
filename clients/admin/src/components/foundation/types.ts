import type { ReactNode } from "react";

export interface SelectOption<TValue> {
  value: TValue;
  label: string;
  description?: string;
  disabled?: boolean;
}

export interface SelectOptionState {
  active: boolean;
  selected: boolean;
}

/** Shared "render option" slot contract used by every select-family component. */
export type RenderOption<TValue> = (
  option: SelectOption<TValue>,
  state: SelectOptionState,
) => ReactNode;
