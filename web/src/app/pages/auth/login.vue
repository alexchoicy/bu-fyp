<script setup lang="ts">
definePageMeta({
  public: true,
  layout: false,
});
</script>

<template>
  <div class="flex min-h-svh w-full items-center justify-center p-6 md:p-10">
    <div class="w-full max-w-sm">
      <ui-auth-login-form />
    </div>
  </div>
</template>
