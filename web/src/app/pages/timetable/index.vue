<script setup lang="ts">
import type { components, paths } from "~/API/schema";
import { Calendar } from "lucide-vue-next";
type tableResponse = components['schemas']['TimetableResponseDto']
// 1 section has many meetings
type Section = components['schemas']['TimetableSectionDto']

type Query = paths['/api/timetable']['get']['parameters']['query'];

const query = ref<Query>({
  courseGroupId: undefined,
});

const { data: availableItems, refresh } = useAPI<tableResponse>('timetable', { query });

const filterDialogOpen = ref(false);

const selectedSections = ref<Section[]>([]);

const handleAddSection = (section: Section) => {
  const course = availableItems.value?.entries?.find((c) => c.sections!.some((s) => s.sectionId === section.sectionId))

  if (!course) return;

  const existingFromSameCourse = selectedSections.value.find((s) => {
    const existingCourse = availableItems.value?.entries?.find((c) =>
      c.sections!.some((sec) => sec.sectionId === s.sectionId)
    );
    return existingCourse?.courseId === course.courseId;
  });

  if (existingFromSameCourse) {
    selectedSections.value = selectedSections.value.map((s) => {
      const existingCourse = availableItems.value?.entries?.find((c) =>
        c.sections!.some((sec) => sec.sectionId === s.sectionId)
      );
      return existingCourse?.courseId === course.courseId ? section : s;
    });
  } else {
    selectedSections.value = [...selectedSections.value, section];
  }
}

const handleRemoveSection = (sectionId: number) => {
  selectedSections.value = selectedSections.value.filter(
    (s) => s.sectionId !== sectionId
  );
};

const totalCredits = computed(() => {
  return selectedSections.value.reduce((total, section) => {
    const course = availableItems.value?.entries?.find((c) =>
      c.sections!.some((s) => s.sectionId === section.sectionId)
    );
    return total + (course?.credit ?? 0);
  }, 0);
});
</script>

<template>
  <div class="flex flex-col h-screen">
    <header class="shrink-0 border-b border-border bg-background">
      <div class="flex items-center justify-between px-4 py-3">
        <div class="flex items-center gap-2">
          <Calendar class="h-5 w-5 text-primary" />
          <h1 class="text-lg font-semibold">Timetable Planner</h1>
        </div>
        <div class="text-sm font-medium">
          Total Credits: <span class="text-primary">{{ totalCredits }}</span>
        </div>
        <div />
      </div>
    </header>

    <div class="flex-1 flex overflow-hidden">
      <div class="w-80 shrink-0 border-r border-border flex flex-col">
        <div class="px-3 py-2 bg-muted/50 border-b border-border">
          <h2 class="font-medium text-sm">Courses & Sections</h2>
        </div>
        <div class="w-full">
          <Button class=" w-full" @click="filterDialogOpen = true">Filter </Button>
        </div>
        <div class="flex-1 overflow-hidden">
          <UiTimetableCoursePanel :courses="availableItems?.entries ?? []" :selected-sections="selectedSections"
            @add-section="handleAddSection" @remove-section="handleRemoveSection" />
        </div>
      </div>
      <UiTimetableGrid :courses="availableItems?.entries ?? []" :selected-sections="selectedSections" />
    </div>
    <UiTimetableFilter v-model:open="filterDialogOpen" v-model:query="query" :refresh="refresh" />
  </div>
</template>