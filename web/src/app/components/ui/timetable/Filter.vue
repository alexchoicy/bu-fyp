<script setup lang="ts">
import { computed } from 'vue';
import { AvailableTerm, AvailableTermLabels } from '~/types/static/facts';
import type { paths } from '~/API/schema';

const query = defineModel<paths['/api/timetable']['get']['parameters']['query']>('query', { default: () => ({}) });

const open = defineModel<boolean>('open', { default: false });

defineProps<{
    refresh?: () => void;
}>();

const { data: categoryOptions } = useAPI<paths['/api/me/category-groups']['get']['responses']['200']['content']['application/json']>('/me/category-groups');
const { data: currentFacts } = useAPI<paths['/api/facts']['get']['responses']['200']['content']['application/json']>('/facts');

const academicYearOptions = computed(() => {
    const currentYear = new Date().getFullYear();
    const startYears = [currentYear, currentYear + 1];

    return startYears.map((year) => ({
        value: year,
        label: `${year}-${year + 1}`,
    }));
});

const termOptions = computed(() =>
    Object.values(AvailableTerm)
        .filter((value): value is AvailableTerm => typeof value === 'number')
        .map((term) => ({
            value: term,
            label: AvailableTermLabels[term],
        })),
);

const yearPlaceholder = computed(() => {
    const start = currentFacts.value?.currentAcademicYear;
    if (start == null) return 'Select Academic Year';
    const yearNum = Number(start);
    return `${yearNum}-${yearNum + 1}`;
});

const termPlaceholder = computed(() => {
    const termId = currentFacts.value?.currentSemester;
    if (termId == null) return 'Select Semester';
    const label = AvailableTermLabels[Number(termId) as AvailableTerm];
    return label ?? 'Select Semester';
});

const resetFilters = () => {
    Object.assign(query.value, {
        year: undefined,
        termId: undefined,
        courseGroupId: undefined,
        categoryGroupId: undefined,
    });
};

</script>

<template>
    <Dialog v-model:open="open">
        <DialogContent>
            <DialogHeader>
                <DialogTitle>TimeTable Filter</DialogTitle>
            </DialogHeader>
            <div class="space-y-4 py-4">
                <div class="space-y-2">
                    <Label>Academic Year</Label>
                    <Select v-model="query.year">
                        <SelectTrigger class="w-full">
                            <SelectValue :placeholder="yearPlaceholder" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem :value="undefined">All Academic Years</SelectItem>
                            <SelectItem v-for="option in academicYearOptions" :key="option.value" :value="option.value">
                                {{ option.label }}
                            </SelectItem>
                        </SelectContent>
                    </Select>
                </div>

                <div class="space-y-2">
                    <Label>Semester</Label>
                    <Select v-model="query.termId">
                        <SelectTrigger class="w-full">
                            <SelectValue :placeholder="termPlaceholder" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem v-for="option in termOptions" :key="option.value" :value="option.value">
                                {{ option.label }}
                            </SelectItem>
                        </SelectContent>
                    </Select>
                </div>

                <div class="space-y-2">
                    <Label>Course Group</Label>
                    <Select v-model="query.courseGroupId">
                        <SelectTrigger class="w-full">
                            <SelectValue placeholder="Select Course Group" />
                        </SelectTrigger>
                        <SelectContent>
                            <SelectItem :value="undefined">All Course Groups</SelectItem>
                            <SelectGroup v-for="category in categoryOptions ?? []" :key="category.categoryId">
                                <SelectLabel>{{ category.categoryName }}</SelectLabel>
                                <SelectItem v-for="group in category.courseGroups" :key="group.groupId"
                                    :value="group.groupId">
                                    {{ group.groupName }}
                                </SelectItem>
                            </SelectGroup>
                        </SelectContent>
                    </Select>
                </div>

                <div class="flex justify-end">
                    <Button variant="outline" size="sm" @click="resetFilters">Reset</Button>
                </div>
            </div>
        </DialogContent>
    </Dialog>
</template>