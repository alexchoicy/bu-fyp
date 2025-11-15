<script setup lang="ts">
  import type { Course } from "@fyp/api/course/types";
  import { CourseSectionTypes, CourseSectionWeekdays, TermMap } from "@fyp/api/static/types";
  import { Book } from "lucide-vue-next";

  const user = useAuthUser();

  const props = defineProps({
    course: {
      type: Object as () => Course,
      required: true,
    },
  });

  function onClickCreate() {
    navigateTo(`/courses/${props.course.id}/schedule/create`);
  }
</script>

<template>
  <div class="space-y-4">
    <div class="flex justify-end">
      <Button v-if="user?.role === 'admin'" @click="onClickCreate">Create Schedule</Button>
    </div>

    <Card>
      <CardHeader>
        <CardTitle>Course Schedule</CardTitle>
      </CardHeader>
      <CardContent class="space-y-4">
        <div v-for="section in course.sections" :key="section.id" class="rounded-lg border border-border p-4 space-y-3">
          <div class="flex items-center justify-between">
            <div>
              <h4 class="font-semibold">Section {{ section.id }}</h4>
              <p class="text-sm text-muted-foreground">{{ TermMap[section.term].label }} {{ section.year }}</p>
            </div>
          </div>

          <div class="space-y-2">
            <div
              v-for="meeting in section.meetings"
              :key="meeting.id"
              class="flex items-start gap-3 rounded-lg bg-muted/50 p-3">
              <Book />
              <div class="flex-1 space-y-1">
                <div class="flex items-center gap-2">
                  <p class="font-medium text-sm">{{ CourseSectionTypes[meeting.sectionType] }}</p>
                  <Badge variant="outline" class="text-xs">{{ CourseSectionWeekdays[meeting.day] }}</Badge>
                </div>
                <p class="text-sm text-muted-foreground">{{ meeting.startTime }} - {{ meeting.endTime }}</p>
                <p class="text-sm text-muted-foreground">{{ meeting.location }}</p>
              </div>
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
