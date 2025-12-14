<script setup lang="ts">
import type { components } from '~/API/schema';

const parseDialogOpen = ref(false);

const { data: courses } = await useAPI<components["schemas"]["SimpleCourseDto"][]>('/courses/simple');

const records = ref<components["schemas"]["CreateStudentCourseDto"][]>([]);

const availableCourses = ref<components["schemas"]["SimpleCourseDto"][]>([]);

watch(records, (newRecords) => {
    const selectedCourseIds = newRecords.map(record => record.courseId);
    availableCourses.value = (courses.value ?? []).filter(course => !selectedCourseIds.includes(course.id));
});

</script>

<template>
    <div class="flex flex-1 flex-col gap-6 p-6">

        <div class="flex items-center justify-between">
            <div>
                <h1 class="text-3xl font-bold tracking-tight">Create Academic Records</h1>
                <p class="text-muted-foreground mt-1">Add multiple course records to your academic history</p>
            </div>
            <div class="flex items-center gap-2">
                <Button variant="outline" @click="parseDialogOpen = true">Import from Grade Record</Button>
            </div>
        </div>


        <UiMeCourseRecordParseDialog v-model:open="parseDialogOpen" v-model:records="records"
            :courses="courses ?? []" />
    </div>
</template>
