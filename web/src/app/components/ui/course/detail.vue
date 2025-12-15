<script setup lang="ts">
import {
    ArrowLeft,
    Download,
    FileText,
    Building2,
    BookOpen,
    Languages,
    Target,
    ListOrdered,
    Lightbulb,
    Users,
    ClipboardCheck,
    AlertTriangle,
    CheckCircle2,
    Clock,
    Calendar,
    Link2,
    ArrowUpRight,
    Ban,
    AlertCircleIcon,
} from "lucide-vue-next";
import type { components } from "~/API/schema";

type CourseResponseDto = components["schemas"]["CourseResponseDto"];
type CourseVersionResponseDto =
    components["schemas"]["CourseVersionResponseDto"];

const route = useRoute();
const courseId = computed(() => route.params.id as string);

const { data: studied } = useAPI<components["schemas"]["UserCourseDto"][]>('me/courses')


// Fetch course data
const { data: course, status } = useAPI<CourseResponseDto>(
    () => `courses/${courseId.value}`
);

// Selected version
const selectedVersionId = ref<number | null>(null);

// Set default selected version when data loads
watch(
    course,
    (newCourse) => {
        if (newCourse?.versions && newCourse.versions.length > 0) {
            // Select the latest version by default
            const latestVersion = newCourse.versions.reduce((latest, current) =>
                (current.versionNumber ?? 0) > (latest.versionNumber ?? 0)
                    ? current
                    : latest
            );
            selectedVersionId.value = latestVersion.id as number;
        }
    },
    { immediate: true }
);

const selectedVersion = computed(() => {
    if (!course.value?.versions || !selectedVersionId.value) return null;
    return course.value.versions.find((v) => v.id === selectedVersionId.value) as
        | CourseVersionResponseDto
        | undefined;
});

const preRequisites = computed(
    () => selectedVersion.value?.preRequisites ?? []
);
const antiRequisites = computed(
    () => selectedVersion.value?.antiRequisites ?? []
);
const totalLinkedCourses = computed(
    () => preRequisites.value.length + antiRequisites.value.length
);

const studiedIds = computed(() => new Set((studied.value ?? []).map(sc => String(sc.courseId))));

const studiedPreRequisites = computed(() => {
    return preRequisites.value.filter(pr => !studiedIds.value.has(String(pr.id)));
});

const studiedAntiRequisites = computed(() => {
    return antiRequisites.value.filter(anti => studiedIds.value.has(String(anti.id)));
});

// Assessment category labels
const categoryLabels: Record<string, string> = {
    "1": "Examination",
    "2": "Coursework",
    "3": "Project",
    "4": "Practical",
    "5": "Oral",
    "6": "Participation",
};

// Calculate total weighting
const totalWeighting = computed(() => {
    if (!selectedVersion.value?.assessments) return 0;
    return selectedVersion.value.assessments.reduce(
        (sum, am) => sum + Number(am.weighting ?? 0),
        0
    );
});

const isWeightingValid = computed(() => totalWeighting.value === 100);

// Format date helper
function formatDate(dateStr?: string) {
    if (!dateStr) return "—";
    return new Date(dateStr).toLocaleDateString("en-US", {
        year: "numeric",
        month: "short",
        day: "numeric",
    });
}

const displayCourseContent = computed(() => {
    const raw = selectedVersion.value?.courseContent ?? "";
    return (raw && raw.replace(/\\n/g, "\n")) || "No course content specified.";
});

function formatCourseCode(course?: components["schemas"]["SimpleCourseDto"]) {
    if (!course) return "—";
    const code = `${course.codeTag ?? ""}${course.courseNumber ?? ""}`.trim();
    return code || "—";
}
</script>

<template>
    <div class="min-h-screen bg-background">
        <!-- Header -->
        <header
            class="sticky top-0 z-50 left-1/2 border-b bg-background/95 backdrop-blur supports-backdrop-filter:bg-background/60">
            <div class="container flex h-16 items-center justify-between px-4 md:px-6">
                <div class="flex items-center gap-4">
                    <NuxtLink to="/courses">
                        <Button variant="ghost" size="icon">
                            <ArrowLeft class="h-5 w-5" />
                        </Button>
                    </NuxtLink>
                    <div>
                        <h1 class="text-lg font-semibold">Course Details</h1>
                        <p v-if="course" class="text-xs text-muted-foreground">
                            {{ course.codeTag }}{{ course.courseNumber }} - {{ course.name }}
                        </p>
                    </div>
                </div>
                <!-- <div class="flex items-center gap-2">
                    <Button variant="outline" size="sm">
                        <Download class="h-4 w-4 mr-2" />
                        Export PDF
                    </Button>
                </div> -->
            </div>
        </header>

        <!-- Loading State -->
        <main v-if="status === 'pending'" class="container px-4 py-6 md:px-6 md:py-8 max-w-5xl mx-auto">
            <div class="space-y-6">
                <Skeleton class="h-32 w-full" />
                <Skeleton class="h-16 w-full" />
                <Skeleton class="h-48 w-full" />
                <Skeleton class="h-48 w-full" />
            </div>
        </main>

        <!-- Error State -->
        <main v-else-if="status === 'error' || !course" class="container px-4 py-6 md:px-6 md:py-8 max-w-5xl mx-auto">
            <div class="flex items-center justify-center min-h-[400px]">
                <p class="text-muted-foreground">Course not found</p>
            </div>
        </main>

        <!-- Main Content -->
        <main v-else class="container px-4 py-6 md:px-6 md:py-8 max-w-5xl mx-auto">
            <div class="space-y-6">
                <Alert v-if="studiedAntiRequisites.length > 0" variant="destructive">
                    <AlertCircleIcon />
                    <AlertTitle>
                        You have studied anti-requisite course(s) for this course.
                    </AlertTitle>
                    <AlertDescription>
                        <ul class="mt-2 list-inside list-disc space-y-1">
                            <li v-for="anti in studiedAntiRequisites" :key="anti.id">
                                {{ formatCourseCode(anti) }} - {{ anti.name || "Untitled course" }}
                            </li>
                        </ul>
                    </AlertDescription>
                </Alert>

                <Alert v-if="studiedPreRequisites.length > 0" variant="destructive">
                    <AlertCircleIcon />
                    <AlertTitle>
                        You need to study the following pre-requisite course(s) before taking this course.
                    </AlertTitle>
                    <AlertDescription>
                        <ul class="mt-2 list-inside list-disc space-y-1">
                            <li v-for="pre in studiedPreRequisites" :key="pre.id">
                                {{ formatCourseCode(pre) }} - {{ pre.name || "Untitled course" }}
                            </li>
                        </ul>
                    </AlertDescription>
                </Alert>


                <!-- Course Info Summary -->
                <Card>
                    <CardHeader class="pb-3">
                        <div class="flex items-center justify-between">
                            <CardTitle class="text-base font-semibold flex items-center gap-2">
                                <BookOpen class="h-4 w-4 text-primary" />
                                Course Information
                            </CardTitle>
                            <Badge :variant="course.isActive ? 'default' : 'secondary'">
                                {{ course.isActive ? "Active" : "Inactive" }}
                            </Badge>
                        </div>
                    </CardHeader>
                    <CardContent>
                        <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                            <div class="space-y-1">
                                <p class="text-xs font-medium text-muted-foreground">
                                    Course Code
                                </p>
                                <p class="text-sm font-medium font-mono">
                                    {{ course.codeTag }}{{ course.courseNumber }}
                                </p>
                            </div>
                            <div class="space-y-1 sm:col-span-2">
                                <p class="text-xs font-medium text-muted-foreground">
                                    Course Title
                                </p>
                                <p class="text-sm font-medium">{{ course.name }}</p>
                            </div>
                            <div v-if="course.departments && course.departments.length > 0"
                                class="space-y-1 sm:col-span-3">
                                <p class="text-xs font-medium text-muted-foreground flex items-center gap-1">
                                    <Building2 class="h-3 w-3" />
                                    Departments
                                </p>
                                <div class="flex flex-wrap gap-1">
                                    <Badge v-for="dept in course.departments" :key="dept" variant="outline"
                                        class="text-xs">
                                        {{ dept }}
                                    </Badge>
                                </div>
                            </div>
                        </div>
                    </CardContent>
                </Card>

                <!-- Version Selector -->
                <div v-if="course.versions && course.versions.length > 0"
                    class="flex items-center justify-between p-4 rounded-lg border bg-muted/20">
                    <div class="flex items-center gap-3">
                        <span class="text-sm font-medium">Version:</span>
                        <Select v-model="selectedVersionId">
                            <SelectTrigger class="w-[200px]">
                                <SelectValue placeholder="Select version" />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem v-for="version in course.versions" :key="version.id"
                                    :value="version.id as number">
                                    Version {{ version.versionNumber }}
                                </SelectItem>
                            </SelectContent>
                        </Select>
                    </div>
                    <div v-if="selectedVersion" class="text-xs text-muted-foreground">
                        Created: {{ formatDate(selectedVersion.createdAt) }}
                    </div>
                </div>

                <Separator />

                <!-- Version Details -->
                <template v-if="selectedVersion">
                    <!-- Version Meta -->
                    <Card>
                        <CardHeader class="pb-3">
                            <div class="flex items-center justify-between">
                                <CardTitle class="text-base font-semibold flex items-center gap-2">
                                    <Clock class="h-4 w-4 text-primary" />
                                    Version Information
                                </CardTitle>
                                <Badge variant="outline"
                                    class="bg-emerald-500/10 text-emerald-600 border-emerald-500/20">
                                    Version {{ selectedVersion.versionNumber }}
                                </Badge>
                            </div>
                        </CardHeader>
                        <CardContent>
                            <div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
                                <div class="space-y-1">
                                    <p class="text-xs font-medium text-muted-foreground">
                                        Version Number
                                    </p>
                                    <p class="text-sm font-medium">
                                        {{ selectedVersion.versionNumber ?? "—" }}
                                    </p>
                                </div>
                                <div class="space-y-1">
                                    <p class="text-xs font-medium text-muted-foreground flex items-center gap-1">
                                        <Calendar class="h-3 w-3" />
                                        Created At
                                    </p>
                                    <p class="text-sm font-medium">
                                        {{ formatDate(selectedVersion.createdAt) }}
                                    </p>
                                </div>
                                <div class="space-y-1">
                                    <p class="text-xs font-medium text-muted-foreground flex items-center gap-1">
                                        <Languages class="h-3 w-3" />
                                        Medium of Instruction
                                    </p>
                                    <div v-if="
                                        selectedVersion.mediumOfInstruction &&
                                        selectedVersion.mediumOfInstruction.length > 0
                                    " class="flex flex-wrap gap-1">
                                        <Badge v-for="moi in selectedVersion.mediumOfInstruction" :key="moi"
                                            variant="outline" class="text-[10px]">
                                            {{ moi }}
                                        </Badge>
                                    </div>
                                    <p v-else class="text-sm font-medium">—</p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <!-- Description -->
                    <Card v-if="selectedVersion.description">
                        <CardHeader class="pb-3">
                            <CardTitle class="text-base font-semibold flex items-center gap-2">
                                <FileText class="h-4 w-4 text-primary" />
                                Description
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p class="text-sm leading-relaxed text-foreground/90 whitespace-pre-wrap">
                                {{ selectedVersion.description || "No description specified." }}
                            </p>
                        </CardContent>
                    </Card>

                    <!-- Aims & Objectives -->
                    <Card>
                        <CardHeader class="pb-3">
                            <CardTitle class="text-base font-semibold flex items-center gap-2">
                                <Target class="h-4 w-4 text-primary" />
                                Aims & Objectives
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <p class="text-sm leading-relaxed text-foreground/90 whitespace-pre-wrap">
                                {{
                                    selectedVersion.aimAndObjectives ||
                                    "No aims and objectives specified."
                                }}
                            </p>
                        </CardContent>
                    </Card>

                    <!-- Course Content -->
                    <Card>
                        <CardHeader class="pb-3">
                            <CardTitle class="text-base font-semibold flex items-center gap-2">
                                <ListOrdered class="h-4 w-4 text-primary" />
                                Course Content
                            </CardTitle>
                        </CardHeader>
                        <CardContent>
                            <div class="text-sm leading-relaxed text-foreground/90 font-mono text-[13px] bg-muted/30 p-4 rounded-lg border whitespace-pre-wrap"
                                v-text="displayCourseContent"></div>
                        </CardContent>
                    </Card>

                    <!-- Course Relationships -->
                    <Card>
                        <CardHeader class="pb-3">
                            <div class="flex items-center justify-between">
                                <CardTitle class="text-base font-semibold flex items-center gap-2">
                                    <Link2 class="h-4 w-4 text-primary" />
                                    Course Relationships
                                </CardTitle>
                                <Badge variant="secondary" class="text-xs">
                                    {{ totalLinkedCourses }} linked course{{
                                        totalLinkedCourses !== 1 ? "s" : ""
                                    }}
                                </Badge>
                            </div>
                        </CardHeader>
                        <CardContent>
                            <div class="grid gap-6 md:grid-cols-2">
                                <div class="space-y-3">
                                    <div class="flex items-center gap-2 text-sm font-semibold">
                                        <ArrowLeft class="h-4 w-4 text-primary" />
                                        <span>Pre-requisites</span>
                                        <Badge variant="outline" class="text-[10px]">
                                            {{ preRequisites.length }}
                                        </Badge>
                                    </div>
                                    <p v-if="preRequisites.length === 0" class="text-sm text-muted-foreground">
                                        No pre-requisite courses.
                                    </p>
                                    <div v-else class="space-y-2">
                                        <NuxtLink v-for="pre in preRequisites"
                                            :key="pre.id || pre.courseNumber || pre.name"
                                            :to="pre.id ? `/courses/${pre.id}` : '#'"
                                            class="flex items-start justify-between rounded-md border p-3 hover:bg-muted/50 transition-colors">
                                            <div class="space-y-1">
                                                <p class="text-sm font-semibold">
                                                    {{ formatCourseCode(pre) }}
                                                </p>
                                                <p class="text-sm text-muted-foreground">
                                                    {{ pre.name || "Untitled course" }}
                                                </p>
                                            </div>
                                            <ArrowUpRight class="h-4 w-4 text-muted-foreground" />
                                        </NuxtLink>
                                    </div>
                                </div>
                                <div class="space-y-3">
                                    <div class="flex items-center gap-2 text-sm font-semibold">
                                        <Ban class="h-4 w-4 text-primary" />
                                        <span>Anti-requisites</span>
                                        <Badge variant="outline" class="text-[10px]">
                                            {{ antiRequisites.length }}
                                        </Badge>
                                    </div>
                                    <p v-if="antiRequisites.length === 0" class="text-sm text-muted-foreground">
                                        No anti-requisite courses.
                                    </p>
                                    <div v-else class="space-y-2">
                                        <NuxtLink v-for="anti in antiRequisites"
                                            :key="anti.id || anti.courseNumber || anti.name"
                                            :to="anti.id ? `/courses/${anti.id}` : '#'"
                                            class="flex items-start justify-between rounded-md border p-3 hover:bg-muted/50 transition-colors">
                                            <div class="space-y-1">
                                                <p class="text-sm font-semibold">
                                                    {{ formatCourseCode(anti) }}
                                                </p>
                                                <p class="text-sm text-muted-foreground">
                                                    {{ anti.name || "Untitled course" }}
                                                </p>
                                            </div>
                                            <ArrowUpRight class="h-4 w-4 text-muted-foreground" />
                                        </NuxtLink>
                                    </div>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <!-- CILOs -->
                    <Card>
                        <CardHeader class="pb-3">
                            <div class="flex items-center justify-between">
                                <CardTitle class="text-base font-semibold flex items-center gap-2">
                                    <Lightbulb class="h-4 w-4 text-primary" />
                                    Course Intended Learning Outcomes (CILOs)
                                </CardTitle>
                                <Badge variant="secondary" class="text-xs">
                                    {{ selectedVersion.cilOs?.length ?? 0 }} outcome{{
                                        selectedVersion.cilOs?.length !== 1 ? "s" : ""
                                    }}
                                </Badge>
                            </div>
                        </CardHeader>
                        <CardContent>
                            <p v-if="
                                !selectedVersion.cilOs || selectedVersion.cilOs.length === 0
                            " class="text-sm text-muted-foreground">
                                No CILOs defined.
                            </p>
                            <div v-else class="space-y-3">
                                <div v-for="cilo in selectedVersion.cilOs" :key="cilo.code"
                                    class="flex gap-3 p-3 rounded-lg bg-muted/30 border border-border/50">
                                    <Badge variant="outline"
                                        class="h-6 shrink-0 bg-primary/5 text-primary border-primary/20 font-semibold">
                                        {{ cilo.code }}
                                    </Badge>
                                    <p class="text-sm leading-relaxed">{{ cilo.description }}</p>
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <!-- TLAs -->
                    <Card>
                        <CardHeader class="pb-3">
                            <div class="flex items-center justify-between">
                                <CardTitle class="text-base font-semibold flex items-center gap-2">
                                    <Users class="h-4 w-4 text-primary" />
                                    Teaching & Learning Activities (TLAs)
                                </CardTitle>
                                <Badge variant="secondary" class="text-xs">
                                    {{ selectedVersion.tlAs?.length ?? 0 }} activit{{
                                        selectedVersion.tlAs?.length !== 1 ? "ies" : "y"
                                    }}
                                </Badge>
                            </div>
                        </CardHeader>
                        <CardContent>
                            <p v-if="
                                !selectedVersion.tlAs || selectedVersion.tlAs.length === 0
                            " class="text-sm text-muted-foreground">
                                No TLAs defined.
                            </p>
                            <div v-else class="overflow-x-auto">
                                <table class="w-full text-sm">
                                    <thead>
                                        <tr class="border-b">
                                            <th class="text-left py-2 px-3 font-medium text-muted-foreground">
                                                Activity
                                            </th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        <tr v-for="tla in selectedVersion.tlAs" :key="tla.description"
                                            class="border-b border-border/50 last:border-0">
                                            <td class="py-3 px-3 font-medium">
                                                {{ tla.description }}
                                            </td>

                                        </tr>
                                    </tbody>
                                </table>
                            </div>
                        </CardContent>
                    </Card>

                    <!-- Assessment Methods -->
                    <Card>
                        <CardHeader class="pb-3">
                            <div class="flex items-center justify-between">
                                <CardTitle class="text-base font-semibold flex items-center gap-2">
                                    <ClipboardCheck class="h-4 w-4 text-primary" />
                                    Assessment Methods
                                </CardTitle>
                                <Badge variant="secondary" class="text-xs">
                                    {{ selectedVersion.assessments?.length ?? 0 }} method{{
                                        selectedVersion.assessments?.length !== 1 ? "s" : ""
                                    }}
                                </Badge>
                            </div>
                        </CardHeader>
                        <CardContent>
                            <p v-if="
                                !selectedVersion.assessments ||
                                selectedVersion.assessments.length === 0
                            " class="text-sm text-muted-foreground">
                                No assessment methods defined.
                            </p>
                            <template v-else>
                                <div class="overflow-x-auto">
                                    <table class="w-full text-sm">
                                        <thead>
                                            <tr class="border-b">
                                                <th class="text-left py-2 px-3 font-medium text-muted-foreground">
                                                    Assessment
                                                </th>
                                                <th class="text-left py-2 px-3 font-medium text-muted-foreground">
                                                    Category
                                                </th>
                                                <th class="text-right py-2 px-3 font-medium text-muted-foreground">
                                                    Weighting
                                                </th>
                                                <th class="text-left py-2 px-3 font-medium text-muted-foreground">
                                                    Description
                                                </th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            <tr v-for="am in selectedVersion.assessments" :key="am.id"
                                                class="border-b border-border/50 last:border-0">
                                                <td class="py-3 px-3 font-medium">{{ am.name }}</td>
                                                <td class="py-3 px-3">
                                                    <Badge variant="outline" class="text-[10px]">
                                                        {{
                                                            categoryLabels[String(am.category)] ??
                                                            am.category ??
                                                            "—"
                                                        }}
                                                    </Badge>
                                                </td>
                                                <td class="py-3 px-3 text-right font-mono font-medium">
                                                    {{ am.weighting }}%
                                                </td>
                                                <td class="py-3 px-3 text-muted-foreground">
                                                    {{ am.description || "—" }}
                                                </td>
                                            </tr>
                                        </tbody>
                                        <tfoot>
                                            <tr class="bg-muted/30">
                                                <td colspan="2" class="py-3 px-3 font-semibold">
                                                    Total Weighting
                                                </td>
                                                <td class="py-3 px-3 text-right font-mono font-bold">
                                                    {{ totalWeighting }}%
                                                </td>
                                                <td class="py-3 px-3">
                                                    <span v-if="isWeightingValid"
                                                        class="flex items-center gap-1 text-emerald-600 text-xs">
                                                        <CheckCircle2 class="h-3.5 w-3.5" />
                                                        Valid
                                                    </span>
                                                    <span v-else class="flex items-center gap-1 text-amber-600 text-xs">
                                                        <AlertTriangle class="h-3.5 w-3.5" />
                                                        Should equal 100%
                                                    </span>
                                                </td>
                                            </tr>
                                        </tfoot>
                                    </table>
                                </div>
                            </template>
                        </CardContent>
                    </Card>
                </template>

                <!-- No versions available -->
                <div v-else-if="!course.versions || course.versions.length === 0"
                    class="flex items-center justify-center min-h-[200px] border rounded-lg bg-muted/20">
                    <div class="text-center">
                        <p class="text-muted-foreground">
                            No versions available for this course.
                        </p>
                    </div>
                </div>

                <!-- Footer -->
                <div v-if="selectedVersion" class="flex items-center justify-center pt-6 border-t">
                    <div class="text-sm text-muted-foreground">
                        Viewing version:
                        <span class="font-medium">Version {{ selectedVersion.versionNumber }}</span>
                    </div>
                </div>
            </div>
        </main>
    </div>
</template>
