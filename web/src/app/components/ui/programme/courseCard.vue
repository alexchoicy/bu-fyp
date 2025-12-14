<script setup lang="ts">
import type { components } from '~/API/schema';


const props = defineProps<{
    studyHistory: components["schemas"]["UserCourseDto"] | null
    courseInfo: components["schemas"]["SimpleGroupCourseDto"] | undefined | null
    userProgrammeItem: components['schemas']['ItemCompletionStatus'] | undefined | null
}>()

const isCompleted = computed(() => {
    return props.userProgrammeItem?.isCompleted || false
})

</script>

<template>
    <Card class="hover:shadow-md transition-shadow"
        :class="isCompleted ? 'border-green-500/50 bg-green-50/30 dark:bg-green-950/20' : ''">
        <CardContent class="space-y-2">
            <div class="space-x-2">
                <Badge variant="secondary">
                    {{ courseInfo?.codeTag }} {{ courseInfo?.courseNumber }}
                </Badge>
                <Badge variant="outline">
                    {{ courseInfo?.credit }} Credits
                </Badge>
                <Badge v-if="isCompleted" class="bg-green-500 text-white">
                    Completed
                </Badge>
            </div>
            <h4 class="font-medium text-sm">{{ courseInfo?.name }}</h4>
        </CardContent>
    </Card>
</template>
