import type { ClaimDto } from "@/types/user";

export interface RoleDto {
  id: string;
  name: string;
  description?: string | null;
  claims: ClaimDto[];
}

export interface CreateRoleRequest {
  name: string;
  description?: string;
}
