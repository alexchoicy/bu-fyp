<script setup lang="ts">
  import type { Course, CourseSection } from "@fyp/api/course/types";
  import { type StudentCourseStatus, TermMap, GradeLabels } from "@fyp/api/static/types";
  import { StudentCourseRequestSchema, type StudentCourseRequest } from "@fyp/api/student/types";
  import { Check, ChevronsUpDown, Search } from "lucide-vue-next";
  import { generateYearOptions } from "@fyp/api/course/utils";
  const { data: availableCourses } = await useAPI<Course[]>("courses");

  const courseOptions = computed(
    () =>
      availableCourses.value?.map((course) => ({
        label: `${course.code?.tag}${course.courseNumber} - ${course.name}`,
        value: course.id,
      })) ?? [],
  );

  const StudentCourseStatusLabels: Record<StudentCourseStatus, string> = {
    enrolled: "Enrolled",
    completed: "Completed",
    withdrawn: "Withdrawn",
    dropped: "Dropped",
    failed: "Failed",
    planned: "Planned",
  };
  const [currentAcademicYear] = generateYearOptions(0, 0);

  const request = ref({
    year: currentAcademicYear ?? "",
  } as StudentCourseRequest);

  const disabledGrades = ref<boolean>(true);

  function onStatusChange(value: unknown) {
    const newStatus = value as StudentCourseStatus | null;
    if (!newStatus) {
      return;
    }
    switch (newStatus) {
      case "planned":
      case "dropped":
        request.value.grade = "NA";
        disabledGrades.value = true;
        break;
      case "enrolled":
        request.value.grade = "E";
        disabledGrades.value = true;
        break;
      case "withdrawn":
        request.value.grade = "W";
        disabledGrades.value = true;
        break;
      case "failed":
        request.value.grade = "F";
        disabledGrades.value = true;
        break;
      default:
        disabledGrades.value = false;
        break;
    }
  }

  const user = useAuthUser();

  const onSubmit = async () => {
    const saveRequest = StudentCourseRequestSchema.safeParse(request.value);
    if (!saveRequest.success) {
      console.error("Validation failed:", saveRequest.error);
      alert("Please fill in all required fields correctly.");
      return;
    }
    await useNuxtApp().$backend(`users/${user.value?.uid}/courses`, {
      method: "POST",
      body: request.value,
    });

    await useRouter().push("/courses/personal");
  };

  function onCourseChange(value: unknown) {
    if (!value || typeof value !== "object") {
      request.value.courseID = "";
      return;
    }

    const maybeCourse = value as { label?: unknown; value?: unknown };
    if (typeof maybeCourse.value === "string") {
      request.value.courseID = maybeCourse.value;
    }
  }

  const sections = ref<CourseSection[]>([]);

  watch(
    () => request.value.courseID,
    async (newCourseID) => {
      if (!newCourseID) {
        return;
      }

      const newSections = await useNuxtApp().$backend<CourseSection[]>(`courses/${newCourseID}/sections`);
      sections.value = newSections;
    },
  );
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>Create New Course Record</CardTitle>
    </CardHeader>
    <CardContent>
      <div class="space-y-4">
        <div class="grid grid-cols-4 gap-4">
          <div class="md:col-span-3 space-y-2">
            <Label>Select Course</Label>
            <Combobox by="value" @update:model-value="onCourseChange">
              <ComboboxAnchor as-child>
                <ComboboxTrigger as-child>
                  <Button variant="outline" class="justify-between w-full">
                    <span class="text-wrap">
                      {{
                        request.courseID
                          ? courseOptions.find((option) => option.value === request.courseID)?.label
                          : "Select course..."
                      }}
                    </span>
                    <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
                  </Button>
                </ComboboxTrigger>
              </ComboboxAnchor>
              <ComboboxList class="w-full">
                <div class="relative w-full max-w-sm items-center">
                  <ComboboxInput
                    class="focus-visible:ring-0 border-0 border-b rounded-none h-10"
                    placeholder="Input Tag..." />
                  <span class="absolute start-0 inset-y-0 flex items-center justify-center px-3">
                    <Search class="size-4 text-muted-foreground" />
                  </span>
                </div>
                <ComboboxEmpty>No course found.</ComboboxEmpty>
                <ComboboxGroup>
                  <ComboboxItem v-for="option in courseOptions" :key="option.value" :value="option">
                    {{ option.label }}
                    <ComboboxItemIndicator>
                      <Check class="size-4" />
                    </ComboboxItemIndicator>
                  </ComboboxItem>
                </ComboboxGroup>
              </ComboboxList>
            </Combobox>
          </div>
          <div>
            <div class="space-y-2">
              <Label>Section</Label>
              <Select v-model="request.sectionID">
                <SelectTrigger class="w-full">
                  <SelectValue placeholder="Select section..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem v-for="section in sections" :key="section.id" :value="section.id!">
                    {{ section.id }}
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label>Semester</Label>
            <Select v-model="request.term">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select Semester..." />
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
                <SelectItem v-for="year in generateYearOptions()" :key="year" :value="year">
                  {{ year }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label>Status</Label>
            <Select v-model="request.status" @update:model-value="onStatusChange">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select status..." />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="(label, status) in StudentCourseStatusLabels" :key="status" :value="status">
                  {{ label }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
          <div class="space-y-2">
            <Label>Grade</Label>
            <Select v-model="request.grade" :disabled="disabledGrades">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select grade..." />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="(label, status) in GradeLabels" :key="status" :value="status">
                  {{ label }}
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
        </div>
        <div class="flex justify-end">
          <Button @click="onSubmit">Create Course Record</Button>
        </div>
      </div>
    </CardContent>
  </Card>
</template>
