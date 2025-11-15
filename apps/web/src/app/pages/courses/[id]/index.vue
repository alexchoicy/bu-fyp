<script setup lang="ts">
  import type { Course } from "@fyp/api/course/types";
  import { BookOpen, CheckCircle2 } from "lucide-vue-next";

  const id = useRoute().params.id as string;

  const { data } = await useAPI<Course>(`/courses/${id}`, {});

  if (!data.value) {
    throw createError({ statusCode: 404, statusMessage: "Course not found" });
  }

  const tabs = [
    { title: "Overview", id: "overview" },
    { title: "Syllabus", id: "syllabus" },
    { title: "Requirements", id: "requirements" },
    { title: "Resources", id: "resources" },
    { title: "Schedule", id: "schedule" },
  ];
</script>

<template>
  <div v-if="data" class="space-y-4">
    <Card>
      <CardHeader>
        <CardTitle>{{ data.code?.tag }}{{ data.courseNumber }} - {{ data.name }}</CardTitle>
      </CardHeader>
      <CardContent class="space-y-4">
        <div class="flex flex-wrap gap-4 text-sm">
          <div class="flex items-center gap-2">
            <BookOpen class="h-4 w-4 text-muted-foreground" />
            <span>{{ data.credit }} Credits</span>
          </div>
        </div>
      </CardContent>
    </Card>
    <Tabs default-value="overview">
      <TabsList class="w-full">
        <TabsTrigger v-for="tab in tabs" :key="tab.id" :value="tab.id">
          {{ tab.title }}
        </TabsTrigger>
      </TabsList>
      <TabsContent value="overview">
        <div class="flex flex-col gap-4">
          <Card>
            <CardHeader>
              <CardTitle>Course Description</CardTitle>
            </CardHeader>
            <CardContent>
              <p>{{ data.description || "No description available." }}</p>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle>Learning Outcomes</CardTitle>
            </CardHeader>
            <CardContent>
              <ul class="space-y-2">
                <li v-for="n in 5" :key="n" class="flex items-start gap-2">
                  <CheckCircle2 class="mt-0.5 h-4 w-4 flex-shrink-0 text-chart-3" />
                </li>
              </ul>
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle>Topics</CardTitle>
            </CardHeader>
            <CardContent>
              <div class="grid gap-2 sm:grid-cols-2">
                <div class="flex items-center p-3 rounded-lg border border-border">
                  <BookOpen class="h-4 w-4 text-muted-foreground" />
                  <span class="text-sm">{topic}</span>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>
      </TabsContent>
      <TabsContent value="syllabus"></TabsContent>
      <TabsContent value="schedule">
        <CourseSchedule :course="data" />
      </TabsContent>
    </Tabs>
  </div>
</template>
