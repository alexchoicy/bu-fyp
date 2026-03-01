<script setup lang="ts">
import { Calendar, ChevronLeft, Loader2 } from 'lucide-vue-next';
import type { components } from '~/API/schema';
import GenerationPreferencesForm from '~/components/ui/timetable/GenerationPreferencesForm.vue';
import { base64UrlEncodeString } from '~/lib/base64utils';

type TimetableGenerationRequest = components["schemas"]["TimetableGenerationRequestDto"];
type TimetableSuggestionsResponse = components["schemas"]["TimetableSuggestionsResponseDto"];
type TimetableResponse = components["schemas"]["TimetableResponseDto"];

const generationRequest = reactive<TimetableGenerationRequest>({
    scoring: {
        baseScore: 0,
        groupWeights: {
            schedule: 0.4,
            timePreference: 0.25,
            gap: 0.2,
            assessments: 0.15,
        },
        scheduleShape: {
            freeDayScore: {
                points: 0,
            },
            singleClassDayScore: {
                points: 0,
            },
            longDayScore: {
                points: 0,
                maxMinutesPerDay: 360,
            },
            dailyLoadScore: {
                points: 0,
                idealActiveDays: 4,
            },
        },
        preferenceShape: {
            startTimePreference: {
                preferredStartTime: '09:30:00',
                points: 0,
            },
            endTimePreference: {
                preferredEndTime: '18:20:00',
                points: 0,
            },
        },
        gapCompactnessShape: {
            shortGap: {
                points: 0,
                maxGapMinutes: 90,
                ignoreGapStartTime: '12:30:00',
                ignoreGapEndTime: '14:20:00',
            },
        },
        assessmentShape: {
            assessmentCategoryScores: [
                { category: 'Examination', points: 0 },
                { category: 'Assignment', points: 0 },
                { category: 'Project', points: 0 },
                { category: 'SoloProject', points: 0 },
                { category: 'Participation', points: 0 },
                { category: 'Presentation', points: 0 },
                { category: 'Other', points: 0 },
            ],
        },
    },
    filter: {
        noClassTime: [
        ],
    },
});

type Step = 'form' | 'results';

const currentStep = ref<Step>('form');
const isSubmitting = ref(false);

const apiErrors = ref<string[]>([]);

const isLoading = ref(false);
const requestError = ref<string | null>(null);
const suggestions = ref<TimetableSuggestionsResponse>({ recommendedLayouts: [] });

const submitGenerationRequest = async () => {
    if (isSubmitting.value) return;

    isSubmitting.value = true;
    requestError.value = null;

    try {
        const payload: TimetableGenerationRequest = structuredClone(toRaw(generationRequest));
        const response = await useNuxtApp().$api<TimetableSuggestionsResponse>('timetable/suggestions', {
            method: 'POST',
            body: payload,
        });

        apiErrors.value = response.errors ?? [];
        suggestions.value = response ?? { recommendedLayouts: [] };

        if (currentStep.value === 'form') {
            currentStep.value = 'results';
        }
    }
    catch (error: any) {
        console.error('Failed to submit timetable generation request:', error);
        requestError.value = error?.data?.message ?? 'Failed to generate timetable suggestions. Please try again.';
        apiErrors.value = [];
        suggestions.value = { recommendedLayouts: [] };
        if (currentStep.value === 'form') {
            currentStep.value = 'results';
        }
    }
    finally {
        isSubmitting.value = false;
    }
};

interface BlockTimeItem {
    id: string;
    startTime: number;
    endTime: number;
}

type BlockTimes = Record<number, BlockTimeItem[]>;

type EncodedTimetable = {
    sections: Section[];
    blockTimes: BlockTimes;
};
type Section = components["schemas"]["TimetableSectionDto"];
type TimetableSuggestionLayout = components["schemas"]["TimetableSuggestionLayoutDto"];
type TimetableDemoBadLayout = components["schemas"]["TimetableDemoBadLayoutDto"];

const openLayoutInTimetable = (layout: TimetableSuggestionLayout | TimetableDemoBadLayout) => {
    const payload: EncodedTimetable = {
        sections: (layout.sections ?? []) as Section[],
        blockTimes: {},
    };
    const encoded = base64UrlEncodeString(JSON.stringify(payload));
    const score = Number(layout.finalScore ?? 0).toFixed(2);

    navigateTo(
        {
            path: "/timetable",
            query: { tb: encoded, score },
        },
        {
            open: {
                target: "_blank",
            },
        },
    );
};

const { data: availableItems } = useAPI<TimetableResponse>("timetable");

const getSectionCourse = (sectionId?: number | string) => {
    if (sectionId == null) return null;
    return availableItems.value?.entries?.find((entry) =>
        entry.sections?.some((section) => section.sectionId === sectionId),
    );
};

const getSectionCourseLabel = (sectionId?: number | string) => {
    const course = getSectionCourse(sectionId);
    if (!course) return "Unknown course";
    return `${course.courseCode ?? ""} ${course.courseNumber ?? ""} ${course.courseName ?? ""}`.trim();
};
const dayNames = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];

const getDayName = (day?: number | string) => {
    const dayIndex = Number(day);
    if (!Number.isFinite(dayIndex) || dayIndex < 0 || dayIndex >= dayNames.length) {
        return "Unknown";
    }
    return dayNames[dayIndex];
};

const formatTime = (time?: string) => {
    if (!time) return "-";
    return time.slice(0, 5);
};

</script>


<template>
    <div class="mx-auto w-full max-w-4xl p-6 space-y-6">
        <div class="flex items-center justify-between border-b border-border pb-4">
            <div class="flex items-center gap-2">
                <Calendar class="h-5 w-5 text-primary" />
                <h1 class="text-xl font-semibold">Timetable Generator</h1>
            </div>
            <Badge variant="secondary">
                Step {{ currentStep === "form" ? "1 / 2" : "2 / 2" }}
            </Badge>
        </div>


        <GenerationPreferencesForm v-if="currentStep === 'form'" v-model:generation-request="generationRequest"
            :submitting="isSubmitting" @submit="submitGenerationRequest" />
        <Card v-else>
            <CardHeader class="space-y-4">
                <div class="flex items-center justify-between gap-3">
                    <div>
                        <CardTitle>Generated Suggestions</CardTitle>
                        <CardDescription>
                            Change preferences any time. Results auto-refresh while you stay on this page.
                        </CardDescription>
                    </div>
                    <Button variant="outline" @click="currentStep = 'form'">
                        <ChevronLeft class="mr-2 h-4 w-4" />
                        Previous
                    </Button>
                </div>
            </CardHeader>

            <CardContent class="space-y-3">
                <Alert v-if="requestError" variant="destructive">
                    <AlertTitle>Request Error</AlertTitle>
                    <AlertDescription>{{ requestError }}</AlertDescription>
                </Alert>

                <Alert v-if="apiErrors.length > 0" variant="destructive">
                    <AlertTitle>Generation Issues</AlertTitle>
                    <AlertDescription>
                        {{ apiErrors.join(" ") }}
                    </AlertDescription>
                </Alert>

                <div v-if="isLoading" class="flex items-center gap-2 text-sm text-muted-foreground">
                    <Loader2 class="h-4 w-4 animate-spin" />
                    Refreshing suggestions...
                </div>

                <div v-else-if="(suggestions.recommendedLayouts ?? []).length === 0 && !suggestions.demoBadLayout && !requestError"
                    class="rounded-md border border-dashed p-6 text-sm text-muted-foreground">
                    No suggestions found for current preferences.
                </div>

                <div v-elseif="!requestError" class="space-y-3">
                    <Accordion type="multiple" :default-value="['bad-layout', 'good-layouts']" class="space-y-3">
                        <AccordionItem value="good-layouts"
                            class="overflow-hidden rounded-md border border-green-200 bg-green-50/30 px-4">
                            <AccordionTrigger class="py-3 hover:no-underline">
                                <div class="flex w-full items-center justify-between gap-3 pr-2">
                                    <h3 class="font-medium text-green-700">😄 Recommended Good Layouts</h3>
                                    <Badge variant="secondary">
                                        {{ (suggestions.recommendedLayouts ?? []).length }} result(s)
                                    </Badge>
                                </div>
                            </AccordionTrigger>
                            <AccordionContent class="space-y-3 pb-4">
                                <div v-if="(suggestions.recommendedLayouts ?? []).length === 0"
                                    class="rounded-md border border-dashed border-green-300 p-4 text-sm text-green-700/90">
                                    No good layouts returned for the current preferences.
                                </div>

                                <div v-for="(layout, index) in suggestions.recommendedLayouts ?? []" :key="index"
                                    class="rounded-md border bg-background p-4 space-y-3">
                                    <div class="flex justify-between">
                                        <h3 class="font-medium">Layout {{ index + 1 }}</h3>
                                        <Button size="sm" @click="openLayoutInTimetable(layout)">
                                            Open in Timetable
                                        </Button>
                                    </div>
                                    <div class="overflow-x-auto rounded-md border">
                                        <table class="w-full min-w-[640px] text-sm">
                                            <thead class="bg-muted/40 text-left">
                                                <tr>
                                                    <th class="px-3 py-2 font-medium">Course</th>
                                                    <th class="px-3 py-2 font-medium">Section</th>
                                                    <th class="px-3 py-2 font-medium">Meetings</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr v-for="section in layout.sections ?? []" :key="section.sectionId"
                                                    class="border-t align-top">
                                                    <td class="px-3 py-2">
                                                        {{ getSectionCourseLabel(section.sectionId) }}
                                                    </td>
                                                    <td class="px-3 py-2 whitespace-nowrap">
                                                        {{ section.sectionNumber ?? "-" }}
                                                    </td>
                                                    <td class="px-3 py-2">
                                                        <ul class="space-y-1">
                                                            <li v-for="meeting in section.meetings ?? []"
                                                                :key="meeting.id">
                                                                Day: {{ getDayName(meeting.day) }} - {{
                                                                    formatTime(meeting.startTime) }} to {{
                                                                    formatTime(meeting.endTime)
                                                                }}
                                                            </li>
                                                        </ul>
                                                    </td>
                                                </tr>
                                            </tbody>
                                            <tfoot class="bg-muted/30">
                                                <tr class="border-t">
                                                    <td colspan="3" class="px-3 py-3">
                                                        <span class="font-semibold text-primary">
                                                            Score: {{ Number(layout.finalScore ?? 0).toFixed(2) }}
                                                        </span>
                                                        <span v-if="(layout.scoreReasons ?? []).length > 0"
                                                            class="text-xs text-muted-foreground">
                                                            <div v-for="(reason, idx) in layout.scoreReasons ?? []"
                                                                :key="idx">
                                                                {{ reason }}
                                                            </div>
                                                        </span>
                                                    </td>
                                                </tr>
                                            </tfoot>
                                        </table>
                                    </div>
                                </div>
                            </AccordionContent>
                        </AccordionItem>

                        <AccordionItem v-if="suggestions.demoBadLayout" value="bad-layout"
                            class="overflow-hidden rounded-md border border-red-200 bg-red-50/40 px-4">
                            <AccordionTrigger class="py-3 hover:no-underline">
                                <div class="flex w-full items-center justify-between gap-3 pr-2">
                                    <h3 class="font-medium text-red-700">😵 Bad Layout</h3>
                                </div>
                            </AccordionTrigger>
                            <AccordionContent class="space-y-3 pb-4">
                                <div class="flex items-center justify-between gap-3">
                                    <p class="text-sm text-red-700/90">
                                        This timetable intentionally scores poorly.
                                    </p>
                                    <Button size="sm" variant="outline"
                                        class="border-red-300 text-red-700 hover:bg-red-100"
                                        @click="openLayoutInTimetable(suggestions.demoBadLayout)">
                                        Open in Timetable
                                    </Button>
                                </div>

                                <div class="overflow-x-auto rounded-md border border-red-200">
                                    <table class="w-full min-w-[640px] text-sm">
                                        <thead class="bg-red-100/50 text-left">
                                            <tr>
                                                <th class="px-3 py-2 font-medium">Course</th>
                                                <th class="px-3 py-2 font-medium">Section</th>
                                                <th class="px-3 py-2 font-medium">Meetings</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr v-for="section in suggestions.demoBadLayout.sections ?? []"
                                                :key="section.sectionId" class="border-t border-red-100 align-top">
                                                <td class="px-3 py-2">
                                                    {{ getSectionCourseLabel(section.sectionId) }}
                                                </td>
                                                <td class="px-3 py-2 whitespace-nowrap">
                                                    {{ section.sectionNumber ?? "-" }}
                                                </td>
                                                <td class="px-3 py-2">
                                                    <ul class="space-y-1">
                                                        <li v-for="meeting in section.meetings ?? []" :key="meeting.id">
                                                            Day: {{ getDayName(meeting.day) }} - {{
                                                                formatTime(meeting.startTime) }} to {{
                                                                formatTime(meeting.endTime)
                                                            }}
                                                        </li>
                                                    </ul>
                                                </td>
                                            </tr>
                                        </tbody>
                                        <tfoot class="bg-red-100/40">
                                            <tr class="border-t border-red-200">
                                                <td colspan="3" class="px-3 py-3">
                                                    <span class="font-semibold text-red-700">
                                                        Score: {{ Number(suggestions.demoBadLayout.finalScore ??
                                                            0).toFixed(2)
                                                        }}
                                                    </span>
                                                    <span
                                                        v-if="(suggestions.demoBadLayout.scoreReasons ?? []).length > 0"
                                                        class="text-xs text-red-700/80">
                                                        <div v-for="(reason, idx) in suggestions.demoBadLayout.scoreReasons ?? []"
                                                            :key="idx">
                                                            {{ reason }}
                                                        </div>
                                                    </span>
                                                </td>
                                            </tr>
                                        </tfoot>
                                    </table>
                                </div>
                            </AccordionContent>
                        </AccordionItem>
                    </Accordion>

                </div>
            </CardContent>
        </Card>
    </div>
</template>
