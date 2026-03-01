<script setup lang="ts">
import type { components } from "~/API/schema";

const { data: schedule, status } = useAPI<components["schemas"]["SuggestedScheduleResponseDto"]>("me/suggested-schedule");
const { data: userCourses } = useAPI<components["schemas"]["UserCourseDto"][]>("me/courses");

const hasSchedule = computed(() => (schedule.value?.years?.length ?? 0) > 0);

const studiedStatusSet = new Set(["Completed", "Exemption", "Withdrawn"]);

const studiedCourseIds = computed(() => {
  const ids = new Set<number>();

  for (const course of userCourses.value ?? []) {
    const id = Number(course.courseId);
    if (!Number.isFinite(id)) continue;
    if (!studiedStatusSet.has(course.status ?? "")) continue;
    ids.add(id);
  }

  return ids;
});

const enrolledCourseIds = computed(() => {
  const ids = new Set<number>();

  for (const course of userCourses.value ?? []) {
    const id = Number(course.courseId);
    if (!Number.isFinite(id)) continue;
    if ((course.status ?? "") !== "Enrolled") continue;
    ids.add(id);
  }

  return ids;
});

const isCurrentTerm = (year?: number | string, termId?: number | string) => {
  return Number(year) === Number(schedule.value?.currentStudyYear) && Number(termId) === Number(schedule.value?.currentTermId);
};

const isStudiedCourse = (item: components["schemas"]["SuggestedScheduleItemDto"]) => {
  if (item.courseId == null) return false;
  return studiedCourseIds.value.has(Number(item.courseId));
};

const isEnrolledCourse = (item: components["schemas"]["SuggestedScheduleItemDto"]) => {
  if (item.courseId == null) return false;
  return enrolledCourseIds.value.has(Number(item.courseId));
};

const getItemLabel = (item: components["schemas"]["SuggestedScheduleItemDto"]) => {
  if (item.isFreeElective) {
    const credits = Number(item.credits ?? 0);
    const creditsLabel = Number.isFinite(credits) && credits > 0 ? ` (${credits} credits)` : "";
    return `Free Elective Slot${creditsLabel}`;
  }

  if (item.isCoreElective) {
    const credits = Number(item.credits ?? 0);
    const creditsLabel = Number.isFinite(credits) && credits > 0 ? ` (${credits} credits)` : "";
    return `Core Elective Slot${creditsLabel}`;
  }

  const code = `${item.courseCode ?? ""}${item.courseNumber ?? ""}`.trim();
  if (!code && !item.courseName) {
    return "Course";
  }

  return `${code}${code && item.courseName ? " - " : ""}${item.courseName ?? ""}`.trim();
};

const getItemCredit = (item: components["schemas"]["SuggestedScheduleItemDto"]) => {
  if (item.isFreeElective || item.isCoreElective) {
    return item.credits ?? "-";
  }

  return item.courseCredit ?? "-";
};
</script>

<template>
  <div class="flex-1 p-6">
    <div class="flex flex-col gap-2">
      <h1 class="text-2xl font-bold">Suggested Schedule</h1>
      <p class="text-muted-foreground">
        Recommended study plan from your programme, grouped by year and term.
      </p>
    </div>

    <div class="mt-6 space-y-4">
      <template v-if="status === 'pending'">
        <Card>
          <CardContent class="py-8 text-sm text-muted-foreground">
            Loading suggested schedule...
          </CardContent>
        </Card>
      </template>

      <template v-else-if="!hasSchedule">
        <Card>
          <CardContent class="py-8 text-sm text-muted-foreground">
            No suggested schedule available for your account.
          </CardContent>
        </Card>
      </template>

      <template v-else>
        <Card v-for="year in schedule?.years ?? []" :key="Number(year.studyYear)">
          <CardHeader>
            <CardTitle>Study Year {{ year.studyYear }}</CardTitle>
            <CardDescription>
              {{ (year.terms ?? []).length }} term(s)
            </CardDescription>
          </CardHeader>
          <CardContent class="space-y-4">
            <div v-for="term in year.terms ?? []" :key="`${year.studyYear}-${term.termId}`" class="rounded-md border">
              <div class="border-b bg-muted/40 px-4 py-2 flex items-center justify-between gap-3">
                <h3 class="font-medium">{{ term.termName }}</h3>
                <Badge v-if="isCurrentTerm(year.studyYear, term.termId)">
                  Current
                </Badge>
              </div>

              <div class="overflow-x-auto">
                <table class="w-full min-w-[520px] text-sm">
                  <thead class="text-left text-muted-foreground">
                    <tr class="border-b">
                      <th class="px-4 py-2 font-medium">Item</th>
                      <th class="px-4 py-2 font-medium">Type</th>
                      <th class="px-4 py-2 font-medium">Progress</th>
                      <th class="px-4 py-2 font-medium">Credits</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr
                      v-for="item in term.items ?? []"
                      :key="Number(item.id)"
                      class="border-b last:border-b-0"
                    >
                      <td class="px-4 py-3">{{ getItemLabel(item) }}</td>
                      <td class="px-4 py-3">
                        <Badge v-if="item.isFreeElective" variant="secondary">Free Elective</Badge>
                        <Badge v-else-if="item.isCoreElective" variant="secondary">Core Elective</Badge>
                        <Badge v-else variant="outline">Course</Badge>
                      </td>
                      <td class="px-4 py-3">
                        <Badge v-if="isStudiedCourse(item)" variant="default">Studied</Badge>
                        <Badge v-else-if="isEnrolledCourse(item)" variant="secondary">Enrolled</Badge>
                        <span v-else class="text-muted-foreground">-</span>
                      </td>
                      <td class="px-4 py-3">{{ getItemCredit(item) }}</td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
          </CardContent>
        </Card>
      </template>
    </div>
  </div>
</template>
