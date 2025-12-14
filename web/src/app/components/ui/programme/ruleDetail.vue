<script setup lang="ts">
import type { components } from '~/API/schema'

type RuleNode = components['schemas']['RuleNode']
type RuleRuleNode = components['schemas']['RuleNodeRuleRuleNode']
type GroupNode = components['schemas']['RuleNodeGroupRuleNode']
type FreeElectiveNode = components['schemas']['RuleNodeFreeElectiveRuleNode']

const props = defineProps<{
    node?: RuleNode
    depth?: number
    groups?: components['schemas']['CategoryGroupDetailDto'][]
    minCredits?: number
}>()

const depth = props.depth ?? 0

const isRule = (n: RuleNode): n is RuleRuleNode => n.type === 'rule'
const isGroup = (n: RuleNode): n is GroupNode => n.type === 'group'
const isFreeElective = (n: RuleNode): n is FreeElectiveNode => n.type === 'free_elective'

const operatorLabel = (n: RuleRuleNode): string => n.operator === "And" ? "ALL of the following" : "ANY of the following"
const operatorBadge = (n: RuleRuleNode): string => n.operator === "And" ? "All Required" : "Any One"

const groupFinder = (groupID: string) => {
    return props.groups?.find((g) => g.groupId === groupID)
}
const groupModeLabel = (n: GroupNode): string => n.courseSelectionMode === "AllOf" ? "Complete ALL courses" : "Complete ONE course"
const groupModeBadge = (n: GroupNode): string => n.courseSelectionMode === "AllOf" ? "All Of" : "One Of"

</script>

<template>
    <div v-if="!node" class="text-sm text-muted-foreground">
        Loading...
    </div>
    <div v-else-if="isRule(node)">
        <div class="space-y-3">
            <div class="flex items-center gap-2">
                <Badge :variant="node.operator === 'And' ? 'default' : 'secondary'">{{ operatorBadge(node) }}</Badge>
                <span class="text-sm text-muted-foreground">{{ operatorLabel(node) }}</span>
            </div>
            <div class="ml-4 border-l-2 border-muted pl-4 space-y-3">
                <RuleDetail v-for="(child, index) in node.children" :key="index" :node="child" :depth="depth + 1"
                    :min-credits="minCredits" :groups="groups" />
            </div>
        </div>
    </div>

    <div v-else-if="isGroup(node)">
        <div class="flex items-center gap-2 p-2 bg-muted/50 rounded-md">
            <Badge variant="outline">{{ groupModeBadge(node) }}</Badge>
            <span class="text-sm">
                {{ groupModeLabel(node) }} from <span class="font-medium">{{ groupFinder(node.groupID)?.groupName
                }}</span>
            </span>
        </div>
    </div>

    <div v-else-if="isFreeElective(node)">
        <div class="flex items-center gap-2 p-2 bg-muted/50 rounded-md">
            <Badge variant="outline" class="bg-blue-50 text-blue-700 border-blue-200">
                Free Elective
            </Badge>
            <span class="text-sm">
                Complete <span class="font-medium">{{ minCredits }} credits</span>
            </span>
        </div>
    </div>

</template>
