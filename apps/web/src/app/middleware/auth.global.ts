import type { JWTCustomPayload, JWTCustomPayloadUserInfo, UserRole } from "@fyp/api/auth/types";
import { useAuthUser } from "~/composables/useAuthUser";

function isPublicRoute(to: ReturnType<typeof useRoute>) {
  if (to.path === "/auth/login") {
    const authUser = useAuthUser();
    const event = useRequestEvent();
    const user = event?.context?.user ?? null;
    authUser.value = user;
  }

  return to.meta.public === true;
}

function isAllowedRoute(to: ReturnType<typeof useRoute>, user: JWTCustomPayloadUserInfo) {
  const allowedRoles = to.meta.allowedRoles as UserRole[];
  if (!allowedRoles || allowedRoles.length === 0) {
    return true;
  }
  return allowedRoles.includes(user.role);
}

export default defineNuxtRouteMiddleware(async (to) => {
  if (isPublicRoute(to)) return;
  const event = useRequestEvent();

  const authUser = useAuthUser();

  if (import.meta.server) {
    const user = event?.context?.user ?? null;
    authUser.value = user;
    if (!user) {
      return navigateTo("/auth/login");
    }
    if (!isAllowedRoute(to, user.info)) {
      return navigateTo("/");
    }
    return;
  }

  try {
    const { authenticated, user } = await $fetch<{
      authenticated: boolean;
      user?: JWTCustomPayload;
    }>("/api/auth/session", { credentials: "include" });

    if (!authenticated) {
      authUser.value = null;
      return navigateTo("/auth/login");
    }
    if (!user) {
      authUser.value = null;
      return navigateTo("/auth/login");
    }
    if (!isAllowedRoute(to, user.info)) {
      return navigateTo("/");
    }
    authUser.value = user.info ?? null;
  } catch {
    authUser.value = null;
    return navigateTo("/auth/login");
  }
});
