<script setup lang="ts">
  import type { StudentCourse } from "@fyp/api/student/types";
  import { Pencil, Calendar } from "lucide-vue-next";

  defineProps({
    record: {
      type: Object as () => StudentCourse,
      required: true,
    },
  });

  function navigateToCourse(courseId: string) {
    navigateTo(`/courses/${courseId}`);
  }
</script>

<template>
  <Card
    :key="record.id"
    class="cursor-pointer hover:shadow-lg transition-shadow"
    @click="navigateToCourse(record.course.id!)">
    <CardHeader>
      <div class="flex items-start justify-between">
        <div class="flex-1">
          <div class="flex items-center gap-3">
            <CardTitle class="font-mono text-lg">
              {{ record.course.code?.tag }} {{ record.course.courseNumber }}
            </CardTitle>
            <Badge variant="secondary">{{ record.grade }}</Badge>
            <Badge variant="outline">{{ record.course.credit }} Credits</Badge>
          </div>
          <CardDescription>{{ record.course.name }}</CardDescription>
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
        <span>{{ record.term }} {{ record.year }}</span>
      </div>
    </CardContent>
  </Card>
</template>
