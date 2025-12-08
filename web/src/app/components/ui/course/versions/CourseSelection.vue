<script setup lang="ts">
import { BookOpen } from "lucide-vue-next";
import type { components } from "~/API/schema";

const props = defineProps({
  title: {
    type: String,
    required: true,
  },
  description: {
    type: String,
    required: true,
  },
  courseList: {
    type: Array as () => components["schemas"]["SimpleCourseDto"][],
    required: true,
  },
});

const courseVersionData = defineModel<number[]>({
  required: true,
});

// Transform course list to have id and name properties for MultiSelectCombobox
const transformedCourses = computed(() => {
  return (
    props.courseList?.map((course) => ({
      id: course.id ?? 0,
      name: `${course.codeTag} ${course.courseNumber} - ${course.name}`,
    })) || []
  );
});

const selectedCourses = computed({
  get() {
    return (
      props.courseList
        ?.filter((course) =>
          (courseVersionData.value || []).includes(
            Number(course.mostRecentVersion?.id ?? 0)
          )
        )
        .map((course) => ({
          id: course.id ?? 0,
          name: `${course.codeTag} ${course.courseNumber} - ${course.name}`,
        })) || []
    );
  },
  set(selected: Array<{ id: string | number; name: string }>) {
    courseVersionData.value = selected.map((s) => Number(s.id));
  },
});
</script>

<template>
  <Card>
    <CardHeader class="pb-4">
      <div class="flex items-center gap-2">
        <BookOpen class="h-5 w-5 text-primary" />
        <div>
          <CardTitle class="text-base font-semibold">{{ title }}</CardTitle>
          <CardDescription class="text-sm">{{ description }}</CardDescription>
        </div>
      </div>
    </CardHeader>
    <CardContent>
      <div class="space-y-4">
        <UiMultiSelectCombobox
          v-model="selectedCourses"
          label="Courses"
          placeholder="Select courses..."
          empty-message="No courses found."
          :options="transformedCourses"
        />
      </div>
    </CardContent>
  </Card>
</template>
