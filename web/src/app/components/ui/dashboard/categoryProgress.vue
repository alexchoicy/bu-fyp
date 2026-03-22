<script setup lang="ts">
import { computed } from 'vue'
import { BarChart } from 'echarts/charts'
import {
  GridComponent,
  LegendComponent,
  TooltipComponent,
} from 'echarts/components'
import { use } from 'echarts/core'
import type { EChartsOption } from 'echarts'
import { SVGRenderer } from 'echarts/renderers'
import type { components } from '~/API/schema'
import VChart from 'vue-echarts'

type CategoryCompletionStatus = components['schemas']['CategoryCompletionStatus']
type ProgrammeCategoryDetailDto = components['schemas']['ProgrammeCategoryDetailDto']

type CategoryProgressPoint = {
  id: number
  name: string
  type: string
  usedCredits: number
  requiredCredits: number
  completionPercent: number
  remainingCredits: number
  isCompleted: boolean
  priority: number
}

use([SVGRenderer, BarChart, GridComponent, TooltipComponent, LegendComponent])

const props = defineProps<{
  programmeCategories: ProgrammeCategoryDetailDto[]
  categoryProgress: CategoryCompletionStatus[]
}>()

const toNumber = (value: number | string | null | undefined) => {
  if (typeof value === 'number') {
    return Number.isFinite(value) ? value : 0
  }

  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : 0
}

const progressData = computed<CategoryProgressPoint[]>(() => {
  const progressByCategoryId = new Map<number, CategoryCompletionStatus>()

  for (const detail of props.categoryProgress) {
    const id = toNumber(detail.id)
    if (id > 0) {
      progressByCategoryId.set(id, detail)
    }
  }

  const categoryIdsInProgramme = new Set<number>()

  const rowsFromProgramme = props.programmeCategories.map((category, index) => {
    const id = toNumber(category.categoryId)
    if (id > 0) {
      categoryIdsInProgramme.add(id)
    }

    const detail = progressByCategoryId.get(id)
    const requiredCredits = Math.max(0, toNumber(category.minCredit ?? detail?.minCredit))
    const usedCreditsRaw = Math.max(0, toNumber(detail?.usedCredits))
    const completionPercent = requiredCredits > 0
      ? Math.min(100, Number(((usedCreditsRaw / requiredCredits) * 100).toFixed(2)))
      : 0

    return {
      id,
      name: category.name?.trim() || detail?.name?.trim() || `Category ${index + 1}`,
      type: category.type ?? 'Category',
      usedCredits: usedCreditsRaw,
      requiredCredits,
      completionPercent,
      remainingCredits: Math.max(requiredCredits - usedCreditsRaw, 0),
      isCompleted: Boolean(detail?.isCompleted) || (requiredCredits > 0 && usedCreditsRaw >= requiredCredits),
      priority: toNumber(category.priority ?? detail?.priority),
    }
  })

  const rowsFromProgress = props.categoryProgress
    .filter((detail) => {
      const id = toNumber(detail.id)
      return id <= 0 || !categoryIdsInProgramme.has(id)
    })
    .map((detail, index) => {
      const requiredCredits = Math.max(0, toNumber(detail.minCredit))
      const usedCreditsRaw = Math.max(0, toNumber(detail.usedCredits))
      const completionPercent = requiredCredits > 0
        ? Math.min(100, Number(((usedCreditsRaw / requiredCredits) * 100).toFixed(2)))
        : 0

      return {
        id: toNumber(detail.id),
        name: detail.name?.trim() || `Category ${rowsFromProgramme.length + index + 1}`,
        type: 'Category',
        usedCredits: usedCreditsRaw,
        requiredCredits,
        completionPercent,
        remainingCredits: Math.max(requiredCredits - usedCreditsRaw, 0),
        isCompleted: Boolean(detail.isCompleted) || (requiredCredits > 0 && usedCreditsRaw >= requiredCredits),
        priority: toNumber(detail.priority),
      }
    })

  return [...rowsFromProgramme, ...rowsFromProgress]
    .sort((left, right) => right.priority - left.priority || left.name.localeCompare(right.name))
})

const hasData = computed(() => progressData.value.length > 0)

const chartHeight = computed(() => {
  const rowCount = progressData.value.length
  if (rowCount <= 0) {
    return 288
  }

  return Math.min(420, Math.max(220, rowCount * 54 + 84))
})

const categoryLabels = computed(() => progressData.value.map(item => item.name))
const completedValues = computed(() => progressData.value.map(item => item.completionPercent))
const remainingValues = computed(() => progressData.value.map(item => Number((100 - item.completionPercent).toFixed(2))))

const chartOptions = computed<EChartsOption>(() => ({
  color: ['#0f766e', 'rgba(148, 163, 184, 0.32)'],
  grid: {
    top: 16,
    right: 20,
    bottom: 44,
    left: 24,
    containLabel: true,
  },
  legend: {
    bottom: 0,
    itemWidth: 10,
    itemHeight: 10,
    textStyle: {
      color: 'var(--muted-foreground)',
    },
  },
  tooltip: {
    trigger: 'axis',
    axisPointer: {
      type: 'shadow',
    },
    backgroundColor: 'var(--card)',
    borderColor: 'var(--border)',
    borderWidth: 1,
    textStyle: {
      color: 'var(--card-foreground)',
    },
    formatter: (params: unknown) => {
      const points = (Array.isArray(params) ? params : [params]) as Array<{ dataIndex: number }>
      const category = progressData.value[points[0]?.dataIndex ?? -1]

      if (!category) {
        return ''
      }

      return [
        `<div class="space-y-1">`,
        `<div class="font-medium">${category.name}</div>`,
        `<div>Category type: ${category.type}</div>`,
        `<div>Completed: ${category.completionPercent.toFixed(2)}%</div>`,
        `<div>Credits: ${category.usedCredits}/${category.requiredCredits}</div>`,
        `<div>Remaining: ${category.remainingCredits}</div>`,
        `</div>`,
      ].join('')
    },
  },
  xAxis: {
    type: 'value',
    min: 0,
    max: 100,
    axisLabel: {
      color: 'var(--muted-foreground)',
      formatter: (value: number) => `${value.toFixed(0)}%`,
    },
    splitLine: {
      lineStyle: {
        color: 'rgba(148, 163, 184, 0.2)',
      },
    },
  },
  yAxis: {
    type: 'category',
    inverse: true,
    data: categoryLabels.value,
    axisTick: {
      show: false,
    },
    axisLine: {
      show: false,
    },
    axisLabel: {
      color: 'var(--muted-foreground)',
      width: 148,
      overflow: 'truncate',
      ellipsis: '...',
      margin: 12,
    },
  },
  series: [
    {
      name: 'Completed',
      type: 'bar',
      stack: 'progress',
      barWidth: 18,
      data: completedValues.value,
      itemStyle: {
        borderRadius: [6, 0, 0, 6],
      },
      label: {
        show: true,
        position: 'insideLeft',
        color: '#ffffff',
        formatter: ({ value }: { value: number }) => (value >= 16 ? `${value.toFixed(0)}%` : ''),
      },
    },
    {
      name: 'Remaining',
      type: 'bar',
      stack: 'progress',
      barWidth: 18,
      data: remainingValues.value,
      itemStyle: {
        borderRadius: [0, 6, 6, 0],
      },
    },
  ],
}))
</script>

<template>
  <div class="w-full" :style="{ height: `${chartHeight}px` }">
    <VChart v-if="hasData" :option="chartOptions" autoresize class="h-full w-full" />
    <div
      v-else
      class="flex h-full items-center justify-center rounded-lg border border-dashed border-border bg-muted/30 px-4 text-center text-sm text-muted-foreground"
    >
      No programme category progress data is available yet.
    </div>
  </div>
</template>
