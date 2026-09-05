"use client";

import { useActionState, useState } from "react";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { useActionSuccessToast } from "@/hooks/use-action-success-toast";
import { useGuardedAction } from "@/hooks/use-guarded-action";
import {
  createEmployeeLoginAction,
  type CreateEmployeeLoginFormState,
} from "@/features/employees/api/create-employee-login-action";
import { linkEmployeeLoginAction } from "@/features/employees/api/link-employee-login-action";
import { unlinkEmployeeLoginAction } from "@/features/employees/api/unlink-employee-login-action";
import { UserSelect } from "@/features/employees/components/user-select";

const initialCreateState: CreateEmployeeLoginFormState = {};

interface EmployeeLoginTabProps {
  employeeId: string;
  userId?: string | null;
  onChanged: () => void;
}

export function EmployeeLoginTab({ employeeId, userId, onChanged }: EmployeeLoginTabProps) {
  const [unlinking, runUnlink] = useGuardedAction();
  const [linking, runLink] = useGuardedAction();
  const [selectedUserId, setSelectedUserId] = useState("");
  const [selectedUserLabel, setSelectedUserLabel] = useState("");

  const boundCreateAction = createEmployeeLoginAction.bind(null, employeeId);
  const [createState, createFormAction, createPending] = useActionState(
    boundCreateAction,
    initialCreateState,
  );

  useActionSuccessToast(createState, "Login created and linked.", onChanged);

  function handleUnlink() {
    runUnlink(() => unlinkEmployeeLoginAction(employeeId), "Login unlinked.", onChanged);
  }

  function handleLink() {
    if (!selectedUserId) return;
    runLink(() => linkEmployeeLoginAction(employeeId, selectedUserId), "Login linked.", onChanged);
  }

  if (userId) {
    return (
      <div className="flex flex-col gap-3">
        <p className="text-sm">
          This employee has a login account linked (user ID <code>{userId}</code>).
        </p>
        <div>
          <Button variant="outline" loading={unlinking} onClick={handleUnlink}>
            Unlink login
          </Button>
        </div>
        <p className="text-xs text-muted-foreground">
          Unlinking only removes the association — the Identity user account itself is not
          deleted.
        </p>
      </div>
    );
  }

  return (
    <div className="flex flex-col gap-6">
      <div className="flex flex-col gap-3">
        <p className="text-sm font-medium">Create a new login</p>
        <form action={createFormAction} className="flex flex-col gap-3" autoComplete="off">
          {createState.error && (
            <Alert variant="destructive">
              <AlertDescription>{createState.error}</AlertDescription>
            </Alert>
          )}
          <div className="grid grid-cols-2 gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="userName">Username</Label>
              <Input id="userName" name="userName" autoComplete="off" required />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="password">Password</Label>
              <Input
                id="password"
                name="password"
                type="password"
                autoComplete="new-password"
                required
              />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="loginEmail">Email</Label>
              <Input id="loginEmail" name="email" type="email" />
            </div>
            <div className="flex flex-col gap-1.5">
              <Label htmlFor="loginPhone">Phone number</Label>
              <Input id="loginPhone" name="phoneNumber" />
            </div>
          </div>
          <div>
            <Button type="submit" loading={createPending}>
              Create login
            </Button>
          </div>
        </form>
      </div>

      <Separator />

      <div className="flex flex-col gap-3">
        <p className="text-sm font-medium">Link an existing user</p>
        <UserSelect
          value={selectedUserId}
          onValueChange={(user) => {
            setSelectedUserId(user.id);
            setSelectedUserLabel(user.userName);
          }}
          placeholder="Search for a user..."
        />
        <div>
          <Button
            type="button"
            variant="outline"
            loading={linking}
            disabled={!selectedUserId}
            onClick={handleLink}
          >
            Link {selectedUserLabel ? `"${selectedUserLabel}"` : "user"}
          </Button>
        </div>
      </div>
    </div>
  );
}
