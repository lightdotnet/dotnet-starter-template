import { AppShell } from "@/components/layout/app-shell";
import { resolveSession } from "@/features/user-profile";

export default async function DashboardLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const resolved = await resolveSession();
  const user =
    resolved?.profile.isSuccess && resolved.profile.data
      ? resolved.profile.data
      : null;

  return <AppShell user={user}>{children}</AppShell>;
}
