export default defineNuxtPlugin((nuxtApp) => {
  const config = useRuntimeConfig();
  const headers = useRequestHeaders(["cookie"]);

  const api = $fetch.create({
    baseURL: config.public.API_BASEURL,
    credentials: "include",
    headers,
    async onResponseError({ response }) {
      if (response.status === 401) {
        await nuxtApp.runWithContext(() => navigateTo("/auth/login"));
      }
    },
  });

  return {
    provide: {
      api,
    },
  };
});
