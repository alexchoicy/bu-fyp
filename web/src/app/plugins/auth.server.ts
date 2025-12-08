export default defineNuxtPlugin(async () => {
  const { user, fetchMe } = useAuth();

  if (user.value) return;
  await fetchMe();
});
