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

const courseById = (id?: number) => (courses.value ?? []).find(c => c.id === id);

function submitRecords() {
    useNuxtApp().$api('/me/courses', {
        method: 'POST',
        body: records.value,
    }).then(() => {
        records.value = [];
    }).finally(() => {
        navigateTo('/user/course');
    });
}

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

        <div class="space-y-4">
            <h2 class="text-xl font-semibold">Parsed Items</h2>
            <div v-if="records.length === 0" class="text-muted-foreground">No items parsed yet.</div>
            <div v-else class="rounded-md border">
                <div class="grid grid-cols-12 gap-2 p-3 text-sm font-medium bg-muted/50">
                    <div class="col-span-4">Course</div>
                    <div class="col-span-3">Code</div>
                    <div class="col-span-3">Grade</div>
                    <div class="col-span-2" />
                </div>
                <div class="divide-y">
                    <div v-for="(rec, idx) in records" :key="idx" class="grid grid-cols-12 gap-2 p-3 items-center">
                        <div class="col-span-4">
                            {{ courseById(rec.courseId)?.name ?? 'Unknown course' }}
                        </div>
                        <div class="col-span-3">
                            {{ courseById(rec.courseId)?.codeTag ?? '-' }} {{ courseById(rec.courseId)?.courseNumber ??
                                '-' }}
                        </div>
                        <div class="col-span-3">
                            {{ rec.grade ?? rec.status }}
                        </div>
                        <div class="col-span-2 flex justify-end">
                            <Button variant="outline" size="sm" @click="records.splice(idx, 1)">Remove</Button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <div class="mt-6 flex justify-end">
            <Button :disabled="records.length === 0" @click="submitRecords()">Submit Records</Button>
        </div>
    </div>
</template>
