<script setup lang="ts">
import { computed } from 'vue'
import type { components } from '~/API/schema'

type AcademicProgressDto = components['schemas']['AcademicProgressDto']
type CurrentFactsResponseDto = components['schemas']['CurrentFactsResponseDto']
type UserCourseDto = components['schemas']['UserCourseDto']

const { data: dashboardData } = useAPI<AcademicProgressDto>('/me/academic-progress')
const { data: facts } = useAPI<CurrentFactsResponseDto>('/facts')
const { data: studentCourses } = useAPI<UserCourseDto[]>('/me/courses')
const { user } = useAuth()

const toNumber = (value: number | string | null | undefined) => {
  if (typeof value === 'number') {
    return Number.isFinite(value) ? value : 0
  }

  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : 0
}

const formatDecimal = (value: number, digits = 2) => value.toFixed(digits)

const overallGpa = computed(() => toNumber(dashboardData.value?.overallGpa))
const completedCredits = computed(() => toNumber(dashboardData.value?.totalCreditsCompleted))
const totalCreditsRequired = computed(() => toNumber(dashboardData.value?.totalCreditsRequired))
const completionPercentage = computed(() => toNumber(dashboardData.value?.completionPercentage))
const enrolledCoursesCount = computed(() => toNumber(dashboardData.value?.enrolledCoursesCount))
const remainingCredits = computed(() => toNumber(dashboardData.value?.remainingCredits))
const programmeName = computed(() => {
  const name = dashboardData.value?.programmeName?.trim()
  return name || 'Track your programme progress in one place.'
})

const currentAcademicYear = computed(() => toNumber(facts.value?.currentAcademicYear))
const academicYearLabel = computed(() => {
  if (!currentAcademicYear.value) {
    return 'Academic year unavailable'
  }

  return `${currentAcademicYear.value} - ${currentAcademicYear.value + 1}`
})

const currentTermLabel = computed(() => {
  const termName = facts.value?.currentTermName?.trim()

  if (termName) {
    return `${termName}, ${academicYearLabel.value}`
  }

  return academicYearLabel.value
})
</script>

<template>
  <div class="container mx-auto space-y-6 p-4">
    <div class="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
      <div class="space-y-1">
        <h1 class="text-3xl font-bold text-balance">Welcome back, {{ user?.name }}</h1>
        <p class="text-sm text-muted-foreground">
          {{ programmeName }}
        </p>
      </div>
      <div class="text-left md:text-right">
        <p class="text-sm text-muted-foreground">Current semester</p>
        <p class="font-semibold">
          {{ currentTermLabel }}
        </p>
      </div>
    </div>

    <div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
      <Card>
        <CardHeader class="space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Cumulative GPA</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-chart-1">{{ formatDecimal(overallGpa) }}</div>
          <p class="text-xs text-muted-foreground">
            Across all GPA-eligible completed courses
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Credits completed</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold text-chart-2">
            {{ completedCredits }}/{{ totalCreditsRequired }}
          </div>
          <Progress :model-value="completionPercentage" class="mt-2" />
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Degree progress</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ formatDecimal(completionPercentage) }}%</div>
          <p class="text-xs text-muted-foreground">
            {{ remainingCredits }} credits remaining to finish the programme
          </p>
        </CardContent>
      </Card>

      <Card>
        <CardHeader class="space-y-0 pb-2">
          <CardTitle class="text-sm font-medium">Active courses</CardTitle>
        </CardHeader>
        <CardContent>
          <div class="text-2xl font-bold">{{ enrolledCoursesCount }}</div>
          <p class="text-xs text-muted-foreground">
            Currently enrolled this semester
          </p>
        </CardContent>
      </Card>
    </div>

    <div class="grid grid-cols-1 gap-6 lg:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle class="flex items-center gap-2">
            GPA trend
          </CardTitle>
          <CardDescription>Semester GPA for each completed term</CardDescription>
        </CardHeader>
        <CardContent>
          <ClientOnly>
            <UiDashboardGpaLine :gpa-data="dashboardData?.semesterGpas ?? []" />
          </ClientOnly>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle class="flex items-center gap-2">
            Subject performance
          </CardTitle>
          <CardDescription>Weighted GPA by subject code from graded courses</CardDescription>
        </CardHeader>
        <CardContent>
          <ClientOnly>
            <UiDashboardStrengthChart :courses="studentCourses ?? []" />
          </ClientOnly>
        </CardContent>
      </Card>
    </div>
  </div>
</template>
