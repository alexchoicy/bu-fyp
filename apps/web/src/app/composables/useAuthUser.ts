import type { JWTCustomPayloadUserInfo } from "@fyp/api/auth/types";

export function useAuthUser() {
  return useState<JWTCustomPayloadUserInfo | null>("auth-user", () => null);
}
