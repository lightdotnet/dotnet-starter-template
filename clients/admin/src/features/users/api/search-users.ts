import { requestJson } from "@/lib/server/http";
import { guardCall } from "@/lib/server/call-guard";
import type { PagedResult } from "@/types/api";
import type { SearchUsersParams, UserDto } from "@/types/user";

export function searchUsers(accessToken: string, params: SearchUsersParams = {}) {
  return guardCall(() =>
    requestJson<PagedResult<UserDto>>("user/search", {
      method: "POST",
      accessToken,
      query: {
        searchValue: params.searchValue,
        pageNumber: String(params.pageNumber ?? 1),
        pageSize: String(params.pageSize ?? 20),
      },
    }),
  );
}
