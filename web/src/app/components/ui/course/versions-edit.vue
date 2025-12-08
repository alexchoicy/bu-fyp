<script setup lang="ts">
import type { components } from "~/API/schema";
import { BookMarked, Target } from "lucide-vue-next";
const courseVersionData = defineModel<
  components["schemas"]["CreateCourseVersionDto"]
>({
  required: true,
});
</script>

<template>
  <UiCourseVersionsCourseVersionMeta v-model="courseVersionData" />

  <Card>
    <CardHeader class="pb-4">
      <div class="flex items-center gap-2">
        <Target class="h-5 w-5 text-primary" />
        <div>
          <CardTitle class="text-base font-semibold"
            >Aims & Objectives</CardTitle
          >
          <CardDescription class="text-sm"
            >Describe the course aims and learning objectives</CardDescription
          >
        </div>
      </div>
    </CardHeader>
    <CardContent>
      <div class="space-y-2">
        <Label for="aimAndObjectives">Course Aims & Objectives</Label>
        <Textarea
          id="aimAndObjectives"
          v-model="courseVersionData.aimAndObjectives"
          :rows="6"
          class="resize-y min-h-[150px]"
        />
      </div>
    </CardContent>
  </Card>

  <Card>
    <CardHeader class="pb-4">
      <div class="flex items-center gap-2">
        <BookMarked class="h-5 w-5 text-primary" />
        <div>
          <CardTitle class="text-base font-semibold">Course Content</CardTitle>
          <CardDescription class="text-sm"
            >Detailed course syllabus and topics covered</CardDescription
          >
        </div>
      </div>
    </CardHeader>
    <CardContent>
      <div class="space-y-2">
        <Label for="courseContent">Syllabus Content</Label>
        <Textarea
          id="courseContent"
          v-model="courseVersionData.courseContent"
          :rows="12"
          class="resize-y min-h-[250px] font-mono text-sm"
        />
      </div>
    </CardContent>
  </Card>

  <UiCourseVersionsCILOsForm v-model="courseVersionData" />
  <UiCourseVersionsTLAsForm v-model="courseVersionData" />
  <UiCourseVersionsAssessmentMethodsForm v-model="courseVersionData" />
</template>
