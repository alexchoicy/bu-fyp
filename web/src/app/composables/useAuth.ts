import type { paths } from "~/API/schema";

type UserInfo =
  paths["/api/Auth/login"]["post"]["responses"]["200"]["content"]["application/json"];

export const useAuth = () => {
  const user = useState<UserInfo | null>("authUser", () => null);
  const loading = useState<boolean>("authLoading", () => false);

  const login = async (
    credential: paths["/api/Auth/login"]["post"]["requestBody"]["content"]["application/json"]
  ) => {
    loading.value = true;
    try {
      const data = await useNuxtApp().$api<UserInfo>("auth/login", {
        method: "POST",
        body: credential,
        credentials: "include",
        baseURL: useRuntimeConfig().public.API_BASEURL,
      });
      user.value = data ?? null;
      return user.value;
    } finally {
      loading.value = false;
    }
  };

  const fetchMe = async () => {
    loading.value = true;
    try {
      const { data, error } = await useFetch<UserInfo>("auth/me", {
        method: "GET",
        credentials: "include",
        headers: useRequestHeaders(["cookie"]),
        baseURL: useRuntimeConfig().public.API_BASEURL,
      });

      if (error.value) {
        user.value = null;
        return null;
      }

      user.value = data.value ?? null;
      return user.value;
    } finally {
      loading.value = false;
    }
  };

  return {
    user,
    loading,
    isAuthenticated: computed(() => !!user.value),
    login,
    fetchMe,
  };
};
