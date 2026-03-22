<script setup lang="ts">
import type { components } from '~/API/schema';

//bro wt are these
const props = defineProps<{
    //Display code based on this, and course Detail
    categoryGroup: components["schemas"]["CategoryGroupDetailDto"] | undefined,
    userStudies: components["schemas"]["UserCourseDto"][] | undefined,
    //display course
    userProgrammeDetail: components['schemas']['CategoryCompletionStatus'] | undefined,
}>();

const codes = computed(() => {
    return props.categoryGroup?.groupCourses?.filter((gc) => gc.code !== null)
})

const courses = computed(() => {
    return props.categoryGroup?.groupCourses?.filter((gc) => gc.course !== null)
})

const normalizeId = (value: number | string | null | undefined) => {
    const parsed = Number(value)
    return Number.isFinite(parsed) ? parsed : null
}

const findStudyById = (courseId: number | string | null | undefined) => {
    const normalizedCourseId = normalizeId(courseId)

    if (normalizedCourseId === null) {
        return null
    }

    return props.userStudies?.find((study) => normalizeId(study.courseId) === normalizedCourseId) ?? null
}

const freeElectiveStudies = computed(() => {
    const usedCourseIds = new Set(
        (props.userProgrammeDetail?.usedCourseIds ?? [])
            .map(id => normalizeId(id))
            .filter((id): id is number => id !== null)
    )

    return [...usedCourseIds]
        .map(id => findStudyById(id))
        .filter((study): study is NonNullable<typeof study> => study !== null)
})

</script>

<template>
    <Card>
        <CardHeader>
            <CardTitle>
                {{ categoryGroup?.groupName }}
            </CardTitle>
        </CardHeader>
        <CardContent class="space-y-4">
            <div v-if="codes?.length > 0" class="space-y-2">
                <p class="text-xs font-medium text-muted-foreground uppercase tracking-wide">Accepted Course Codes</p>
                <div class="flex flex-warp gap-2">
                    <div class="inline-flex items-center gap-2 p-3 rounded-lg border" v-for="code in codes"
                        :key="code?.code?.codeId">
                        <Badge class="p-1">{{ code.code?.tag }}</Badge>
                        <span class="text-sm text-muted-foreground">Any {{ code.code.name }} course</span>
                    </div>
                    <Button as-child variant="ghost" size="sm">
                        <NuxtLink to="/courses">
                            Browse
                        </NuxtLink>
                    </Button>
                </div>
            </div>
            <div v-if="courses?.length > 0 && userProgrammeDetail?.ruleNode.type !== 'free_elective'" class="space-y-2">
                <p class="text-xs font-medium text-muted-foreground uppercase tracking-wide">Courses</p>
                <div className="grid gap-2 sm:grid-cols-2">
                    <UiProgrammeCourseCard v-for="course in courses" :key="course?.course?.courseId"
                        :course-info="course.course" :study-history="findStudyById(course?.course?.courseId)"
                        :user-programme-item="userProgrammeDetail?.items?.find((item) => course.course?.courseId === item.courseID)" />
                </div>
            </div>
            <div v-else-if="userProgrammeDetail?.ruleNode.type === 'free_elective'" class="space-y-2">
                <p class="text-xs font-medium text-muted-foreground uppercase tracking-wide">Courses has Finished</p>
                <div class="flex flex-col gap-2">
                    <UiProgrammeCourseCard v-for="study in freeElectiveStudies" :key="study.courseId" :course-info="{
                        courseId: study.courseId,
                        name: study.courseName || 'Unknown Course',
                        codeTag: study.codeTag || '',
                        courseNumber: study.courseNumber || '',
                        credit: study.credit || 0
                    }" :study-history="study" :user-programme-item="{
                        groupCourseID: 0,
                        courseID: study.courseId,
                        codeID: null,
                        isCompleted: true,
                        creditsUsed: study.credit || 0,
                    }" />
                </div>
            </div>
        </CardContent>
    </Card>
</template>
<!-- just ignore the UiProgrammeCourseCard for free_elective bro LMAO -->
