import { LoginForm } from "@/features/auth/components/login-form";

export async function LoginPage({
  searchParams,
}: {
  searchParams: Promise<{ redirect?: string }>;
}) {
  const { redirect } = await searchParams;

  return (
    <div className="flex min-h-full flex-1 items-center justify-center p-4">
      <LoginForm redirect={redirect} />
    </div>
  );
}
