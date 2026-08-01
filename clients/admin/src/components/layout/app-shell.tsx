import { SidebarProvider } from "@/hooks/use-sidebar";
import { Sidebar } from "@/components/layout/sidebar";
import { TopBar } from "@/components/layout/topbar";
import type { UserDto } from "@/types/user";

export function AppShell({
  user,
  children,
}: {
  user: UserDto | null;
  children: React.ReactNode;
}) {
  return (
    <SidebarProvider>
      <div className="flex min-h-full flex-col">
        <TopBar user={user} />
        <div className="flex flex-1">
          <Sidebar />
          <main className="flex-1 p-4 sm:p-6">{children}</main>
        </div>
      </div>
    </SidebarProvider>
  );
}
