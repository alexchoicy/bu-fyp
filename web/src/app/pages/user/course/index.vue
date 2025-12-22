<script setup lang="ts">
import type { components } from "~/API/schema";

const { data: courses } = useAPI<components["schemas"]["UserCourseDto"][]>('me/courses')

const enrolledCourses = computed(() =>
  courses.value?.filter(course => course.status === 'Enrolled') || []
)

const completedCourses = computed(() =>
  courses.value
    ?.filter(course => ['Completed', 'Withdrawn', 'Exemption'].includes(course.status))
    .sort((a, b) => {
      const priority: Record<string, number> = { Completed: 0, Withdrawn: 1, Exemption: 2 }
      return (priority[a.status] ?? 99) - (priority[b.status] ?? 99)
    }) || []
)

const plannedCourses = computed(() =>
  courses.value?.filter(course => course.status === "Planned") || []
)
</script>

<template>
  <div class="flex-1">
    <div class="flex flex-col gap-6 p-6">
      <div class="flex flex-row justify-between">
        <div class="flex flex-col gap-2">
          <h2 class="text-2xl font-bold text-balance">Academic Records</h2>
          <p class="text-muted-foreground">
            View and manage your course history, grades, and academic progress
          </p>
        </div>
        <Button asChild>
          <NuxtLink to="/user/course/create">
            Add new record
          </NuxtLink>
        </Button>
      </div>

      <Tabs default-value="completed">
        <TabsList>
          <TabsTrigger value="enrolled">Enrolled ({{ enrolledCourses.length }})</TabsTrigger>
          <TabsTrigger value="completed">Completed ({{ completedCourses.length }})</TabsTrigger>
          <TabsTrigger value="planned">Planned ({{ plannedCourses.length }})</TabsTrigger>
        </TabsList>


        <TabsContent value="enrolled">
          <UiMeCourseCard :courses="enrolledCourses" />
        </TabsContent>
        <TabsContent value="completed">
          <UiMeCourseCard :courses="completedCourses" />
        </TabsContent>
        <TabsContent value="planned">
          <UiMeCourseCard :courses="plannedCourses" />
        </TabsContent>
      </Tabs>

    </div>
  </div>
</template>
