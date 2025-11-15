<script setup lang="ts">
  import type { Course, CreateCourseSection } from "@fyp/api/course/types";
  import { TermMap } from "@fyp/api/static/types";
  import { generateYearOptions, formatAcademicYear } from "@fyp/api/course/utils";

  const id = useRoute().params.id as string;

  const { data } = await useAPI<Course>(`/courses/${id}`, {});

  const defaultAcademicYear = generateYearOptions(0, 0)[0] ?? formatAcademicYear(new Date().getFullYear());

  const request = ref({
    term: "SEM1",
    year: defaultAcademicYear,
    position: 30,
    meetings: [],
  } as CreateCourseSection);

  const addMeeting = () => {
    request.value.meetings.push({
      day: "MON",
      sectionType: "LEC",
      startTime: "8:30",
      endTime: "10:20",
      location: "NA",
    });
  };

  onMounted(() => {
    addMeeting();
  });

  function submitForm() {
    useNuxtApp().$backend(`/courses/${id}/sections`, {
      method: "POST",
      body: request.value,
    });
  }
</script>

<template>
  <div class="space-y-4">
    <Card>
      <CardHeader>
        <CardTitle>Section Information</CardTitle>
      </CardHeader>
      <CardContent class="space-y-4">
        <div class="space-y-2">
          <Label>Course</Label>
          <p>{{ data?.code?.tag }}{{ data?.courseNumber }} - {{ data?.name }}</p>
          <div class="rounded-lg border border-border p-4">
            <p class="text-sm text-muted-foreground">
              {{ data?.description || "No course description provided." }}
            </p>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-6">
          <div class="space-y-2">
            <Label>Term</Label>
            <Select v-model="request.term">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select term" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="(term, code) in TermMap" :key="code" :value="code">
                  {{ term.label }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div class="space-y-2">
            <Label>Academic Year</Label>
            <Select v-model="request.year">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select year..." />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="year in generateYearOptions(2, 0)" :key="year" :value="year">
                  {{ year }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>

        <div class="space-y-2">
          <Label for="capacity">Enrollment Capacity</Label>
          <Input id="capacity" v-model="request.position" type="number" placeholder="30" />
        </div>
      </CardContent>
    </Card>

    <Card>
      <CardHeader>
        <CardTitle>Meeting Schedule</CardTitle>
      </CardHeader>

      <CardContent class="space-y-4">
        <div class="flex justify-end">
          <Button @click="addMeeting">Create Meeting</Button>
        </div>
        <div class="space-y-4">
          <CourseCreateScheduleMeetingCard
            v-for="(meeting, index) in request.meetings"
            :key="index"
            v-model="request.meetings[index]!" />
        </div>
      </CardContent>
    </Card>

    <div class="flex justify-end">
      <Button @click="submitForm">Create Section</Button>
    </div>
  </div>
</template>
