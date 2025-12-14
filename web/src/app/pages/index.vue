<script setup lang="ts">
const { user } = useAuth();
</script>

<template>
  <!-- <div class="container mx-auto p-8">
    <h1 class="text-3xl font-bold mb-6">Home</h1>

    <div class="mb-4">
      <h2 class="text-xl font-semibold mb-2">Current User Info:</h2>
      <pre class="bg-gray-100 p-4 rounded overflow-auto">{{ user }}</pre>
    </div>
  </div> -->
  <UiDashboardStudent />
</template>
