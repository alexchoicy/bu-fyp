<script setup lang="ts">
  import { Album, User, Upload, Settings } from "lucide-vue-next";
</script>

<template>
  <Sidebar>
    <SidebarHeader>
      <NuxtLink to="/" class="text-2xl font-bold items-center flex justify-center">FYP</NuxtLink>
    </SidebarHeader>
    <SidebarContent></SidebarContent>

    <SidebarFooter>
      <SidebarMenu>
        <SidebarMenuItem>
          <SidebarMenuButton size="lg" class="hover:cursor-pointer" @click="$router.push('/settings')">
            <Settings />
            Settings
          </SidebarMenuButton>
        </SidebarMenuItem>
      </SidebarMenu>
    </SidebarFooter>
  </Sidebar>
</template>
