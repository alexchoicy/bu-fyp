<script setup lang="ts">
import { CheckCircle2, Circle } from 'lucide-vue-next';
import type { components } from '~/API/schema';


const props = defineProps<{
    programmeCategory: components['schemas']["ProgrammeCategoryDetailDto"] | undefined,
    userProgrammeDetail: components['schemas']['CategoryCompletionStatus'] | undefined,
    userStudies: components["schemas"]["UserCourseDto"][] | undefined,
}>();


</script>

<template>
    <Card :class="{ 'border-green-500 bg-green-50/30': userProgrammeDetail?.isCompleted }">
        <CardHeader>
            <CardTitle class="flex items-center justify-between">
                {{ programmeCategory?.name }}
                <Circle v-if="!userProgrammeDetail?.isCompleted" />
                <CheckCircle2 v-else class="text-green-500" />
            </CardTitle>
            <CardDescription>
                {{ programmeCategory?.type }}
            </CardDescription>
        </CardHeader>
        <CardContent class="space-y-6">
            <div>
                <div class="flex justify-between">
                    <span class="text-sm text-muted-foreground">Credits Earned</span>
                    <span class="font-medium">
                        {{ userProgrammeDetail?.usedCredits }} / {{ programmeCategory?.minCredit }} credits
                    </span>
                </div>
                <Progress :model-value="(userProgrammeDetail?.usedCredits) / (programmeCategory?.minCredit) * 100"
                    class="h-3 mt-2" />
            </div>
            <div class="space-y-2">
                <h4 class="text-sm font-medium text-muted-foreground">Completion Rules</h4>
                <div class="p-4 bg-muted/30 rounded-lg border">
                    <UiProgrammeRuleDetail :node="userProgrammeDetail?.ruleNode" :groups="programmeCategory?.groups"
                        :min-credits="programmeCategory?.minCredit" />
                </div>
            </div>
            <Accordion type="single" collapsible>
                <AccordionItem v-for="(categoryGroup, index) in programmeCategory?.groups" :key="categoryGroup?.groupId"
                    :value="'group-' + index">
                    <AccordionTrigger>
                        {{ categoryGroup.groupName }}
                    </AccordionTrigger>
                    <AccordionContent>
                        <UiProgrammeGroupSection :category-group="categoryGroup" :userStudies="userStudies"
                            :user-programme-detail="userProgrammeDetail" />
                    </AccordionContent>
                </AccordionItem>
            </Accordion>
        </CardContent>
    </Card>
</template>
