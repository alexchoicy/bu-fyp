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

const FindStudyById = (courseId: string) => {
    return props.userStudies?.find((study) => study.courseId === courseId)
}

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
                </div>
            </div>
            <div v-if="courses?.length > 0 && userProgrammeDetail?.ruleNode.type !== 'free_elective'" class="space-y-2">
                <p class="text-xs font-medium text-muted-foreground uppercase tracking-wide">Courses</p>
                <div className="grid gap-2 sm:grid-cols-2">
                    <UiProgrammeCourseCard v-for="course in courses" :key="course?.course?.courseId"
                        :course-info="course.course"
                        :study-history="userStudies?.find(us => us.courseId === course?.course?.courseId) || null"
                        :user-programme-item="userProgrammeDetail?.items?.find((item) => course.course?.courseId === item.courseID)" />
                </div>
            </div>
            <div v-else-if="userProgrammeDetail?.ruleNode.type === 'free_elective'" class="space-y-2">
                <p class="text-xs font-medium text-muted-foreground uppercase tracking-wide">Courses has Finished</p>
                <div class="flex flex-col gap-2">
                    <UiProgrammeCourseCard v-for="ids in userProgrammeDetail.usedCourseIds" :key="ids" :course-info="{
                        courseId: ids,
                        name: FindStudyById(ids)?.courseName || 'Unknown Course',
                        codeTag: FindStudyById(ids)?.codeTag || '',
                        courseNumber: FindStudyById(ids)?.courseNumber || '',
                        credit: FindStudyById(ids)?.credit || 0
                    }" :study-history="FindStudyById(ids)" :user-programme-item="{
                        groupCourseID: 0,
                        courseID: ids,
                        codeID: null,
                        isCompleted: true,
                        creditsUsed: FindStudyById(ids)?.credit || 0,
                    }" />
                </div>
            </div>
        </CardContent>
    </Card>
</template>
<!-- just ignore the UiProgrammeCourseCard for free_elective bro LMAO -->