export default defineNuxtRouteMiddleware(async (to) => {
  const publicRoutes = ["/auth/login"];
  if (publicRoutes.includes(to.path)) {
    return;
  }

  const { isAuthenticated, fetchMe, user } = useAuth();

  if (!isAuthenticated.value) {
    await fetchMe();
  }

  if (!isAuthenticated.value) {
    return navigateTo("/auth/login");
  }

  const roles = user.value?.roles || [];
  if (roles.includes("Student") && !to.path.startsWith("/")) {
    return navigateTo("/");
  }
  if (roles.includes("Admin") && !to.path.startsWith("/admin")) {
    return navigateTo("/admin");
  }
});
