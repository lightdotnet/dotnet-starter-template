/** Mirrors `Identity.Contracts/Authorization/IdentityPermissions.cs` — keep the string values in sync with the backend. */
export const IDENTITY_PERMISSIONS = {
  Users: {
    View: "Users.View",
    Create: "Users.Create",
    Update: "Users.Update",
    Delete: "Users.Delete",
  },
} as const;
