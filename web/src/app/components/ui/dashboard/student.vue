<script setup lang="ts">
import type { components } from '~/API/schema';
import { BookOpen } from 'lucide-vue-next';
const { data: dashboardData } = useAPI<components['schemas']['AcademicProgressDto']>('/me/academic-progress');
const { user } = useAuth();

const { data: facts } = useAPI<components['schemas']['CurrentFactsResponseDto']>('facts');
</script>

<template>
    <div class="space-y-6 p-4 container mx-auto">
        <div class="flex items-center justify-between">
            <div>
                <h1 class="text-3xl font-bold text-balance">Welcome back! {{ user?.name }}</h1>
            </div>
            <div class="text-right">
                <p class="text-sm text-muted-foreground">Current Semester</p>
                <p class="font-semibold">
                    {{ facts?.currentTermName }}, {{ facts?.currentAcademicYear }} - {{ facts?.currentAcademicYear + 1
                    }}
                </p>
            </div>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <Card>
                <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle class="text-sm font-medium">Current GPA</CardTitle>
                </CardHeader>
                <CardContent>
                    <div class='text-2xl font-bold text-green-500'>{{ dashboardData?.overallGpa }}</div>

                </CardContent>
            </Card>
            <Card>
                <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle class="text-sm font-medium">Credits Earned</CardTitle>
                    <BookOpen class="h-4 w-4 text-chart-2" />
                </CardHeader>
                <CardContent>
                    <div class="text-2xl font-bold text-chart-2">
                        {{ dashboardData?.totalCreditsCompleted }}/{{ dashboardData?.totalCreditsRequired }}
                    </div>
                    <Progress :model-value="dashboardData?.completionPercentage ?? 0" class="mt-2" />
                </CardContent>
            </Card>

            <Card>
                <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle class="text-sm font-medium">Degree Progress</CardTitle>
                </CardHeader>
                <CardContent>
                    <div class="text-2xl font-bold">{{ dashboardData?.completionPercentage.toFixed(2) }}%</div>
                    <Progress :model-value="dashboardData?.completionPercentage ?? 0" class="mt-2" />
                </CardContent>
            </Card>
            <Card>
                <CardHeader class="flex flex-row items-center justify-between space-y-0 pb-2">
                    <CardTitle class="text-sm font-medium">Active Course</CardTitle>
                </CardHeader>
                <CardContent>
                    <div class="text-2xl font-bold">{{ dashboardData?.enrolledCoursesCount ?? 0 }}</div>
                    <p class="text-xs text-muted-foreground">
                        Ongoing this semester
                    </p>
                </CardContent>
            </Card>
        </div>
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
                <CardHeader>
                    <CardTitle class="flex items-center gap-2">
                        GPA Trend
                    </CardTitle>
                    <CardDescription>Your academic performance over time</CardDescription>
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
                        Course Strengths
                    </CardTitle>
                    <CardDescription>Performance across different subject areas</CardDescription>
                </CardHeader>
                <CardContent>
                    <!-- <StrengthRadar /> -->
                </CardContent>
            </Card>
        </div>
    </div>
</template>
