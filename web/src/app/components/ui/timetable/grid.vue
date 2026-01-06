<script setup lang="ts">
import type { components } from '~/API/schema';

type Section = components['schemas']['TimetableSectionDto']
type Course = components["schemas"]["TimetableEntryDto"]

interface BlockTimeItem {
    id: string;
    startTime: number;
    endTime: number;
}

type BlockTimes = Record<number, BlockTimeItem[]>;

interface CoursePanelProps {
    courses: Course[];
    selectedSections: Section[];
    blockTimes: BlockTimes;
}

interface CourseGridEmits {
    (e: "removeSection", sectionId: number): void;
    (e: "deleteBlockTime", day: number, itemId: string): void;
}

const emit = defineEmits<CourseGridEmits>();



const props = defineProps<CoursePanelProps>();


const dayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];

const meetingsByDay = computed(() => {
    const map: Record<number, components['schemas']['TimetableMeetingDto'][]> = {};
    props.selectedSections.forEach((section) => {
        section.meetings?.forEach((meeting) => {
            if (!map[meeting.day]) {
                map[meeting.day] = [];
            }
            map[meeting.day].push(meeting);
        });
    });
    return map;
});


const START_HOUR = 8;
const END_HOUR = 23;
const BLOCK_HEIGHT = 50;
const HOUR_HEIGHT = BLOCK_HEIGHT * 2;

const totalHeight = (END_HOUR - START_HOUR) * HOUR_HEIGHT;

const getMeetingStyle = (meeting: components['schemas']['TimetableMeetingDto']) => {
    const startParts = meeting.startTime.split(':').map(Number);
    const endParts = meeting.endTime.split(':').map(Number);

    const startInMinutes = (startParts[0] - START_HOUR) * 60 + startParts[1];
    const endInMinutes = (endParts[0] - START_HOUR) * 60 + endParts[1];

    const top = (startInMinutes / 60) * HOUR_HEIGHT;
    const height = ((endInMinutes - startInMinutes) / 60) * HOUR_HEIGHT;

    return {
        top: `${top}px`,
        height: `${height}px`,
    };
};


const getCourseContext = (meetingID: number) => {
    for (const course of props.courses) {
        const section = course.sections!.find((s) => {
            return s.meetings?.some((m) => m.id === meetingID);
        });
        if (section) {
            return { course, section };
        }
    }
    return null;
};

</script>

<template>
    <!-- i think grid is possible, but later -->
    <div class="flex flex-col h-full w-full overflow-auto">
        <div class="flex border-b border-border sticky top-0 bg-background z-10">
            <div class="w-16 shrink-0 p-2 text-xs text-center font-medium text-muted-foreground border-r border-border">
                Time </div>
            <div class="grid grid-cols-6 w-full">
                <div v-for="day in dayNames" :key="day"
                    class="p-2 text-center text-sm font-medium border-r border-border last:border-r-0"> {{ day }} </div>
            </div>
        </div>
        <div>
            <div :style="{ height: totalHeight + 'px' }">
                <div class="flex">
                    <div class="w-16 shrink-0 border-r border-border relative">
                        <div v-for="h in END_HOUR - START_HOUR" :key="h" :style="{
                            top: ((h - 1) * HOUR_HEIGHT) + 'px',
                            height: HOUR_HEIGHT + 'px'
                        }"
                            class="absolute left-0 w-full border-r border-b border-border flex items-start justify-center pt-1 text-xs text-muted-foreground">
                            {{ (START_HOUR + h - 1).toString().padStart(2, '0') }}:00
                        </div>
                    </div>
                    <div class="grid grid-cols-6 w-full">
                        <div v-for="dayIndex in 6" :key="dayIndex"
                            class="relative border-r border-border last:border-r-0">
                            <!-- grid lines -->
                            <div class="absolute inset-0 pointer-events-none">
                                <div v-for="h in END_HOUR - START_HOUR" :key="h"
                                    :style="{ top: ((h - 1) * HOUR_HEIGHT) + 'px', height: HOUR_HEIGHT + 'px' }"
                                    class="absolute left-0 right-0 border-b border-border border-r" />
                            </div>

                            <!-- meetings -->
                            <div v-for="meeting in meetingsByDay[dayIndex - 1] || []" :key="meeting.id"
                                :style="getMeetingStyle(meeting)"
                                @click="emit('removeSection', getCourseContext(meeting.id)?.section.sectionId!)"
                                class="absolute left-1 right-1 bg-primary/20 border border-primary rounded p-1 text-xs text-primary overflow-hidden hover:bg-primary/30 cursor-pointer">

                                <div>{{ getCourseContext(meeting.id)?.course.courseName }}</div>
                                <div>{{ meeting.startTime }} - {{ meeting.endTime }}</div>
                            </div>

                            <div v-for="blocktime in blockTimes[dayIndex - 1] || []" :key="blocktime.id"
                                :style="getMeetingStyle({ startTime: blocktime.startTime.toString().padStart(2, '0') + ':30', endTime: blocktime.endTime.toString().padStart(2, '0') + ':20' })"
                                @click="emit('deleteBlockTime', dayIndex - 1, blocktime.id)"
                                class="absolute left-1 right-1 bg-black border border-primary rounded p-1 text-xs text-primary overflow-hidden hover:bg-primary/30 cursor-pointer">
                                <div class=" text-white">
                                    BLOCKED
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>