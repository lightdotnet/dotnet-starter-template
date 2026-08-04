import { redirect } from "next/navigation";
import { ShieldOff } from "lucide-react";
import { Card, CardContent } from "@/components/ui/card";
import {
  Empty,
  EmptyDescription,
  EmptyMedia,
  EmptyTitle,
} from "@/components/ui/empty";
import { resolveSession } from "@/features/user-profile";
import { getAllRoles } from "@/features/roles/api/get-all-roles";
import { RolesDataTable } from "@/features/roles/components/roles-data-table";
import { ROLES_PERMISSIONS } from "@/features/roles/constants/permissions";
import { hasPermission } from "@/lib/server/authorization";

export async function RolesPage() {
  const session = await resolveSession();
  if (!session) {
    redirect("/login");
  }

  if (
    !hasPermission(session, session.profile?.userName, ROLES_PERMISSIONS.View)
  ) {
    return (
      <Empty>
        <EmptyMedia variant="icon">
          <ShieldOff />
        </EmptyMedia>
        <EmptyTitle>Access denied</EmptyTitle>
        <EmptyDescription>
          You don&apos;t have the {ROLES_PERMISSIONS.View} permission.
        </EmptyDescription>
      </Empty>
    );
  }

  const canManage = hasPermission(
    session,
    session.profile?.userName,
    ROLES_PERMISSIONS.Manage,
  );

  const result = await getAllRoles(session.accessToken);

  const error =
    !result.isSuccess || !result.data
      ? {
          title: "Unable to load roles",
          description: result.message || "Please try again.",
        }
      : undefined;
  const roles = result.data ?? [];

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Roles</h1>
        <p className="text-sm text-muted-foreground">
          Manage roles and the permissions assigned to them.
        </p>
      </div>

      <Card>
        <CardContent>
          <RolesDataTable
            roles={roles}
            error={error}
            canManage={canManage}
          />
        </CardContent>
      </Card>
    </div>
  );
}
