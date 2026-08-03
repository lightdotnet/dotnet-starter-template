import { AppShell } from "@/components/layout/app-shell";
import { resolveSession } from "@/features/user-profile";

export default async function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const session = await resolveSession();

  return (
    <AppShell
      permissions={session?.permissions ?? []}
      userName={session?.profile?.userName ?? null}
      user={session?.profile ?? null}
    >
      {children}
    </AppShell>
  );
}
