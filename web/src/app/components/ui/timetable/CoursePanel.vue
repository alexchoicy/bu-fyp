<script setup lang="ts">
import {
  ChevronDown,
  ChevronRight,
  Plus,
  Minus,
  Search,
  ChevronLeft,
} from "lucide-vue-next";

import type { components } from "~/API/schema";
type Course = components["schemas"]["TimetableEntryDto"]
type Section = components['schemas']['TimetableSectionDto']

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

interface CoursePanelEmits {
  (e: "addSection", section: Section): void;

  (e: "removeSection", sectionId: number): void;
}

const props = defineProps<CoursePanelProps>();
const emit = defineEmits<CoursePanelEmits>();

const searchQuery = ref("");
const expandedCourses = ref<Set<number>>(new Set());

const dayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat"];
const getDayName = (day: number) => dayNames[day] || "Mon";

const filteredCourses = computed(() => {
  if (!searchQuery.value.trim()) return props.courses;

  const query = searchQuery.value.toLowerCase();
  return props.courses.filter(
    (course) =>
      course.courseCode!.toLowerCase().includes(query) ||
      course.courseNumber!.toLowerCase().includes(query)
  );
});

const isSectionSelected = (sectionId: number) => {
  return props.selectedSections.some((s) => s.sectionId === sectionId);
};

const getSelectedSectionForCourse = (courseId: number) => {
  return props.selectedSections.find((selectedSection) => {
    return props.courses.some(
      (c) =>
        c.courseId === courseId &&
        c.sections!.some((s) => s.sectionId === selectedSection.sectionId)
    );
  });
};

const toggleCourse = (courseId: number) => {
  if (expandedCourses.value.has(courseId)) {
    expandedCourses.value.delete(courseId);
  } else {
    expandedCourses.value.add(courseId);
  }
};

const formatTime = (t?: string) => (t ? t.slice(0, 5) : "");

const toMinutes = (t?: string) => {
  if (!t) return NaN;
  const [hh, mm] = t.slice(0, 5).split(":").map(Number);
  return hh * 60 + mm;
};

const doesOverlap = (aStart?: string, aEnd?: string, bStart?: string, bEnd?: string) => {
  const s1 = toMinutes(aStart);
  const e1 = toMinutes(aEnd);
  const s2 = toMinutes(bStart);
  const e2 = toMinutes(bEnd);
  if ([s1, e1, s2, e2].some(Number.isNaN)) return false;
  return s1 < e2 && s2 < e1;
};

const hasConflict = (section: Section) => {
  // Compare this section's meetings to all meetings of selected sections
  const selectedMeetings = props.selectedSections.flatMap((s) => s.meetings ?? []);
  const currentMeetings = section.meetings ?? [];
  for (const m of currentMeetings) {
    for (const sm of selectedMeetings) {
      if (m.day === sm.day && doesOverlap(m.startTime, m.endTime, sm.startTime, sm.endTime)) {
        return true;
      }
    }
    const blocktimes = props.blockTimes[m.day] || [];
    for (const bt of blocktimes) {
      const btStart = bt.startTime.toString().padStart(2, '0') + ':30';
      const btEnd = bt.endTime.toString().padStart(2, '0') + ':20';
      if (doesOverlap(m.startTime, m.endTime, btStart, btEnd)) {
        return true;
      }
    }
  }
  return false;
};


</script>

<template>
  <div class="flex flex-col h-full">
    <div class="p-3 border-b border-border">
      <div class="relative">
        <Search class="absolute left-2.5 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
        <Input v-model="searchQuery" type="text" placeholder="Search by code or name..." class="pl-9" />
      </div>
    </div>
    <div class="flex-1 overflow-auto">
      <div v-if="filteredCourses.length === 0" class="p-4 text-center text-muted-foreground text-sm">
        No courses found matching "{{ searchQuery }}"
      </div>
      <div v-else class="divide-y divide-border">
        <div v-for="course in filteredCourses" :key="course.courseId" class="bg-background">
          <button class="w-full flex items-center gap-2 p-3 hover:bg-muted/50 transition-colors text-left"
            @click="toggleCourse(course.courseId)">
            <ChevronDown v-if="expandedCourses.has(course.courseId)" class="h-4 w-4 shrink-0 text-muted-foreground" />
            <ChevronRight v-else class="h-4 w-4 shrink-0 text-muted-foreground" />
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2">
                <span class="font-medium text-sm">{{ course.courseCode }}</span>
                <span v-if="getSelectedSectionForCourse(course.courseId)"
                  class="text-xs bg-primary text-primary-foreground px-1.5 py-0.5 rounded">
                  Section
                  {{
                    getSelectedSectionForCourse(course.courseId)?.sectionNumber
                  }}
                </span>
              </div>
              <div class="text-xs text-muted-foreground truncate">
                {{ course.courseName }}
              </div>
            </div>
          </button>
          <div v-if="expandedCourses.has(course.courseId)" class="bg-muted/30 border-t border-border">
            <div v-for="section in course.sections" :key="section.sectionId"
              class="flex items-center justify-between px-3 py-2 pl-9 border-b border-border last:border-b-0">
              <div class="flex-1 min-w-0">
                <div class="flex justify-between items-center object-center pb-2">
                  <div class="font-medium text-sm">
                    Section {{ section.sectionNumber }}
                  </div>
                  <Button v-if="isSectionSelected(section.sectionId)" size="sm" variant="destructive"
                    class="shrink-0 ml-2" @click="emit('removeSection', section.sectionId)">
                    <Minus class="h-4 w-4 mr-1" />
                    Remove
                  </Button>
                  <Button v-else size="sm" variant="secondary" class="shrink-0 ml-2" :disabled="hasConflict(section)"
                    @click="emit('addSection', section)">
                    <Plus class="h-4 w-4 mr-1" />
                    Add
                  </Button>
                </div>
                <div class="text-xs text-muted-foreground">
                  <div v-for="meeting in section.meetings" :key="meeting.id"
                    class="flex items-center gap-2 text-sm bg-background rounded border border-border px-2 py-1.5">
                    <Badge>
                      {{ meeting.meetingType }}
                    </Badge>
                    <span class="shrink-0 text-muted-foreground">
                      {{ getDayName(meeting.day) }}
                    </span>
                    <span class="shrink-0 text-muted-foreground">
                      {{ formatTime(meeting.startTime) }}-{{ formatTime(meeting.endTime) }}
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
