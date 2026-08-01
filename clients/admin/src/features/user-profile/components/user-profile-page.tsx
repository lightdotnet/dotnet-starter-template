import { redirect } from "next/navigation";
import QRCode from "qrcode";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Alert, AlertDescription, AlertTitle } from "@/components/ui/alert";
import { resolveSession } from "@/features/user-profile/api/resolve-session";
import { dedupeClaims } from "@/lib/shared/dedupe-claims";
import { getDisplayName, getInitials } from "@/lib/shared/user-display";

export async function UserProfilePage() {
  const resolved = await resolveSession();

  if (!resolved) {
    redirect("/login");
  }

  const { profile } = resolved;

  if (!profile.isSuccess || !profile.data) {
    return (
      <Alert variant="destructive">
        <AlertTitle>Unable to load profile</AlertTitle>
        <AlertDescription>
          {profile.message || "Please try again."}
        </AlertDescription>
      </Alert>
    );
  }

  const user = profile.data;
  const claims = dedupeClaims(user.claims);
  const qrCode = await QRCode.toDataURL(user.id, { width: 200, margin: 1 });

  return (
    <div className="flex flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">My profile</h1>
        <p className="text-sm text-muted-foreground">
          Account details for the currently signed-in user.
        </p>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-3">
        <Card>
          <CardHeader>
            <CardTitle>User ID QR code</CardTitle>
            <CardDescription>Encodes the raw user ID.</CardDescription>
          </CardHeader>
          <CardContent className="flex flex-col items-center gap-3">
            {/* eslint-disable-next-line @next/next/no-img-element -- static data: URI, no next/image optimization needed */}
            <img
              src={qrCode}
              alt={`QR code for user ID ${user.id}`}
              width={200}
              height={200}
              className="rounded-lg border border-border p-2"
            />
            <p className="text-center font-mono text-xs break-all text-muted-foreground">
              {user.id}
            </p>
          </CardContent>
        </Card>

        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Account</CardTitle>
            <CardDescription>
              Profile information from Identity.Api.
            </CardDescription>
          </CardHeader>
          <CardContent className="flex flex-col gap-4">
            <div className="flex items-center gap-3">
              <Avatar size="lg">
                <AvatarFallback>{getInitials(user)}</AvatarFallback>
              </Avatar>
              <div>
                <p className="text-sm font-semibold">{getDisplayName(user)}</p>
                <p className="text-xs text-muted-foreground">
                  @{user.userName}
                </p>
              </div>
            </div>

            <Separator />

            <dl className="grid grid-cols-1 gap-3 sm:grid-cols-2">
              <div>
                <dt className="text-xs text-muted-foreground">User ID</dt>
                <dd className="font-mono text-sm">{user.id}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Status</dt>
                <dd className="text-sm">{user.status ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Email</dt>
                <dd className="text-sm">{user.email ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">Phone</dt>
                <dd className="text-sm">{user.phoneNumber ?? "—"}</dd>
              </div>
              <div>
                <dt className="text-xs text-muted-foreground">
                  Auth provider
                </dt>
                <dd className="text-sm">{user.authProvider ?? "—"}</dd>
              </div>
            </dl>

            <Separator />

            <div>
              <p className="mb-1.5 text-xs text-muted-foreground">Roles</p>
              <div className="flex flex-wrap gap-1.5">
                {user.roles.length > 0 ? (
                  user.roles.map((role) => <Badge key={role}>{role}</Badge>)
                ) : (
                  <span className="text-sm text-muted-foreground">
                    No roles assigned.
                  </span>
                )}
              </div>
            </div>

            <div>
              <p className="mb-1.5 text-xs text-muted-foreground">Claims</p>
              <div className="flex flex-wrap gap-1.5">
                {claims.length > 0 ? (
                  claims.map((claim) => (
                    <Badge
                      key={`${claim.type}:${claim.value}`}
                      variant="outline"
                    >
                      {claim.type}: {claim.value}
                    </Badge>
                  ))
                ) : (
                  <span className="text-sm text-muted-foreground">
                    No claims.
                  </span>
                )}
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
