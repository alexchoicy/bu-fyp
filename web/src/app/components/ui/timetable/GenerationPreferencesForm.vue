<script setup lang="ts">
import { ChevronRight, Filter } from 'lucide-vue-next';
import type { components } from '~/API/schema';

type TimetableGenerationRequest = components["schemas"]["TimetableGenerationRequestDto"];
type TimetableNoClassTime = components["schemas"]["TimetableNoClassTimeDto"];
type AssessmentCategory = components["schemas"]["AssessmentCategory"];

const assessmentTypeOptions: { label: string; value: AssessmentCategory }[] = [
    { label: "Examination", value: "Examination" },
    { label: "Assignment", value: "Assignment" },
    { label: "Project / Group Project", value: "Project" },
    { label: "Solo Project", value: "SoloProject" },
    { label: "Participation", value: "Participation" },
    { label: "Presentation", value: "Presentation" },
    { label: "Other", value: "Other" },
];


interface BlockTimeItem {
    id: string;
    startTime: number;
    endTime: number;
}

type BlockTimes = Record<number, BlockTimeItem[]>;


const blockTimeDialogOpen = ref(false);
const blockTimes = ref<BlockTimes>({});


const generationRequest = defineModel<TimetableGenerationRequest>("generationRequest", {
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

const days = ["Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"];

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

const scheduleEnabled = ref(true);
const timePreferenceEnabled = ref(true);
const gapEnabled = ref(true);
const assessmentEnabled = ref(true);

const toNumber = (value: string | number | undefined) => Number(value ?? 0);

const isWeightsValid = computed(() => {
    const totalWeight =
        (scheduleEnabled.value ? toNumber(generationRequest.value.scoring.groupWeights.schedule) : 0) +
        (timePreferenceEnabled.value ? toNumber(generationRequest.value.scoring.groupWeights.timePreference) : 0) +
        (gapEnabled.value ? toNumber(generationRequest.value.scoring.groupWeights.gap) : 0) +
        (assessmentEnabled.value ? toNumber(generationRequest.value.scoring.groupWeights.assessments) : 0);

    return Math.abs(totalWeight - 1) < 0.000001;
});

const padHour = (hour: number) => hour.toString().padStart(2, "0");

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

const selectedAssessmentType = ref<AssessmentCategory>("Examination");
const assessmentRewardPoints = ref<number>(60);
const assessmentPenaltyPoints = ref<number>(25);

const getHour = (time?: string | null) => {
    if (!time) return undefined;
    const hour = Number.parseInt(time.split(":")[0] ?? "", 10);
    return Number.isFinite(hour) ? hour : undefined;
};

const toNoClassTimes = (source: BlockTimes): TimetableNoClassTime[] => Object.entries(source)
    .flatMap(([day, items]) => items.map((item) => ({
        day: Number(day),
        start: `${item.startTime.toString().padStart(2, "0")}:30:00`,
        end: `${item.endTime.toString().padStart(2, "0")}:30:00`,
    })));

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

const normalizeAssessmentCategory = (category?: AssessmentCategory | number | string) => {
    if (!category) return "Other" as AssessmentCategory;
    if (category === "GroupProject") return "Project" as AssessmentCategory;

    const numeric = Number(category);
    if (Number.isFinite(numeric)) {
        if (numeric === 1) return "Examination";
        if (numeric === 2) return "Assignment";
        if (numeric === 3) return "Project";
        if (numeric === 5) return "SoloProject";
        if (numeric === 6) return "Participation";
        if (numeric === 7) return "Presentation";
        if (numeric === 99) return "Other";
    }

    return category as AssessmentCategory;
};

const formatAssessmentCategory = (category?: AssessmentCategory) => {
    if (category === "Project" || category === "GroupProject") return "Project / Group Project";
    if (!category) return "Other";
    return category;
};


const addOrUpdateAssessmentScore = () => {
    if (!generationRequest.value.scoring.assessmentShape.assessmentCategoryScores) {
        generationRequest.value.scoring.assessmentShape.assessmentCategoryScores = [];
    }

    const category = normalizeAssessmentCategory(selectedAssessmentType.value);
    const existing = generationRequest.value.scoring.assessmentShape.assessmentCategoryScores
        .find((item) => normalizeAssessmentCategory(item.category) === category);

    if (existing) {
        existing.rewardPoints = assessmentRewardPoints.value;
        existing.penaltyPoints = assessmentPenaltyPoints.value;
        existing.category = category;
        return;
    }

    generationRequest.value.scoring.assessmentShape.assessmentCategoryScores.push({
        category,
        rewardPoints: assessmentRewardPoints.value,
        penaltyPoints: assessmentPenaltyPoints.value,
    });
};

const removeAssessmentScore = (category?: AssessmentCategory) => {
    if (!generationRequest.value.scoring.assessmentShape.assessmentCategoryScores) return;
    const target = normalizeAssessmentCategory(category);
    generationRequest.value.scoring.assessmentShape.assessmentCategoryScores =
        generationRequest.value.scoring.assessmentShape.assessmentCategoryScores
            .filter((item) => normalizeAssessmentCategory(item.category) !== target);
};

const assessmentScores = computed(() => generationRequest.value.scoring.assessmentShape.assessmentCategoryScores ?? []);

if (!generationRequest.value.filter) {
    generationRequest.value.filter = {};
}

if (!generationRequest.value.scoring.assessmentShape.assessmentCategoryScores) {
    generationRequest.value.scoring.assessmentShape.assessmentCategoryScores = [];
}

generationRequest.value.scoring.assessmentShape.assessmentCategoryScores =
    generationRequest.value.scoring.assessmentShape.assessmentCategoryScores.map((item) => ({
        ...item,
        category: normalizeAssessmentCategory(item.category),
    }));

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
                            <h3 class="text-sm font-medium mb-2">{{ days[Number(day)] }}</h3>
                            <div class="space-y-2">
                                <div v-for="item in items" :key="item.id"
                                    class="flex items-center justify-between p-2 bg-muted rounded">
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

        <Alert variant="destructive" v-if="!isWeightsValid">
            <AlertDescription>
                Please set at Total Sum of Weights need to be = 1
            </AlertDescription>
        </Alert>

        <Card>
            <CardHeader>
                <div class="flex items-start justify-between gap-3">
                    <div>
                        <CardTitle>Schedule Shape</CardTitle>
                        <CardDescription>
                            Scores overall schedule distribution across days.
                        </CardDescription>
                    </div>
                    <!-- <div class="flex items-center gap-2 pt-0.5">
                        <Label for="scheduleSwitch" class="cursor-pointer text-sm">Enable</Label>
                        <Switch id="scheduleSwitch" v-model="scheduleEnabled" />
                    </div> -->
                </div>
            </CardHeader>

            <CardContent :class="{ 'pointer-events-none opacity-50': !scheduleEnabled }" class="space-y-4">
                <div class="space-y-2 sm:max-w-xs">
                    <Label for="scheduleWeight">Weight</Label>
                    <Input id="scheduleWeight" v-model.number="generationRequest.scoring.groupWeights.schedule"
                        type="number" min="0" step="0.1" />
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Free Day Score</p>
                    <p class="text-xs text-muted-foreground">Rewards timetables with fully free days.</p>
                    <div class="grid gap-3 sm:grid-cols-2">
                        <div class="space-y-1.5">
                            <NumberField id="freeDayReward"
                                v-model="generationRequest.scoring.scheduleShape.freeDayScore.rewardPoints" :min="0"
                                :max="100" :step="1">
                                <Label for="freeDayReward">Reward Point</Label>
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <NumberField id="freeDayPenalty"
                                v-model="generationRequest.scoring.scheduleShape.freeDayScore.penaltyPoints" :min="0"
                                :max="90" :step="1">
                                <Label for="freeDayPenalty">Penalty Point</Label>
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                    </div>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Single Class Day Score</p>
                    <p class="text-xs text-muted-foreground">Rewards days that only contain one class block.</p>
                    <div class="grid gap-3 sm:grid-cols-2">
                        <div class="space-y-1.5">
                            <NumberField id="singleDayReward"
                                v-model="generationRequest.scoring.scheduleShape.singleClassDayScore.rewardPoints"
                                :min="0" :max="100" :step="1">
                                <Label for="singleDayReward">Reward Point</Label>
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <NumberField id="singleDayPenalty"
                                v-model="generationRequest.scoring.scheduleShape.singleClassDayScore.penaltyPoints"
                                :min="0" :max="90" :step="1">
                                <Label for="singleDayPenalty">Penalty Point</Label>
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                    </div>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Long Day Score</p>
                    <p class="text-xs text-muted-foreground">Max minutes per day is checked before long-day penalties
                        apply.</p>
                    <div class="grid gap-3 sm:grid-cols-3">
                        <div class="space-y-1.5">
                            <NumberField id="longDayReward"
                                v-model="generationRequest.scoring.scheduleShape.longDayScore.rewardPoints" :min="0"
                                :max="100" :step="1">
                                <Label for="longDayReward">Reward Point</Label>
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
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
                        <div class="space-y-1.5">
                            <Label for="longDayPenalty">Penalty Point</Label>
                            <NumberField id="longDayPenalty"
                                v-model="generationRequest.scoring.scheduleShape.longDayScore.penaltyPoints" :min="0"
                                :max="90" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                    </div>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Daily Load Score</p>
                    <p class="text-xs text-muted-foreground">If ideal active days is 0, only 0 active days get full
                        ratio. Above ideal active days receive penalty ratio.</p>
                    <div class="grid gap-3 sm:grid-cols-3">
                        <div class="space-y-1.5">
                            <Label for="dailyLoadReward">Reward Point</Label>
                            <NumberField id="dailyLoadReward"
                                v-model="generationRequest.scoring.scheduleShape.dailyLoadScore.rewardPoints" :min="0"
                                :max="100" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <Label for="idealActiveDays">Ideal Active Days</Label>
                            <NumberField id="idealActiveDays"
                                v-model="generationRequest.scoring.scheduleShape.dailyLoadScore.idealActiveDays"
                                :min="0" :max="7" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <Label for="dailyLoadPenalty">Penalty Point</Label>
                            <NumberField id="dailyLoadPenalty"
                                v-model="generationRequest.scoring.scheduleShape.dailyLoadScore.penaltyPoints" :min="0"
                                :max="90" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>

        <Card>
            <CardHeader>
                <div class="flex items-start justify-between gap-3">
                    <div>
                        <CardTitle>Time Preference Shape</CardTitle>
                        <CardDescription>
                            Scores layouts based on preferred start and end times.
                        </CardDescription>
                    </div>
                    <!-- <div class="flex items-center gap-2 pt-0.5">
                        <Label for="timePreferenceSwitch" class="cursor-pointer text-sm">Enable</Label>
                        <Switch id="timePreferenceSwitch" v-model="timePreferenceEnabled" />
                    </div> -->
                </div>
            </CardHeader>

            <CardContent :class="{ 'pointer-events-none opacity-50': !timePreferenceEnabled }" class="space-y-4">
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
                    <div class="grid gap-3 sm:grid-cols-3">
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
                        <div class="space-y-1.5">
                            <Label for="startTimeReward">Reward Point</Label>
                            <NumberField id="startTimeReward"
                                v-model="generationRequest.scoring.preferenceShape.startTimePreference.rewardPoints"
                                :min="0" :max="100" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <Label for="startTimePenalty">Penalty Point</Label>
                            <NumberField id="startTimePenalty"
                                v-model="generationRequest.scoring.preferenceShape.startTimePreference.penaltyPoints"
                                :min="0" :max="90" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                    </div>
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">End Time Preference</p>
                    <div class="grid gap-3 sm:grid-cols-3">
                        <div class="space-y-1.5">
                            <Label>Preferred End Time</Label>
                            <Select
                                v-model="generationRequest.scoring.preferenceShape.endTimePreference.preferredEndTime">
                                <SelectTrigger class="w-full">
                                    <SelectValue placeholder="Select End Time" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem v-for="option in endTimeOptions" :key="option.value"
                                        :value="option.value">
                                        {{ option.label }}
                                    </SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                        <div class="space-y-1.5">
                            <Label for="endTimeReward">Reward Point</Label>
                            <NumberField id="endTimeReward"
                                v-model="generationRequest.scoring.preferenceShape.endTimePreference.rewardPoints"
                                :min="0" :max="100" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <Label for="endTimePenalty">Penalty Point</Label>
                            <NumberField id="endTimePenalty"
                                v-model="generationRequest.scoring.preferenceShape.endTimePreference.penaltyPoints"
                                :min="0" :max="90" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>

        <Card>
            <CardHeader>
                <div class="flex items-start justify-between gap-3">
                    <div>
                        <CardTitle>Gap Compactness Shape</CardTitle>
                        <CardDescription>
                            Scores schedules by penalizing long gaps between classes.
                        </CardDescription>
                    </div>
                    <!-- <div class="flex items-center gap-2 pt-0.5">
                        <Label for="gapSwitch" class="cursor-pointer text-sm">Enable</Label>
                        <Switch id="gapSwitch" v-model="gapEnabled" />
                    </div> -->
                </div>
            </CardHeader>

            <CardContent :class="{ 'pointer-events-none opacity-50': !gapEnabled }" class="space-y-4">
                <div class="space-y-2 sm:max-w-xs">
                    <Label for="gapWeight">Weight</Label>
                    <Input id="gapWeight" v-model.number="generationRequest.scoring.groupWeights.gap" type="number"
                        min="0" step="0.1" />
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Short Gap Rule</p>
                    <p class="text-xs text-muted-foreground">Use a target gap duration to decide when a gap becomes too
                        long.</p>
                    <div class="grid gap-3 sm:grid-cols-2">
                        <div class="space-y-1.5">
                            <Label for="shortGapReward">Reward Point</Label>
                            <NumberField id="shortGapReward"
                                v-model="generationRequest.scoring.gapCompactnessShape.shortGap.rewardPoints" :min="0"
                                :max="100" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <Label for="shortGapPenalty">Penalty Point</Label>
                            <NumberField id="shortGapPenalty"
                                v-model="generationRequest.scoring.gapCompactnessShape.shortGap.penaltyPoints" :min="0"
                                :max="90" :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                    </div>

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
                </div>
            </CardContent>
        </Card>

        <Card>
            <CardHeader>
                <div class="flex items-start justify-between gap-3">
                    <div>
                        <CardTitle>Assessment Shape</CardTitle>
                        <CardDescription>
                            Configure per-assessment-type reward and penalty points.
                        </CardDescription>
                    </div>
                    <div class="flex items-center gap-2 pt-0.5">
                        <Label for="assessmentSwitch" class="cursor-pointer text-sm">Enable</Label>
                        <Switch id="assessmentSwitch" v-model="assessmentEnabled" />
                    </div>
                </div>
            </CardHeader>

            <CardContent :class="{ 'pointer-events-none opacity-50': !assessmentEnabled }" class="space-y-4">
                <div class="space-y-2 sm:max-w-xs">
                    <Label for="assessmentWeight">Weight</Label>
                    <Input id="assessmentWeight" v-model.number="generationRequest.scoring.groupWeights.assessments"
                        type="number" min="0" step="0.1" />
                </div>

                <div class="rounded-md border p-4 space-y-3">
                    <p class="text-sm font-medium">Add Assessment Type Score</p>
                    <div class="grid gap-3 sm:grid-cols-4">
                        <div class="space-y-1.5 sm:col-span-2">
                            <Label>Assessment Type</Label>
                            <Select v-model="selectedAssessmentType">
                                <SelectTrigger class="w-full">
                                    <SelectValue placeholder="Select Type" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem v-for="option in assessmentTypeOptions" :key="option.value"
                                        :value="option.value">
                                        {{ option.label }}
                                    </SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                        <div class="space-y-1.5">
                            <Label for="assessmentReward">Reward Point</Label>
                            <NumberField id="assessmentReward" v-model="assessmentRewardPoints" :min="0" :max="100"
                                :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                        <div class="space-y-1.5">
                            <Label for="assessmentPenalty">Penalty Point</Label>
                            <NumberField id="assessmentPenalty" v-model="assessmentPenaltyPoints" :min="0" :max="90"
                                :step="1">
                                <NumberFieldContent>
                                    <NumberFieldDecrement />
                                    <NumberFieldInput />
                                    <NumberFieldIncrement />
                                </NumberFieldContent>
                            </NumberField>
                        </div>
                    </div>
                    <Button variant="outline" @click="addOrUpdateAssessmentScore">Add / Update</Button>
                </div>

                <div v-if="assessmentScores.length === 0"
                    class="rounded-md border border-dashed p-3 text-sm text-muted-foreground">
                    No assessment type score added yet.
                </div>

                <div v-else class="space-y-2">
                    <div v-for="item in assessmentScores" :key="item.category"
                        class="flex flex-wrap items-center justify-between gap-2 rounded-md border bg-muted/30 px-3 py-2">
                        <span class="text-sm">
                            {{ formatAssessmentCategory(item.category) }} - Reward {{ Number(item.rewardPoints ?? 0) }},
                            Penalty {{ Number(item.penaltyPoints ?? 0) }}
                        </span>
                        <Button variant="ghost" size="sm" @click="removeAssessmentScore(item.category)">Delete</Button>
                    </div>
                </div>
            </CardContent>
        </Card>


        <div class="flex justify-end w-full">
            <Button :disabled="!isWeightsValid || props.submitting" @click="emit('submit')">
                Next
                <ChevronRight class="ml-2 h-4 w-4" />
            </Button>
        </div>
    </div>

    <UiTimetableBlocktime v-model:open="blockTimeDialogOpen" :block-times="blockTimes"
        @add-block-time="handleAddBlockTime" @delete-block-time="handleDeleteBlockTime" />
</template>
