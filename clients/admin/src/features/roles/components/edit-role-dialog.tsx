"use client";

import { useActionState, useEffect, useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogClose,
  DialogContent,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Spinner } from "@/components/ui/spinner";
import { notifySuccess } from "@/components/toast";
import { getRoleDetailAction } from "@/features/roles/api/get-role-detail-action";
import {
  updateRoleAction,
  type UpdateRoleFormState,
} from "@/features/roles/api/update-role-action";
import type { PermissionDefinition } from "@/features/roles/types/permission-definition";
import type { RoleDto } from "@/features/roles/types/role";

const updateInitialState: UpdateRoleFormState = {};
const PERMISSION_CLAIM_TYPE = "permission";

function groupPermissions(
  permissions: PermissionDefinition[],
): [string, PermissionDefinition[]][] {
  const groups = new Map<string, PermissionDefinition[]>();

  for (const permission of permissions) {
    const group = permission.parent || "other";
    const list = groups.get(group) ?? [];
    list.push(permission);
    groups.set(group, list);
  }

  return Array.from(groups.entries());
}

interface EditRoleDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  role: RoleDto | null;
  permissions: PermissionDefinition[];
  onUpdated: () => void;
}

export function EditRoleDialog({
  open,
  onOpenChange,
  role,
  permissions,
  onUpdated,
}: EditRoleDialogProps) {
  const [state, formAction, pending] = useActionState(
    updateRoleAction,
    updateInitialState,
  );

  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [detail, setDetail] = useState<RoleDto | null>(null);
  const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);

  // The roles list only carries name/description — claims aren't populated
  // there, so load the full record once the dialog opens.
  useEffect(() => {
    if (!role) return;

    let cancelled = false;

    (async () => {
      setLoading(true);
      setLoadError("");

      const result = await getRoleDetailAction(role.id);
      if (cancelled) return;

      if (!result.data) {
        setLoadError(result.error || "Unable to load role details.");
        setLoading(false);
        return;
      }

      setDetail(result.data);
      setSelectedPermissions(
        result.data.claims
          .filter((claim) => claim.type === PERMISSION_CLAIM_TYPE)
          .map((claim) => claim.value),
      );
      setLoading(false);
    })();

    return () => {
      cancelled = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps -- fetch once per mount; the dialog remounts fresh (via `key`) on every open
  }, []);

  useEffect(() => {
    if (!state.success) return;
    onUpdated();
    notifySuccess("Role updated.");
    onOpenChange(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps -- only react to a fresh success result
  }, [state.success]);

  function togglePermission(name: string, checked: boolean) {
    setSelectedPermissions((previous) =>
      checked
        ? [...previous, name]
        : previous.filter((value) => value !== name),
    );
  }

  if (!role) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        className="sm:max-w-lg"
        onPointerDownOutside={(event) => event.preventDefault()}
      >
        <DialogHeader>
          <DialogTitle>Edit role</DialogTitle>
        </DialogHeader>

        {loading ? (
          <div className="flex items-center justify-center gap-2 py-10 text-sm text-muted-foreground">
            <Spinner />
            Loading role details...
          </div>
        ) : loadError ? (
          <Alert variant="destructive">
            <AlertDescription>{loadError}</AlertDescription>
          </Alert>
        ) : (
          detail && (
            <form action={formAction} className="flex flex-col gap-4">
              <input type="hidden" name="id" value={detail.id} />

              {state.error && (
                <Alert variant="destructive">
                  <AlertDescription>{state.error}</AlertDescription>
                </Alert>
              )}

              <div className="flex flex-col gap-1.5">
                <Label htmlFor="name">Name</Label>
                <Input
                  id="name"
                  name="name"
                  defaultValue={detail.name}
                  required
                />
              </div>
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="description">Description</Label>
                <Input
                  id="description"
                  name="description"
                  defaultValue={detail.description ?? ""}
                />
              </div>

              <div className="flex flex-col gap-1.5">
                <Label>Permissions</Label>
                <div className="flex max-h-56 flex-col gap-3 overflow-y-auto rounded-md border border-border p-3">
                  {permissions.length === 0 ? (
                    <p className="text-sm text-muted-foreground">
                      No permissions available.
                    </p>
                  ) : (
                    groupPermissions(permissions).map(([group, items]) => (
                      <div key={group} className="flex flex-col gap-1.5">
                        <p className="text-xs font-medium uppercase text-muted-foreground">
                          {group}
                        </p>
                        {items.map((permission) => (
                          <label
                            key={permission.name}
                            className="flex items-center gap-2 text-sm"
                          >
                            <Checkbox
                              name="permissions"
                              value={permission.name}
                              checked={selectedPermissions.includes(
                                permission.name,
                              )}
                              onCheckedChange={(checked) =>
                                togglePermission(
                                  permission.name,
                                  checked === true,
                                )
                              }
                            />
                            {permission.displayName}
                          </label>
                        ))}
                      </div>
                    ))
                  )}
                </div>
              </div>

              <DialogFooter>
                <DialogClose asChild>
                  <Button type="button" variant="outline">
                    Cancel
                  </Button>
                </DialogClose>
                <Button type="submit" loading={pending}>
                  Save
                </Button>
              </DialogFooter>
            </form>
          )
        )}
      </DialogContent>
    </Dialog>
  );
}
