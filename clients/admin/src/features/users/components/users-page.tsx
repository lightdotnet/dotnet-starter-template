import { redirect } from "next/navigation";
import { ShieldOff } from "lucide-react";
import { Empty, EmptyDescription, EmptyMedia, EmptyTitle } from "@/components/ui/empty";
import { resolveSession } from "@/features/user-profile";
import { searchUsers } from "@/features/users/api/search-users";
import { UsersDataTable } from "@/features/users/components/users-data-table";
import { hasPermission } from "@/lib/server/authorization";
import { IDENTITY_PERMISSIONS } from "@/constants/permissions";

const PAGE_SIZE = 10;

interface UsersPageProps {
  searchParams: Promise<{ q?: string; page?: string }>;
}

export async function UsersPage({ searchParams }: UsersPageProps) {
  const session = await resolveSession();
  if (!session) {
    redirect("/login");
  }

  if (!hasPermission(session, session.profile?.userName, IDENTITY_PERMISSIONS.Users.View)) {
    return (
      <Empty>
        <EmptyMedia variant="icon">
          <ShieldOff />
        </EmptyMedia>
        <EmptyTitle>Access denied</EmptyTitle>
        <EmptyDescription>
          You don&apos;t have the {IDENTITY_PERMISSIONS.Users.View} permission.
        </EmptyDescription>
      </Empty>
    );
  }

  const canCreate = hasPermission(
    session,
    session.profile?.userName,
    IDENTITY_PERMISSIONS.Users.Create,
  );

  const { q, page } = await searchParams;
  const pageNumber = Math.max(Number(page) || 1, 1);

  const result = await searchUsers(session.accessToken, {
    searchValue: q,
    pageNumber,
    pageSize: PAGE_SIZE,
  });

  const error =
    !result.isSuccess || !result.data
      ? { title: "Unable to load users", description: result.message || "Please try again." }
      : undefined;
  const paged = result.data;

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">Users</h1>
        <p className="text-sm text-muted-foreground">
          Manage the accounts that can sign in to the workspace.
        </p>
      </div>

      <UsersDataTable
        records={paged?.records ?? []}
        searchValue={q ?? ""}
        pageNumber={paged?.pageNumber ?? pageNumber}
        pageSize={paged?.pageSize ?? PAGE_SIZE}
        totalPages={paged?.totalPages ?? 1}
        totalRecords={paged?.totalRecords ?? 0}
        error={error}
        canCreate={canCreate}
      />
    </div>
  );
}
