import { requestJson } from "@/lib/server/backend-api";
import { guardCall } from "@/lib/server/call-guard";
import type { PagedResult } from "@/types/api";
import type { SearchUsersParams, UserDto } from "@/features/users/types/user";

export function searchUsers(params: SearchUsersParams = {}) {
  return guardCall(() =>
    requestJson<PagedResult<UserDto>>("user/search", {
      method: "GET",
      query: {
        searchValue: params.searchValue,
        pageNumber: String(params.pageNumber ?? 1),
        pageSize: String(params.pageSize ?? 20),
      },
    }),
  );
}
