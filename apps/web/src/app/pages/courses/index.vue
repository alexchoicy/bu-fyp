<script setup lang="ts">
  import type { Course } from "@fyp/api/course/types";
  import { BookOpen, Search } from "lucide-vue-next";

  const { data } = await useAPI<Course[]>("/courses", {});

  function navigateToCourse(courseId: string) {
    navigateTo(`courses/${courseId}`);
  }

  const searchWord = ref("");

  const filteredCourses = computed(() => {
    if (!searchWord.value) {
      return data.value || [];
    }
    return (
      data.value?.filter(
        (course) =>
          course.name.toLowerCase().includes(searchWord.value.toLowerCase()) ||
          `${course.code?.tag}${course.courseNumber}`.toLowerCase().includes(searchWord.value.toLowerCase()),
      ) || []
    );
  });
</script>

<template>
  <div class="flex flex-col gap-6 p-6">
    <div class="flex flex-col gap-2">
      <h2 class="text-2xl font-bold text-balance">Browse Course Catalog</h2>
      <p class="text-muted-foreground">Explore all available courses and find the perfect fit for your program.</p>
    </div>

    <Card>
      <CardHeader>
        <CardTitle>Search & Filter</CardTitle>
      </CardHeader>
      <CardContent>
        <div class="flex flex-col gap-4">
          <div class="relative flex-1">
            <Search class="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input v-model="searchWord" placeholder="Search courses by name, code, ..." class="pl-10" />
          </div>
          <div class="flex flex-wrap gap-3"></div>
        </div>
      </CardContent>
    </Card>

    <Card
      v-for="course in filteredCourses"
      :key="course.id"
      class="cursor-pointer hover:shadow-lg transition-shadow"
      @click="navigateToCourse(course.id!)">
      <CardHeader>
        <div class="flex justify-between">
          <CardTitle>{{ course.code?.tag }}{{ course.courseNumber }} - {{ course.name }}</CardTitle>
          <Button variant="outline" size="sm">View Details</Button>
        </div>
      </CardHeader>
      <CardContent class="space-y-4">
        <p>{{ course.description || "No description available." }}</p>
        <div class="flex flex-wrap gap-4 text-sm">
          <div class="flex items-center gap-2">
            <BookOpen class="h-4 w-4 text-muted-foreground" />
            <span>{{ course.credit }} Credits</span>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
