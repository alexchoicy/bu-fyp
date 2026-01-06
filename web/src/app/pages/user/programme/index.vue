<script setup lang="ts">
import { computed } from 'vue'
import type { components } from '~/API/schema';


const { data: programme } = useAPI<components['schemas']['UserProgrammeDetailDto']>('me/programme')

const { data: userProgrammeDetail } = useAPI<components['schemas']['CategoryCompletionStatus'][]>('me/check')

const { data: userStudies } = useAPI<components["schemas"]["UserCourseDto"][]>('me/courses')

const sortedCategories = computed(() => {
    const categories = programme.value?.categories ?? []
    const safeNumber = (value?: number | string | null) => {
        const num = Number(value)
        return Number.isFinite(num) ? num : Number.POSITIVE_INFINITY
    }

    return [...categories].sort((a, b) => safeNumber(b?.priority) - safeNumber(a?.priority))
})

</script>

<template>
    <div class="flex flex-col  py-8 px-4">
        <div class="space-y-6">
            <UiProgrammeDetailCard :user-studies="userStudies" :programme="programme"
                :user-programme-detail="userProgrammeDetail" />
            <Separator />
            <div class="space-y-4">
                <h2 className="text-xl font-semibold">Programme Categories</h2>
                <UiProgrammeCategoryCard v-for="category in sortedCategories" :key="category?.categoryId"
                    :programme-category="category"
                    :user-programme-detail="userProgrammeDetail?.find(cp => cp.id === category?.categoryId)"
                    :user-studies="userStudies" />
            </div>
        </div>
    </div>
</template>
