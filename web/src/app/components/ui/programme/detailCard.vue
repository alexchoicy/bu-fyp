<script setup lang="ts">
import { CheckCircle2 } from 'lucide-vue-next';
import type { components } from '~/API/schema';


const props = defineProps<{
    programme: components['schemas']['UserProgrammeDetailDto'] | undefined,
    userProgrammeDetail: components['schemas']['CategoryCompletionStatus'][] | undefined,
    userStudies: components["schemas"]["UserCourseDto"][] | undefined,
}>();

const completedCategories = computed(() => {
    return props.userProgrammeDetail?.filter((cp) => cp.isCompleted).length ?? 0;
});

const totalCategories = computed(() => {
    return props.userProgrammeDetail?.length ?? 0;
});

const usedCredits = computed(() => {
    return props.userProgrammeDetail?.reduce((sum, cp) => sum + (cp.usedCredits ?? 0), 0);
})

</script>

<template>
    <Card>
        <CardHeader>
            <CardTitle>
                {{ programme?.programmeName }}
            </CardTitle>
            <CardDescription>
                {{ programme?.programmeDescription?.replace(/\r\n/g, '\n') }}
            </CardDescription>
        </CardHeader>
        <CardContent>
            <div class="p-4 bg-muted/30 rounded-lg border space-y-3">
                <div class="flex items-center justify-between">
                    <h3 class="font-semibold flex items-center gap-2">
                        <CheckCircle2 class="h-5 w-5 text-primary" />
                        Overall Progress
                    </h3>
                    <span class="text-sm text-muted-foreground">
                        {{ completedCategories }} of {{ totalCategories }} categories completed
                    </span>
                </div>
                <div class="space-y-2">
                    <div class="flex justify-between text-sm">
                        <span class="text-muted-foreground">Credits Earned</span>
                        <span class="font-medium">
                            {{ usedCredits }} / {{ programme?.totalCredits }} credits
                        </span>
                    </div>
                    <Progress :model-value="programme?.totalCredits ? (usedCredits / programme.totalCredits) * 100 : 0"
                        class="h-3" />
                </div>
            </div>
        </CardContent>
    </Card>
</template>
