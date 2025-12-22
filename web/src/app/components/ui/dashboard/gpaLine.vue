<script setup lang="ts">
import type { components } from '~/API/schema'
import { VisAxis, VisLine, VisScatter, VisXYContainer } from '@unovis/vue'
import { CurveType } from '@unovis/ts'
import type { ChartConfig } from '~/components/shadcn/chart'

type GpaData = components["schemas"]["SemesterGpaDto"]

const props = defineProps<{
    gpaData: GpaData[]
}>()

const x = (_d: GpaData, i: number) => i
const y = (d: GpaData) => d.gpa

const tickFormat = (tick: number) => {
    const item = props.gpaData[tick]
    return item ? `${item.year} ${item.termName}` : ''
}

const chartConfig = {
    gpa: { label: 'GPA Over Time', color: 'var(--chart-1)' },
} satisfies ChartConfig


//This is weird, anyone can teach me

//move to using other charting library later
</script>

<template>
    <ChartContainer :config="chartConfig" class="h-60 w-full">
        <VisXYContainer :data="gpaData">
            <VisLine :curve-type="CurveType.Linear" />
            <VisScatter :x="x" :y="y" :size="10" />
            <VisAxis type="x" :tick-format="tickFormat" />
            <VisAxis type="y" :num-ticks="3" :tick-line="true" :domain-line="true" />
        </VisXYContainer>
    </ChartContainer>
</template>
