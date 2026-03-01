<script setup lang="ts">
import { ChevronRight, Filter } from 'lucide-vue-next';
import type { components } from '~/API/schema';
import { RadioGroup, RadioGroupItem } from '~/components/shadcn/radio-group';

type TimetableGenerationRequest = components["schemas"]["TimetableGenerationRequestDto"];
type TimetableNoClassTime = components["schemas"]["TimetableNoClassTimeDto"];
type AssessmentCategory = components["schemas"]["AssessmentCategory"];

interface ScoreOption {
    label: string;
    value: number;
}

const scoreOptions: ScoreOption[] = [
    { label: '-3', value: -3 },
    { label: '-2', value: -2 },
    { label: '-1', value: -1 },
    { label: '0', value: 0 },
    { label: '1', value: 1 },
    { label: '2', value: 2 },
    { label: '3', value: 3 },
];

const assessmentTypeOptions: { label: string; value: AssessmentCategory }[] = [
    { label: 'Examination', value: 'Examination' },
    { label: 'Assignment', value: 'Assignment' },
    { label: 'Project / Group Project', value: 'Project' },
    { label: 'Solo Project', value: 'SoloProject' },
    { label: 'Participation', value: 'Participation' },
    { label: 'Presentation', value: 'Presentation' },
    { label: 'Other', value: 'Other' },
];

interface BlockTimeItem {
    id: string;
    startTime: number;
    endTime: number;
}

type BlockTimes = Record<number, BlockTimeItem[]>;

const blockTimeDialogOpen = ref(false);
const blockTimes = ref<BlockTimes>({});

const generationRequest = defineModel<TimetableGenerationRequest>('generationRequest', {
    required: true,
});

const props = withDefaults(defineProps<{
    submitting?: boolean;
}>(), {
    submitting: false,
});

const emit = defineEmits<{
    submit: [];
}>();

const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

const toNumber = (value: string | number | undefined) => Number(value ?? 0);

const toScore = (value: string | number | undefined) => {
    const numeric = Number(value ?? 0);
    if (!Number.isFinite(numeric)) return 0;
    return Math.max(-3, Math.min(3, Math.round(numeric)));
};

const getHour = (time?: string | null) => {
    if (!time) return undefined;
    const hour = Number.parseInt(time.split(':')[0] ?? '', 10);
    return Number.isFinite(hour) ? hour : undefined;
};

const toNoClassTimes = (source: BlockTimes): TimetableNoClassTime[] => Object.entries(source)
    .flatMap(([day, items]) => items.map((item) => ({
        day: Number(day),
        start: `${item.startTime.toString().padStart(2, '0')}:30:00`,
        end: `${item.endTime.toString().padStart(2, '0')}:30:00`,
    })));

const padHour = (hour: number) => hour.toString().padStart(2, '0');

const startTimeOptions = Array.from({ length: 15 }, (_, index) => {
    const hour = index + 9;
    return {
        label: `${padHour(hour)}:30`,
        value: `${padHour(hour)}:30:00`,
    };
});

const endTimeOptions = Array.from({ length: 14 }, (_, index) => {
    const hour = index + 10;
    return {
        label: `${padHour(hour)}:20`,
        value: `${padHour(hour)}:20:00`,
    };
});

const normalizeAssessmentCategory = (category?: AssessmentCategory | number | string) => {
    if (!category) return 'Other' as AssessmentCategory;
    if (category === 'GroupProject') return 'Project' as AssessmentCategory;

    const numeric = Number(category);
    if (Number.isFinite(numeric)) {
        if (numeric === 1) return 'Examination';
        if (numeric === 2) return 'Assignment';
        if (numeric === 3) return 'Project';
        if (numeric === 5) return 'SoloProject';
        if (numeric === 6) return 'Participation';
        if (numeric === 7) return 'Presentation';
        if (numeric === 99) return 'Other';
    }

    return category as AssessmentCategory;
};

const formatAssessmentCategory = (category?: AssessmentCategory | number | string) => {
    const normalized = normalizeAssessmentCategory(category);
    if (normalized === 'Project') return 'Project / Group Project';
    return normalized;
};

const ensureRequestShape = () => {
    if (!generationRequest.value.filter) {
        generationRequest.value.filter = {};
    }

    const scoring = generationRequest.value.scoring;

    if (!scoring.groupWeights) {
        scoring.groupWeights = {
            schedule: 0.4,
            timePreference: 0.25,
            gap: 0.2,
            assessments: 0.15,
        };
    }

    if (!scoring.scheduleShape) {
        scoring.scheduleShape = {
            freeDayScore: {},
            singleClassDayScore: {},
            longDayScore: {},
            dailyLoadScore: {},
        };
    }
    if (!scoring.scheduleShape.freeDayScore) scoring.scheduleShape.freeDayScore = {};
    if (!scoring.scheduleShape.singleClassDayScore) scoring.scheduleShape.singleClassDayScore = {};
    if (!scoring.scheduleShape.longDayScore) scoring.scheduleShape.longDayScore = {};
    if (!scoring.scheduleShape.dailyLoadScore) scoring.scheduleShape.dailyLoadScore = {};

    scoring.scheduleShape.freeDayScore.points = toScore(scoring.scheduleShape.freeDayScore.points);
    scoring.scheduleShape.singleClassDayScore.points = toScore(scoring.scheduleShape.singleClassDayScore.points);
    scoring.scheduleShape.longDayScore.points = toScore(scoring.scheduleShape.longDayScore.points);
    scoring.scheduleShape.dailyLoadScore.points = toScore(scoring.scheduleShape.dailyLoadScore.points);

    if (scoring.scheduleShape.longDayScore.maxMinutesPerDay == null) {
        scoring.scheduleShape.longDayScore.maxMinutesPerDay = 360;
    }
    if (scoring.scheduleShape.dailyLoadScore.idealActiveDays == null) {
        scoring.scheduleShape.dailyLoadScore.idealActiveDays = 4;
    }

    if (!scoring.preferenceShape) {
        scoring.preferenceShape = {
            startTimePreference: {},
            endTimePreference: {},
        };
    }
    if (!scoring.preferenceShape.startTimePreference) scoring.preferenceShape.startTimePreference = {};
    if (!scoring.preferenceShape.endTimePreference) scoring.preferenceShape.endTimePreference = {};

    scoring.preferenceShape.startTimePreference.points = toScore(scoring.preferenceShape.startTimePreference.points);
    scoring.preferenceShape.endTimePreference.points = toScore(scoring.preferenceShape.endTimePreference.points);

    if (!scoring.preferenceShape.startTimePreference.preferredStartTime) {
        scoring.preferenceShape.startTimePreference.preferredStartTime = '09:00:00';
    }
    if (!scoring.preferenceShape.endTimePreference.preferredEndTime) {
        scoring.preferenceShape.endTimePreference.preferredEndTime = '18:00:00';
    }

    if (!scoring.gapCompactnessShape) {
        scoring.gapCompactnessShape = { shortGap: {} };
    }
    if (!scoring.gapCompactnessShape.shortGap) {
        scoring.gapCompactnessShape.shortGap = {};
    }

    scoring.gapCompactnessShape.shortGap.points = toScore(scoring.gapCompactnessShape.shortGap.points);

    if (scoring.gapCompactnessShape.shortGap.maxGapMinutes == null) {
        scoring.gapCompactnessShape.shortGap.maxGapMinutes = 90;
    }
    if (!scoring.gapCompactnessShape.shortGap.ignoreGapStartTime) {
        scoring.gapCompactnessShape.shortGap.ignoreGapStartTime = '12:00:00';
    }
    if (!scoring.gapCompactnessShape.shortGap.ignoreGapEndTime) {
        scoring.gapCompactnessShape.shortGap.ignoreGapEndTime = '14:00:00';
    }

    if (!scoring.assessmentShape) {
        scoring.assessmentShape = { assessmentCategoryScores: [] };
    }
    if (!scoring.assessmentShape.assessmentCategoryScores) {
        scoring.assessmentShape.assessmentCategoryScores = [];
    }

    const existingByCategory = new Map<AssessmentCategory, number>();
    for (const item of scoring.assessmentShape.assessmentCategoryScores) {
        const category = normalizeAssessmentCategory(item.category);
        existingByCategory.set(category, toScore(item.points));
    }

    scoring.assessmentShape.assessmentCategoryScores = assessmentTypeOptions.map((option) => ({
        category: option.value,
        points: existingByCategory.get(option.value) ?? 0,
    }));
};

const loadBlockTimesFromRequest = () => {
    const mapped: BlockTimes = {};
    for (const blockTime of generationRequest.value.filter?.noClassTime ?? []) {
        const day = Number(blockTime.day);
        const startTime = getHour(blockTime.start);
        const endTime = getHour(blockTime.end);

        if (!Number.isFinite(day) || startTime === undefined || endTime === undefined) continue;
        if (!mapped[day]) mapped[day] = [];

        mapped[day].push({
            id: crypto.randomUUID(),
            startTime,
            endTime,
        });
    }

    blockTimes.value = mapped;
};

const handleAddBlockTime = (day: number, items: BlockTimeItem[]) => {
    blockTimes.value[day] = items;
};

const handleDeleteBlockTime = (day: number, itemId: string) => {
    if (!blockTimes.value[day]) return;
    blockTimes.value[day] = blockTimes.value[day].filter((item) => item.id !== itemId);
    if (blockTimes.value[day].length === 0) {
        const dayKey = String(day);
        const { [dayKey]: _removed, ...rest } = blockTimes.value as Record<string, BlockTimeItem[]>;
        blockTimes.value = rest as BlockTimes;
    }
};

const isWeightsValid = computed(() => {
    const scheduleWeight = toNumber(generationRequest.value.scoring.groupWeights.schedule);
    const timePreferenceWeight = toNumber(generationRequest.value.scoring.groupWeights.timePreference);
    const gapWeight = toNumber(generationRequest.value.scoring.groupWeights.gap);
    const assessmentWeight = toNumber(generationRequest.value.scoring.groupWeights.assessments);

    if (scheduleWeight < 0 || timePreferenceWeight < 0 || gapWeight < 0 || assessmentWeight < 0) {
        return false;
    }

    const totalWeight = scheduleWeight + timePreferenceWeight + gapWeight + assessmentWeight;

    return Math.abs(totalWeight - 1) < 0.000001;
});

const assessmentScores = computed(() => generationRequest.value.scoring.assessmentShape.assessmentCategoryScores ?? []);

ensureRequestShape();
loadBlockTimesFromRequest();

watch(blockTimes, (value) => {
    if (!generationRequest.value.filter) {
        generationRequest.value.filter = {};
    }
    generationRequest.value.filter.noClassTime = toNoClassTimes(value);
}, { deep: true, immediate: true });
</script>

<template>
    <div class="space-y-4">
        <Card>
            <CardHeader class="space-y-3">
                <div class="flex items-center gap-2">
                    <Filter class="h-4 w-4 text-primary" />
                    <CardTitle>Filtering</CardTitle>
                </div>
                <CardDescription>
                    Set time window and blocked times to remove unwanted suggestions.
                </CardDescription>
            </CardHeader>

            <CardContent class="space-y-6">
                <div class="space-y-3">
                    <div class="flex items-center justify-between gap-3">
                        <div>
                            <p class="text-sm font-medium">Blocked Times</p>
                            <p class="text-xs text-muted-foreground">
                                Exclude specific day/time slots from generated suggestions.
                            </p>
                        </div>
                        <Button variant="outline" @click="blockTimeDialogOpen = true">
                            Add Block Time
                        </Button>
                    </div>

                    <div v-if="Object.values(blockTimes).flat().length === 0"
                        class="rounded-md border border-dashed p-3 text-sm text-muted-foreground">
                        No blocked time added yet.
                    </div>

                    <div v-else class="mt-4 space-y-3">
                        <div v-for="(items, day) in blockTimes" :key="day">
                            <h3 class="mb-2 text-sm font-medium">{{ days[Number(day)] }}</h3>
                            <div class="space-y-2">
                                <div v-for="item in items" :key="item.id"
                                    class="flex items-center justify-between rounded bg-muted p-2">
                                    <span class="text-sm">{{ item.startTime }}:30 - {{ item.endTime }}:30</span>
                                    <Button variant="ghost" size="sm"
                                        @click="handleDeleteBlockTime(Number(day), item.id)">Delete</Button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>

        <Alert v-if="!isWeightsValid" variant="destructive">
            <AlertDescription>
                Set all weights to 0 or higher, and make sure they add up to 1.0.
            </AlertDescription>
        </Alert>

        <Card>
            <CardHeader>
                <CardTitle>Schedule Preference</CardTitle>
                <CardDescription>
                    Choose the kind of weekly layout you want. These 4 settings are averaged into one schedule preference score.
                </CardDescription>
            </CardHeader>

            <CardContent class="space-y-4">
                <div class="space-y-2 sm:max-w-xs">
                    <Label for="scheduleWeight">Weight</Label>
                    <Input id="scheduleWeight" v-model.number="generationRequest.scoring.groupWeights.schedule"
                        type="number" min="0" step="0.1" />
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Free Day Score</p>
                    <p class="text-xs text-muted-foreground">Move toward +3 if you want more days off. Move toward -3 if you prefer a fuller week.
                    </p>
                    <RadioGroup v-model="generationRequest.scoring.scheduleShape.freeDayScore.points" class="grid grid-cols-7 gap-2">
                        <div v-for="option in scoreOptions" :key="`free-day-${option.value}`"
                            class="flex items-center space-x-2 rounded border px-2 py-1.5">
                            <RadioGroupItem :id="`free-day-${option.value}`" :value="option.value" />
                            <Label :for="`free-day-${option.value}`">{{ option.label }}</Label>
                        </div>
                    </RadioGroup>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Single Class Day Score</p>
                    <p class="text-xs text-muted-foreground">Move toward +3 if you like days with only one class. Move toward -3 if you want to avoid one-class days.</p>
                    <RadioGroup v-model="generationRequest.scoring.scheduleShape.singleClassDayScore.points" class="grid grid-cols-7 gap-2">
                        <div v-for="option in scoreOptions" :key="`single-day-${option.value}`"
                            class="flex items-center space-x-2 rounded border px-2 py-1.5">
                            <RadioGroupItem :id="`single-day-${option.value}`" :value="option.value" />
                            <Label :for="`single-day-${option.value}`">{{ option.label }}</Label>
                        </div>
                    </RadioGroup>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Long Day Score</p>
                    <p class="text-xs text-muted-foreground">Set your longest acceptable day length. +3 favors days within that limit, while -3 favors longer days.</p>
                    <div class="space-y-1.5">
                        <Label for="maxMinutesPerDay">Max Minutes Per Day</Label>
                        <NumberField id="maxMinutesPerDay"
                            v-model="generationRequest.scoring.scheduleShape.longDayScore.maxMinutesPerDay" :min="0"
                            :step="30">
                            <NumberFieldContent>
                                <NumberFieldDecrement />
                                <NumberFieldInput />
                                <NumberFieldIncrement />
                            </NumberFieldContent>
                        </NumberField>
                    </div>
                    <RadioGroup v-model="generationRequest.scoring.scheduleShape.longDayScore.points" class="grid grid-cols-7 gap-2">
                        <div v-for="option in scoreOptions" :key="`long-day-${option.value}`"
                            class="flex items-center space-x-2 rounded border px-2 py-1.5">
                            <RadioGroupItem :id="`long-day-${option.value}`" :value="option.value" />
                            <Label :for="`long-day-${option.value}`">{{ option.label }}</Label>
                        </div>
                    </RadioGroup>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Daily Load Score</p>
                    <p class="text-xs text-muted-foreground">Choose how many active days feels ideal. +3 prefers staying at or below that number; -3 prefers going above it.</p>
                    <div class="space-y-1.5">
                        <Label for="idealActiveDays">Ideal Active Days</Label>
                        <NumberField id="idealActiveDays"
                            v-model="generationRequest.scoring.scheduleShape.dailyLoadScore.idealActiveDays" :min="0"
                            :max="7" :step="1">
                            <NumberFieldContent>
                                <NumberFieldDecrement />
                                <NumberFieldInput />
                                <NumberFieldIncrement />
                            </NumberFieldContent>
                        </NumberField>
                    </div>
                    <RadioGroup v-model="generationRequest.scoring.scheduleShape.dailyLoadScore.points" class="grid grid-cols-7 gap-2">
                        <div v-for="option in scoreOptions" :key="`daily-load-${option.value}`"
                            class="flex items-center space-x-2 rounded border px-2 py-1.5">
                            <RadioGroupItem :id="`daily-load-${option.value}`" :value="option.value" />
                            <Label :for="`daily-load-${option.value}`">{{ option.label }}</Label>
                        </div>
                    </RadioGroup>
                </div>
            </CardContent>
        </Card>

        <Card>
            <CardHeader>
                <CardTitle>Time Preference</CardTitle>
                <CardDescription>
                    Control what time your day starts and ends. This section checks how well each schedule matches your preferred start and finish times.
                </CardDescription>
            </CardHeader>

            <CardContent class="space-y-4">
                <div class="space-y-2 sm:max-w-xs">
                    <Label for="timePreferenceWeight">Weight</Label>
                    <NumberField id="timePreferenceWeight"
                        v-model="generationRequest.scoring.groupWeights.timePreference" :min="0" :step="0.01">
                        <NumberFieldContent>
                            <NumberFieldDecrement />
                            <NumberFieldInput />
                            <NumberFieldIncrement />
                        </NumberFieldContent>
                    </NumberField>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Start Time Preference</p>
                    <p class="text-xs text-muted-foreground">Move toward +3 to avoid early classes. Move toward -3 if you prefer earlier starts.</p>
                    <div class="space-y-1.5">
                        <Label>Preferred Start Time</Label>
                        <Select
                            v-model="generationRequest.scoring.preferenceShape.startTimePreference.preferredStartTime">
                            <SelectTrigger class="w-full">
                                <SelectValue placeholder="Select Start Time" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem v-for="option in startTimeOptions" :key="option.value"
                                    :value="option.value">
                                    {{ option.label }}
                                </SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                    <RadioGroup v-model="generationRequest.scoring.preferenceShape.startTimePreference.points" class="grid grid-cols-7 gap-2">
                        <div v-for="option in scoreOptions" :key="`start-pref-${option.value}`"
                            class="flex items-center space-x-2 rounded border px-2 py-1.5">
                            <RadioGroupItem :id="`start-pref-${option.value}`" :value="option.value" />
                            <Label :for="`start-pref-${option.value}`">{{ option.label }}</Label>
                        </div>
                    </RadioGroup>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">End Time Preference</p>
                    <p class="text-xs text-muted-foreground">Move toward +3 to finish earlier. Move toward -3 if later finishes are okay.</p>
                    <div class="space-y-1.5">
                        <Label>Preferred End Time</Label>
                        <Select v-model="generationRequest.scoring.preferenceShape.endTimePreference.preferredEndTime">
                            <SelectTrigger class="w-full">
                                <SelectValue placeholder="Select End Time" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem v-for="option in endTimeOptions" :key="option.value" :value="option.value">
                                    {{ option.label }}
                                </SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                    <RadioGroup v-model="generationRequest.scoring.preferenceShape.endTimePreference.points" class="grid grid-cols-7 gap-2">
                        <div v-for="option in scoreOptions" :key="`end-pref-${option.value}`"
                            class="flex items-center space-x-2 rounded border px-2 py-1.5">
                            <RadioGroupItem :id="`end-pref-${option.value}`" :value="option.value" />
                            <Label :for="`end-pref-${option.value}`">{{ option.label }}</Label>
                        </div>
                    </RadioGroup>
                </div>
            </CardContent>
        </Card>

        <Card>
            <CardHeader>
                <CardTitle>Gap Compactness</CardTitle>
                <CardDescription>
                    Decide whether you want classes packed closely together or spread out with longer breaks in between.
                </CardDescription>
            </CardHeader>

            <CardContent class="space-y-4">
                <div class="space-y-2 sm:max-w-xs">
                    <Label for="gapWeight">Weight</Label>
                    <Input id="gapWeight" v-model.number="generationRequest.scoring.groupWeights.gap" type="number"
                        min="0" step="0.1" />
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Short Gap Rule</p>
                    <p class="text-xs text-muted-foreground">Set the biggest break you consider "short", and an optional lunch window to ignore. +3 favors short breaks, -3 favors longer breaks, and 0 ignores this rule.</p>
                    <div class="grid gap-3 sm:grid-cols-3">
                        <div class="space-y-1.5">
                            <Label for="maxGapMinutes">Max Gap Minutes</Label>
                            <NumberField id="maxGapMinutes"
                                v-model="generationRequest.scoring.gapCompactnessShape.shortGap.maxGapMinutes" :min="0"
                                :step="10">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <Label>Ignore Gap Start Time</Label>
                            <Select v-model="generationRequest.scoring.gapCompactnessShape.shortGap.ignoreGapStartTime">
                                <SelectTrigger class="w-full">
                                    <SelectValue placeholder="Select Start Time" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem v-for="option in startTimeOptions" :key="`gap-start-${option.value}`"
                                        :value="option.value">
                                        {{ option.label }}
                                    </SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                        <div class="space-y-1.5">
                            <Label>Ignore Gap End Time</Label>
                            <Select v-model="generationRequest.scoring.gapCompactnessShape.shortGap.ignoreGapEndTime">
                                <SelectTrigger class="w-full">
                                    <SelectValue placeholder="Select End Time" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem v-for="option in endTimeOptions" :key="`gap-end-${option.value}`"
                                        :value="option.value">
                                        {{ option.label }}
                                    </SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                    </div>
                    <RadioGroup v-model="generationRequest.scoring.gapCompactnessShape.shortGap.points" class="grid grid-cols-7 gap-2">
                        <div v-for="option in scoreOptions" :key="`short-gap-${option.value}`"
                            class="flex items-center space-x-2 rounded border px-2 py-1.5">
                            <RadioGroupItem :id="`short-gap-${option.value}`" :value="option.value" />
                            <Label :for="`short-gap-${option.value}`">{{ option.label }}</Label>
                        </div>
                    </RadioGroup>
                </div>
            </CardContent>
        </Card>

        <Card>
            <CardHeader>
                <CardTitle>Assessments</CardTitle>
                <CardDescription>
                    Tell the generator which assessment types you want to see more or less often. Each type is scored by whether it appears in a timetable.
                </CardDescription>
            </CardHeader>

            <CardContent class="space-y-4">
                <div class="space-y-2 sm:max-w-xs">
                    <Label for="assessmentWeight">Weight</Label>
                    <Input id="assessmentWeight" v-model.number="generationRequest.scoring.groupWeights.assessments"
                        type="number" min="0" step="0.1" />
                </div>

                <div class="space-y-3">
                    <div v-for="item in assessmentScores" :key="item.category" class="rounded-md border p-4 space-y-3">
                        <p class="text-sm font-medium">{{ formatAssessmentCategory(item.category) }}</p>
                        <RadioGroup v-model="item.points" class="grid grid-cols-7 gap-2">
                            <div v-for="score in scoreOptions" :key="`assessment-${item.category}-${score.value}`"
                                class="flex items-center space-x-2 rounded border px-2 py-1.5">
                                <RadioGroupItem :id="`assessment-${item.category}-${score.value}`" :value="score.value" />
                                <Label :for="`assessment-${item.category}-${score.value}`">{{ score.label }}</Label>
                            </div>
                        </RadioGroup>
                    </div>
                </div>
            </CardContent>
        </Card>

        <div class="flex w-full justify-end">
            <p class="mr-auto text-xs text-muted-foreground">
                Final score combines these 4 groups based on your weights.
            </p>
            <Button :disabled="!isWeightsValid || props.submitting" @click="emit('submit')">
                Next
                <ChevronRight class="ml-2 h-4 w-4" />
            </Button>
        </div>
    </div>

    <UiTimetableBlocktime v-model:open="blockTimeDialogOpen" :block-times="blockTimes"
        @add-block-time="handleAddBlockTime" @delete-block-time="handleDeleteBlockTime" />
</template>
