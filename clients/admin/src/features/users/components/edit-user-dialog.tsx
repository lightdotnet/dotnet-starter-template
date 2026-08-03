"use client";

import { useActionState, useEffect, useState } from "react";
import { Plus, X } from "lucide-react";
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
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Spinner } from "@/components/ui/spinner";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { notifySuccess } from "@/components/toast";
import {
  forcePasswordAction,
  type ForcePasswordFormState,
} from "@/features/users/api/force-password-action";
import { getUserDetailAction } from "@/features/users/api/get-user-detail-action";
import {
  updateUserAction,
  type UpdateUserFormState,
} from "@/features/users/api/update-user-action";
import type { RoleDto } from "@/features/roles/types/role";
import type { ClaimDto, UserDto } from "@/types/user";

const STATUS_OPTIONS = ["active", "locked"];
const AUTH_PROVIDER_OPTIONS = ["Local", "AD"];

const updateInitialState: UpdateUserFormState = {};
const passwordInitialState: ForcePasswordFormState = {};

interface ClaimRow {
  key: string;
  type: string;
  value: string;
}

let claimKeySeed = 0;
function nextClaimKey() {
  claimKeySeed += 1;
  return `claim-${claimKeySeed}`;
}

function toClaimRows(claims: ClaimDto[]): ClaimRow[] {
  return claims.map((claim) => ({
    key: nextClaimKey(),
    type: claim.type,
    value: claim.value,
  }));
}

interface EditUserDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: UserDto | null;
  roles: RoleDto[];
  onUpdated: () => void;
}

export function EditUserDialog({
  open,
  onOpenChange,
  user,
  roles,
  onUpdated,
}: EditUserDialogProps) {
  const [state, formAction, pending] = useActionState(
    updateUserAction,
    updateInitialState,
  );
  const [passwordState, passwordFormAction, passwordPending] = useActionState(
    forcePasswordAction,
    passwordInitialState,
  );

  const [loading, setLoading] = useState(true);
  const [loadError, setLoadError] = useState("");
  const [status, setStatus] = useState("active");
  const [authProvider, setAuthProvider] = useState("Local");
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [claimRows, setClaimRows] = useState<ClaimRow[]>([]);
  const [newPassword, setNewPassword] = useState("");

  // The users table only carries list-level fields — roles and claims aren't
  // populated there, so load the full record once the dialog opens.
  useEffect(() => {
    if (!user) return;

    let cancelled = false;

    (async () => {
      setLoading(true);
      setLoadError("");

      const result = await getUserDetailAction(user.id);
      if (cancelled) return;

      if (!result.data) {
        setLoadError(result.error || "Unable to load user details.");
        setLoading(false);
        return;
      }

      setStatus(result.data.status ?? "active");
      setAuthProvider(result.data.authProvider ?? "Local");
      setSelectedRoles(result.data.roles);
      setClaimRows(toClaimRows(result.data.claims));
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
    notifySuccess("User updated.");
    onOpenChange(false);
    // eslint-disable-next-line react-hooks/exhaustive-deps -- only react to a fresh success result
  }, [state.success]);

  useEffect(() => {
    if (!passwordState.success) return;
    notifySuccess("Password reset.");
    // eslint-disable-next-line react-hooks/set-state-in-effect -- resetting the field after a completed reset action, not deriving state from a prop
    setNewPassword("");
  }, [passwordState.success]);

  function toggleRole(roleName: string, checked: boolean) {
    setSelectedRoles((previous) =>
      checked
        ? [...previous, roleName]
        : previous.filter((name) => name !== roleName),
    );
  }

  function addClaimRow() {
    setClaimRows((previous) => [
      ...previous,
      { key: nextClaimKey(), type: "", value: "" },
    ]);
  }

  function removeClaimRow(key: string) {
    setClaimRows((previous) => previous.filter((row) => row.key !== key));
  }

  function updateClaimRow(key: string, field: "type" | "value", value: string) {
    setClaimRows((previous) =>
      previous.map((row) =>
        row.key === key ? { ...row, [field]: value } : row,
      ),
    );
  }

  if (!user) return null;

  const isLocalAccount = authProvider.toLowerCase() === "local";

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        className="sm:max-w-lg"
        onPointerDownOutside={(event) => event.preventDefault()}
      >
        <DialogHeader>
          <DialogTitle>Edit user</DialogTitle>
        </DialogHeader>

        {loading ? (
          <div className="flex items-center justify-center gap-2 py-10 text-sm text-muted-foreground">
            <Spinner />
            Loading user details...
          </div>
        ) : loadError ? (
          <Alert variant="destructive">
            <AlertDescription>{loadError}</AlertDescription>
          </Alert>
        ) : (
          <Tabs defaultValue="details">
            <TabsList>
              <TabsTrigger value="details">Details</TabsTrigger>
              <TabsTrigger value="password">Reset password</TabsTrigger>
            </TabsList>

            <TabsContent value="details">
              <form action={formAction} className="flex flex-col gap-4">
                <input type="hidden" name="id" value={user.id} />
                <input type="hidden" name="userName" value={user.userName} />

                {state.error && (
                  <Alert variant="destructive">
                    <AlertDescription>{state.error}</AlertDescription>
                  </Alert>
                )}

                <div className="grid grid-cols-2 gap-3">
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="firstName">First name</Label>
                    <Input
                      id="firstName"
                      name="firstName"
                      defaultValue={user.firstName ?? ""}
                    />
                  </div>
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="lastName">Last name</Label>
                    <Input
                      id="lastName"
                      name="lastName"
                      defaultValue={user.lastName ?? ""}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="email">Email</Label>
                    <Input
                      id="email"
                      name="email"
                      type="email"
                      defaultValue={user.email ?? ""}
                    />
                  </div>
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="phoneNumber">Phone number</Label>
                    <Input
                      id="phoneNumber"
                      name="phoneNumber"
                      defaultValue={user.phoneNumber ?? ""}
                    />
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-3">
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="status">Status</Label>
                    <Select
                      name="status"
                      value={status}
                      onValueChange={setStatus}
                    >
                      <SelectTrigger id="status" className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {STATUS_OPTIONS.map((option) => (
                          <SelectItem key={option} value={option}>
                            {option}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                  <div className="flex flex-col gap-1.5">
                    <Label htmlFor="authProvider">Auth provider</Label>
                    <Select
                      name="authProvider"
                      value={authProvider}
                      onValueChange={setAuthProvider}
                    >
                      <SelectTrigger id="authProvider" className="w-full">
                        <SelectValue />
                      </SelectTrigger>
                      <SelectContent>
                        {AUTH_PROVIDER_OPTIONS.map((option) => (
                          <SelectItem key={option} value={option}>
                            {option}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="flex flex-col gap-1.5">
                  <Label>Roles</Label>
                  <div className="flex max-h-32 flex-col gap-2 overflow-y-auto rounded-md border border-border p-3">
                    {roles.length === 0 ? (
                      <p className="text-sm text-muted-foreground">
                        No roles available.
                      </p>
                    ) : (
                      roles.map((role) => (
                        <label
                          key={role.id}
                          className="flex items-center gap-2 text-sm"
                        >
                          <Checkbox
                            name="roles"
                            value={role.name}
                            checked={selectedRoles.includes(role.name)}
                            onCheckedChange={(checked) =>
                              toggleRole(role.name, checked === true)
                            }
                          />
                          {role.name}
                        </label>
                      ))
                    )}
                  </div>
                </div>

                <div className="flex flex-col gap-1.5">
                  <div className="flex items-center justify-between">
                    <Label>Claims</Label>
                    <Button
                      type="button"
                      variant="outline"
                      size="sm"
                      onClick={addClaimRow}
                    >
                      <Plus />
                      Add claim
                    </Button>
                  </div>
                  {claimRows.length === 0 ? (
                    <p className="text-sm text-muted-foreground">
                      No claims added.
                    </p>
                  ) : (
                    <div className="flex flex-col gap-2">
                      {claimRows.map((row) => (
                        <div key={row.key} className="flex items-center gap-2">
                          <Input
                            name="claimType"
                            placeholder="Type"
                            value={row.type}
                            onChange={(event) =>
                              updateClaimRow(
                                row.key,
                                "type",
                                event.target.value,
                              )
                            }
                          />
                          <Input
                            name="claimValue"
                            placeholder="Value"
                            value={row.value}
                            onChange={(event) =>
                              updateClaimRow(
                                row.key,
                                "value",
                                event.target.value,
                              )
                            }
                          />
                          <Button
                            type="button"
                            variant="ghost"
                            size="icon"
                            aria-label="Remove claim"
                            onClick={() => removeClaimRow(row.key)}
                          >
                            <X />
                          </Button>
                        </div>
                      ))}
                    </div>
                  )}
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
            </TabsContent>

            <TabsContent value="password">
              <form action={passwordFormAction} className="flex flex-col gap-4">
                <input type="hidden" name="id" value={user.id} />

                {passwordState.error && (
                  <Alert variant="destructive">
                    <AlertDescription>{passwordState.error}</AlertDescription>
                  </Alert>
                )}

                <div className="flex flex-col gap-1.5">
                  <Label htmlFor="newPassword">New password</Label>
                  <Input
                    id="newPassword"
                    name="newPassword"
                    type="password"
                    autoComplete="new-password"
                    value={newPassword}
                    onChange={(event) => setNewPassword(event.target.value)}
                    required
                  />
                  {!isLocalAccount && (
                    <p className="text-xs text-muted-foreground">
                      Password reset is only available for local accounts.
                    </p>
                  )}
                </div>

                <DialogFooter>
                  <Button
                    type="submit"
                    loading={passwordPending}
                    disabled={!isLocalAccount}
                  >
                    Reset password
                  </Button>
                </DialogFooter>
              </form>
            </TabsContent>
          </Tabs>
        )}
      </DialogContent>
    </Dialog>
  );
}
