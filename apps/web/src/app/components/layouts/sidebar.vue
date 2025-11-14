<script setup lang="ts">
  import { BookOpen, LayoutDashboard, Settings } from "lucide-vue-next";

  const user = useAuthUser();

  const navigationUser = [
    {
      title: "Dashboard",
      icon: LayoutDashboard,
      href: "/",
    },
    {
      title: "Courses",
      icon: BookOpen,
      href: "/courses",
      items: [{ title: "Course Catalog", href: "/courses" }],
    },
  ];

  const navigationAdmin = [
    {
      title: "Dashboard",
      icon: LayoutDashboard,
      href: "/",
    },
    {
      title: "Courses",
      icon: BookOpen,
      href: "/courses",
      items: [
        { title: "Course Catalog", href: "/courses" },
        { title: "Create Course", href: "/courses/create", permission: "admin" },
      ],
    },
  ];
</script>

<template>
  <Sidebar>
    <SidebarHeader>
      <NuxtLink to="/" class="text-2xl font-bold items-center flex justify-center">FYP</NuxtLink>
    </SidebarHeader>
    <SidebarContent>
      <SidebarGroup>
        <SidebarGroupContent>
          <ClientOnly>
            <SidebarMenu v-if="user?.role === 'admin'">
              <SidebarMenuItem v-for="(item, index) in navigationAdmin" :key="index">
                <template v-if="item.items">
                  <SidebarMenuButton class="w-full">
                    <NuxtLink :to="item.href" class="flex items-center gap-2 w-full h-full">
                      <component :is="item.icon" class="h-4 w-4" />
                      <span>{{ item.title }}</span>
                    </NuxtLink>
                  </SidebarMenuButton>
                  <SidebarMenuSub>
                    <SidebarMenuSubItem v-for="(subItem, subIndex) in item.items" :key="subIndex">
                      <SidebarMenuSubButton as-child>
                        <NuxtLink :to="subItem.href" class="">
                          {{ subItem.title }}
                        </NuxtLink>
                      </SidebarMenuSubButton>
                    </SidebarMenuSubItem>
                  </SidebarMenuSub>
                </template>
                <template v-else>
                  <SidebarMenuButton as-child class="w-full">
                    <NuxtLink :to="item.href" class="flex items-center gap-2 w-full h-full">
                      <component :is="item.icon" class="h-4 w-4" />
                      <span>{{ item.title }}</span>
                    </NuxtLink>
                  </SidebarMenuButton>
                </template>
              </SidebarMenuItem>
            </SidebarMenu>
            <SidebarMenu v-else>
              <SidebarMenuItem v-for="(item, index) in navigationUser" :key="index">
                <template v-if="item.items">
                  <SidebarMenuButton class="w-full">
                    <NuxtLink :to="item.href" class="flex items-center gap-2 w-full h-full">
                      <component :is="item.icon" class="h-4 w-4" />
                      <span>{{ item.title }}</span>
                    </NuxtLink>
                  </SidebarMenuButton>
                  <SidebarMenuSub>
                    <SidebarMenuSubItem v-for="(subItem, subIndex) in item.items" :key="subIndex">
                      <SidebarMenuSubButton as-child>
                        <NuxtLink :to="subItem.href" class="">
                          {{ subItem.title }}
                        </NuxtLink>
                      </SidebarMenuSubButton>
                    </SidebarMenuSubItem>
                  </SidebarMenuSub>
                </template>
                <template v-else>
                  <SidebarMenuButton as-child class="w-full">
                    <NuxtLink :to="item.href" class="flex items-center gap-2 w-full h-full">
                      <component :is="item.icon" class="h-4 w-4" />
                      <span>{{ item.title }}</span>
                    </NuxtLink>
                  </SidebarMenuButton>
                </template>
              </SidebarMenuItem>
            </SidebarMenu>
          </ClientOnly>
        </SidebarGroupContent>
      </SidebarGroup>
    </SidebarContent>

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
