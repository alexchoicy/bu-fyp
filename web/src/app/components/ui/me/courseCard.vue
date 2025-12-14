<script setup lang="ts">
import type { components } from "~/API/schema";
import { GradeDisplayNames } from "~/types/static/grade";
import { Calendar } from "lucide-vue-next";

defineProps<{
  courses: components["schemas"]["UserCourseDto"][]
}>();
</script>

<template>
  <Card v-for="course in courses" :key="course.id" class="w-full">
    <CardHeader>
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <div class="flex items-center gap-3 flex-wrap">
            <CardTitle>
              {{ course.codeTag }}
              {{ course.courseNumber }}
            </CardTitle>
            <Badge variant="outline">{{ course.status }}</Badge>
            <Badge variant="secondary">{{ course.credit }} Credits</Badge>
            <Badge v-if="course.grade" variant="default">{{ GradeDisplayNames[course.grade] }}</Badge>
          </div>
          <p class="mt-2 text-base font-medium">{{ course.courseName }}</p>
        </div>
        <div class="flex items-center gap-2">
          <!-- <Button variant="outline" size="sm">
            View Course Detail
          </Button>

          <Button variant="destructive" size="sm">
            Delete
          </Button> -->

        </div>
      </div>
    </CardHeader>
    <CardContent class="space-y-3">
      <div class="flex flex-wrap gap-4 text-sm">
        <div class="flex items-center gap-2">
          <Calendar class="h-4 w-4 text-muted-foreground" />
          <span v-if="course.term && course.academicYear">{{ course.term }} {{
            course.academicYear
          }}-{{ course.academicYear + 1 }}</span>
        </div>
      </div>
      <div v-if="course.notes" class="text-sm">
        <span class="text-muted-foreground">Notes: </span>
        <span>{{ course.notes }}</span>
      </div>
    </CardContent>

  </Card>
</template>
