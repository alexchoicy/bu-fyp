<script setup lang="ts">
  import type { StudentCourse } from "@fyp/api/student/types";
  import { Pencil, Calendar } from "lucide-vue-next";

  defineProps({
    course: {
      type: Object as () => StudentCourse,
      required: true,
    },
  });

  function navigateToCourse(courseId: string) {
    navigateTo(`/courses/${courseId}`);
  }
</script>

<template>
  <Card :key="course.id" class="cursor-pointer hover:shadow-lg transition-shadow" @click="navigateToCourse(course.id!)">
    <CardHeader>
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <div class="flex items-center gap-3">
            <CardTitle class="font-mono text-lg">
              {{ course.course.code?.tag }} {{ course.course.courseNumber }}
            </CardTitle>
            <Badge variant="secondary">{{ course.grade }}</Badge>
            <Badge variant="outline">{{ course.course.credit }} Credits</Badge>
          </div>
          <CardDescription>{{ course.course.name }}</CardDescription>
        </div>
        <div class="flex items-center gap-2">
          <Button variant="outline" size="sm" as-child>
            <Link>
              <Pencil class="h-4 w-4" />
            </Link>
          </Button>
        </div>
      </div>
    </CardHeader>
    <CardContent class="space-y-2">
      <div class="flex items-center gap-2">
        <Calendar class="h-4 w-4 text-muted-foreground" />
        <span>{{ course.term }} {{ course.year }}</span>
      </div>
    </CardContent>
  </Card>
</template>
