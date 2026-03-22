<script setup lang="ts">
import { computed } from 'vue'
import { PieChart } from 'echarts/charts'
import {
  LegendComponent,
  TooltipComponent,
} from 'echarts/components'
import { use } from 'echarts/core'
import type { EChartsOption } from 'echarts'
import { SVGRenderer } from 'echarts/renderers'
import VChart from 'vue-echarts'

type AssessmentMixCategory = {
  category: string
  exposureCredits: number
  exposurePercentage: number
  performanceGpa: number
  assessmentCount: number
  courseCount: number
}

type AssessmentMixInsight = {
  bestCategory: string
  bestCategoryGpa: number
  weakestCategory: string
  weakestCategoryGpa: number
}

type AssessmentMixResponse = {
  coverageStartYear: number
  coverageStartTermId: number
  coverageStartTermName: string
  coverageEndYear: number
  coverageEndTermId: number
  coverageEndTermName: string
  courseCount: number
  termCount: number
  totalExposureCredits: number
  insight: AssessmentMixInsight | null
  categories: AssessmentMixCategory[]
}

use([SVGRenderer, PieChart, TooltipComponent, LegendComponent])

const { data: assessmentMix } = useAPI<AssessmentMixResponse>('me/assessment-mix')

const hasData = computed(() => (assessmentMix.value?.categories?.length ?? 0) > 0)

const coverageLabel = computed(() => {
  const startYear = Number(assessmentMix.value?.coverageStartYear ?? 0)
  const startTermName = assessmentMix.value?.coverageStartTermName?.trim()
  const endYear = Number(assessmentMix.value?.coverageEndYear ?? 0)
  const endTermName = assessmentMix.value?.coverageEndTermName?.trim()

  if (!startYear || !startTermName || !endYear || !endTermName) {
    return 'Full study timeline'
  }

  const startLabel = `${startYear}-${startYear + 1} ${startTermName}`
  const endLabel = `${endYear}-${endYear + 1} ${endTermName}`

  if (startLabel === endLabel) {
    return startLabel
  }

  return `${startLabel} to ${endLabel}`
})

const bestInsight = computed(() => {
  const insight = assessmentMix.value?.insight
  if (!insight?.bestCategory) {
    return null
  }

  return `${insight.bestCategory} has the strongest weighted GPA (${Number(insight.bestCategoryGpa).toFixed(2)}).`
})

const weakestInsight = computed(() => {
  const insight = assessmentMix.value?.insight
  if (!insight?.weakestCategory) {
    return null
  }

  return `${insight.weakestCategory} is the weakest weighted category (${Number(insight.weakestCategoryGpa).toFixed(2)}).`
})

const chartOptions = computed<EChartsOption>(() => ({
  color: ['#0f766e', '#2563eb', '#f59e0b', '#dc2626', '#7c3aed', '#0284c7', '#4d7c0f', '#7c2d12'],
  tooltip: {
    trigger: 'item',
    backgroundColor: 'var(--card)',
    borderColor: 'var(--border)',
    borderWidth: 1,
    textStyle: {
      color: 'var(--card-foreground)',
    },
    formatter: (params: unknown) => {
      const point = params as {
        data: AssessmentMixCategory & { value: number }
        marker: string
      }
      const data = point.data

      return [
        `<div class="space-y-1">`,
        `<div class="font-medium">${data.category}</div>`,
        `<div>${point.marker} Exposure share: ${Number(data.exposurePercentage).toFixed(2)}%</div>`,
        `<div>Weighted GPA: ${Number(data.performanceGpa).toFixed(2)}</div>`,
        `<div>Exposure credits: ${Number(data.exposureCredits).toFixed(2)}</div>`,
        `<div>Assessments: ${data.assessmentCount}</div>`,
        `<div>Courses: ${data.courseCount}</div>`,
        `</div>`,
      ].join('')
    },
  },
  legend: {
    bottom: 0,
    left: 'center',
    icon: 'circle',
    itemWidth: 10,
    itemHeight: 10,
    textStyle: {
      color: 'var(--muted-foreground)',
    },
  },
  series: [
    {
      name: 'Assessment Mix',
      type: 'pie',
      radius: ['44%', '72%'],
      center: ['50%', '42%'],
      minAngle: 3,
      avoidLabelOverlap: true,
      label: {
        show: true,
        color: 'var(--foreground)',
        formatter: ({ data }: { data: AssessmentMixCategory }) => `${data.category}\n${Number(data.exposurePercentage).toFixed(0)}%`,
      },
      labelLine: {
        length: 12,
        length2: 8,
      },
      data: (assessmentMix.value?.categories ?? []).map(item => ({
        ...item,
        value: Number(item.exposureCredits ?? 0),
      })),
    },
  ],
}))
</script>

<template>
  <div class="flex h-72 w-full flex-col">
    <p class="mb-2 text-xs text-muted-foreground">
      {{ coverageLabel }} · {{ assessmentMix?.termCount ?? 0 }} term(s) · {{ assessmentMix?.courseCount ?? 0 }} course(s)
    </p>
    <p class="mb-2 text-xs text-muted-foreground">
      Total weighted exposure credits: {{ Number(assessmentMix?.totalExposureCredits ?? 0).toFixed(2) }}
    </p>
    <VChart v-if="hasData" :option="chartOptions" autoresize class="h-full w-full" />
    <div
      v-else
      class="flex h-full items-center justify-center rounded-lg border border-dashed border-border bg-muted/30 px-4 text-center text-sm text-muted-foreground"
    >
      No assessment weighting data is available across your graded courses.
    </div>
    <p v-if="bestInsight" class="mt-2 text-xs text-muted-foreground">{{ bestInsight }}</p>
    <p v-if="weakestInsight" class="mt-1 text-xs text-muted-foreground">{{ weakestInsight }}</p>
  </div>
</template>
