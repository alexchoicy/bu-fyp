<script setup lang="ts">
import type { paths } from "~/API/schema";
import { BookOpen, Search } from "lucide-vue-next";

defineProps({
    onOpen: Function
});

// Reactive state
const searchQuery = ref("");
const selectedCredits = ref("all");
const showInactive = ref("active");

// Fetch courses from API
const { data: courses } = await useAPI<
    paths["/api/courses"]["get"]["responses"]["200"]["content"]["application/json"]
>("courses");

// Filtered courses based on search and filters
const filteredCourses = computed(() => {
    if (!courses.value) return [];

    return courses.value.filter((course) => {
        // Filter by active status
        if (showInactive.value === "active" && !course.isActive) {
            return false;
        }

        // Filter by search query
        if (searchQuery.value) {
            const query = searchQuery.value.toLowerCase();
            const matchesName = course.name?.toLowerCase().includes(query);
            const matchesCode = `${course.codeTag} ${course.courseNumber}`
                .toLowerCase()
                .includes(query);
            const matchesDescription = course.description
                ?.toLowerCase()
                .includes(query);
            if (!matchesName && !matchesCode && !matchesDescription) {
                return false;
            }
        }

        // Filter by credits
        if (selectedCredits.value !== "all") {
            const credit = course.credit;
            if (credit !== parseInt(selectedCredits.value)) {
                return false;
            }
        }

        return true;
    });
});

</script>
<template>
    <div>
        <div class="flex flex-col gap-6 p-6">
            <!-- Page Header -->
            <div class="flex flex-col gap-2">
                <h2 class="text-2xl font-bold text-balance">Browse Course Catalog</h2>
                <p class="text-muted-foreground">
                    Explore all available courses and find the perfect fit for your
                    program.
                </p>
            </div>
            <Card>
                <CardHeader>
                    <CardTitle>Search & Filter</CardTitle>
                </CardHeader>
                <CardContent>
                    <div class="flex flex-col gap-4">
                        <div class="relative flex-1">
                            <Search class="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                            <Input v-model="searchQuery" placeholder="Search courses by name, code, or instructor..."
                                class="pl-10" />
                        </div>
                        <div class="flex flex-wrap gap-3">
                            <Select v-model="selectedCredits">
                                <SelectTrigger class="w-[180px]">
                                    <SelectValue placeholder="Credits" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="all">Any Credits</SelectItem>
                                    <SelectItem value="1">1 Credit</SelectItem>
                                    <SelectItem value="2">2 Credits</SelectItem>
                                    <SelectItem value="3">3 Credits</SelectItem>
                                </SelectContent>
                            </Select>
                            <Select v-model="showInactive">
                                <SelectTrigger class="w-[180px]">
                                    <SelectValue placeholder="Status" />
                                </SelectTrigger>
                                <SelectContent>
                                    <SelectItem value="active">Active Only</SelectItem>
                                    <SelectItem value="all">Show All</SelectItem>
                                </SelectContent>
                            </Select>
                        </div>
                    </div>
                </CardContent>
            </Card>
            <Card v-for="course in filteredCourses" :key="course.id" class="transition-shadow hover:shadow-md">
                <CardHeader>
                    <div class="flex items-start justify-between">
                        <div class="flex-1">
                            <div class="flex items-center gap-3">
                                <CardTitle class="font-mono text-lg">{{ course.codeTag }} {{ course.courseNumber
                                }}</CardTitle>
                                <Badge v-for="dept in course.departments" :key="dept" variant="secondary">
                                    {{ dept }}
                                </Badge>
                            </div>
                            <CardDescription class="mt-1 text-base font-medium text-foreground">
                                {{ course.name }}
                            </CardDescription>
                        </div>
                        <Button @click="onOpen(course.id!)">
                            View Details
                        </Button>
                    </div>
                </CardHeader>
                <CardContent class="space-y-4">
                    <p class="text-sm text-muted-foreground">
                        {{
                            course.description ??
                            course.versions?.[0]?.description ??
                            "No description available"
                        }}
                    </p>
                    <div class="flex flex-wrap gap-4 text-sm">
                        <div v-if="course.versions?.[0]" class="flex items-center gap-2">
                            <BookOpen class="h-4 w-4 text-muted-foreground" />
                            <span>{{ course.credit }} Credits</span>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    </div>
</template>
