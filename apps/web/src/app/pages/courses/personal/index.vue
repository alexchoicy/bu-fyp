<script setup lang="ts">
  import type { StudentCourse } from "@fyp/api/student/types";
  import { BookOpen, CheckCircle2, TrendingUp } from "lucide-vue-next";
  import TabsList from "~/components/ui/tabs/TabsList.vue";
  const user = useAuthUser();

  const { data } = await useAPI<StudentCourse[]>(`users/${user.value?.uid}/courses`, {});

  const activeTab = ref<"all" | "enrolled" | "completed" | "planned">("all");

  const counts = computed(() => {
    const list = data?.value ?? [];
    return {
      all: list.length,
      enrolled: list.filter((c) => c?.studentCourseStatus === "enrolled").length,
      completed: list.filter((c) => c?.studentCourseStatus === "completed").length,
      planned: list.filter((c) => c?.studentCourseStatus === "planned").length,
    };
  });

  const filteredCoursesRecord = computed(() => {
    const list = data?.value ?? [];
    if (activeTab.value === "all") return list;
    return list.filter((c) => c?.studentCourseStatus === activeTab.value);
  });
</script>

<template>
  <div class="flex flex-col gap-6 p-6">
    <div class="flex flex-col gap-2">
      <h2 class="text-2xl font-bold text-balance">My Academic Progress</h2>
      <p class="text-muted-foreground">
        Manage your course records, add historical grades, track ongoing courses, and plan future enrollments.
      </p>
    </div>
    <div class="grid gap-4 md:grid-cols-3">
      <Card>
        <CardHeader class="flex flex-row items-center justify-between pb-2">
          <CardTitle class="text-sm font-medium">Current GPA</CardTitle>
          <TrendingUp class="h-4 w-4 text-accent" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{currentGPA}</div>
          <p class="text-xs text-muted-foreground">Out of 4.0</p>
        </CardContent>
      </Card>
      <Card>
        <CardHeader class="flex flex-row items-center justify-between pb-2">
          <CardTitle class="text-sm font-medium">Credits Earned</CardTitle>
          <CheckCircle2 class="h-4 w-4 text-chart-3" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{totalCredits}/{requiredCredits}</div>
          <Progress />
        </CardContent>
      </Card>
      <Card>
        <CardHeader class="flex flex-row items-center justify-between pb-2">
          <CardTitle class="text-sm font-medium">Current Enrollment</CardTitle>
          <BookOpen class="h-4 w-4 text-primary" />
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{enrolledCourses.length} Courses</div>
          <p class="text-xs text-muted-foreground">Fall 2025 semester</p>
        </CardContent>
      </Card>
    </div>

    <Tabs v-model="activeTab" class="w-full">
      <div class="flex justify-between">
        <TabsList>
          <TabsTrigger value="all">All Courses ({{ counts.all }})</TabsTrigger>
          <TabsTrigger value="enrolled">Enrolled ({{ counts.enrolled }})</TabsTrigger>
          <TabsTrigger value="completed">Completed ({{ counts.completed }})</TabsTrigger>
          <TabsTrigger value="planned">Planned ({{ counts.planned }})</TabsTrigger>
        </TabsList>
        <Button @click="navigateTo('personal/create')">Add Course</Button>
      </div>
      <TabsContent value="all">
        <div class="grid gap-4 grid-cols-1">
          <CourseCard v-for="record in filteredCoursesRecord" :key="record.id" :record="record" />
        </div>
      </TabsContent>
      <TabsContent value="enrolled">
        <div class="grid gap-4 grid-cols-1">
          <CourseCard v-for="record in filteredCoursesRecord" :key="record.id" :record="record" />
        </div>
      </TabsContent>
      <TabsContent value="completed">
        <div class="grid gap-4 grid-cols-1">
          <CourseCard v-for="record in filteredCoursesRecord" :key="record.id" :record="record" />
        </div>
      </TabsContent>
      <TabsContent value="planned">
        <div class="grid gap-4 grid-cols-1">
          <CourseCard v-for="record in filteredCoursesRecord" :key="record.id" :record="record" />
        </div>
      </TabsContent>
    </Tabs>
  </div>
</template>
