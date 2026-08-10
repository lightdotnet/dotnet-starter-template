"use server";

import { resolveSession } from "@/features/user-profile";
import { createUser } from "@/features/users/api/create-user";
import type { CreateUserRequest } from "@/features/users/types/user";

export interface CreateUserFormState {
  error?: string;
  success?: boolean;
}

export async function createUserAction(
  _prevState: CreateUserFormState,
  formData: FormData,
): Promise<CreateUserFormState> {
  const session = await resolveSession();
  if (!session) {
    return { error: "Your session has expired. Please sign in again." };
  }

  const userName = String(formData.get("userName") ?? "").trim();
  const password = String(formData.get("password") ?? "");

  if (!userName || !password) {
    return { error: "Username and password are required." };
  }

  const request: CreateUserRequest = {
    userName,
    password,
    firstName: String(formData.get("firstName") ?? "") || undefined,
    lastName: String(formData.get("lastName") ?? "") || undefined,
    email: String(formData.get("email") ?? "") || undefined,
    phoneNumber: String(formData.get("phoneNumber") ?? "") || undefined,
  };

  const result = await createUser(request);

  if (!result.isSuccess) {
    return { error: result.message || "Failed to create user." };
  }

  return { success: true };
}
