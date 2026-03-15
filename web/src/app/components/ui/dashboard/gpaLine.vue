<script setup lang="ts">
import { LineChart } from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
} from 'echarts/components'
import { use } from 'echarts/core'
import type { EChartsOption } from 'echarts'
import { SVGRenderer } from 'echarts/renderers'
import type { components } from '~/API/schema'
import VChart from 'vue-echarts'

type GpaData = components['schemas']['SemesterGpaDto']

type GpaPoint = {
  label: string
  gpa: number
  credits: number
  termName: string
}

use([SVGRenderer, LineChart, GridComponent, TooltipComponent])

const props = defineProps<{
  gpaData: GpaData[]
}>()

const toNumber = (value: number | string | null | undefined) => {
  if (typeof value === 'number') {
    return Number.isFinite(value) ? value : 0
  }

  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : 0
}

const chartData = computed<GpaPoint[]>(() => {
  return props.gpaData.map((item) => {
    const year = toNumber(item.year)
    const termName = item.termName?.trim() || 'Unknown term'

    return {
      label: `${year}-${year + 1} ${termName}`,
      gpa: toNumber(item.gpa),
      credits: toNumber(item.creditsCompleted),
      termName,
    }
  })
})

const hasData = computed(() => chartData.value.length > 0)

const chartLabels = computed(() => chartData.value.map(item => item.label))
const chartValues = computed(() => chartData.value.map(item => item.gpa))

const chartOptions = computed<EChartsOption>(() => ({
  color: ['#d97706'],
  grid: {
    top: 24,
    right: 24,
    bottom: chartData.value.length > 4 ? 92 : 52,
    left: 52,
    containLabel: true,
  },
  tooltip: {
    trigger: 'axis',
    backgroundColor: 'var(--card)',
    borderColor: 'var(--border)',
    borderWidth: 1,
    textStyle: {
      color: 'var(--card-foreground)',
    },
    formatter: (params: unknown) => {
      const point = (Array.isArray(params) ? params[0] : params) as {
        data: number
        dataIndex: number
        marker: string
      }
      const data = chartData.value[point.dataIndex]

      if (!data) {
        return ''
      }

      return [
        `<div class="space-y-1">`,
        `<div class="font-medium">${data.label}</div>`,
        `<div>${point.marker} Semester GPA: ${data.gpa.toFixed(2)}</div>`,
        `<div>Completed credits: ${data.credits}</div>`,
        `</div>`,
      ].join('')
    },
  },
  xAxis: {
    type: 'category',
    boundaryGap: false,
    data: chartLabels.value,
    axisTick: {
      show: false,
    },
    axisLine: {
      lineStyle: {
        color: 'var(--border)',
      },
    },
    axisLabel: {
      color: 'var(--muted-foreground)',
      interval: 0,
      rotate: chartData.value.length > 4 ? 28 : 0,
      hideOverlap: false,
      margin: 16,
    },
  },
  yAxis: {
    type: 'value',
    min: 0,
    max: 4,
    interval: 1,
    axisLabel: {
      color: 'var(--muted-foreground)',
      formatter: (value: number) => value.toFixed(2),
      margin: 12,
    },
    splitLine: {
      lineStyle: {
        color: 'rgba(148, 163, 184, 0.2)',
      },
    },
  },
  series: [
    {
      name: 'Semester GPA',
      type: 'line',
      smooth: true,
      symbol: 'circle',
      symbolSize: 10,
      lineStyle: {
        width: 3,
      },
      itemStyle: {
        borderColor: '#ffffff',
        borderWidth: 2,
      },
      areaStyle: {
        color: 'rgba(217, 119, 6, 0.18)',
      },
      data: chartValues.value,
    },
  ],
}))
</script>

<template>
  <div class="h-72 w-full">
    <VChart v-if="hasData" :option="chartOptions" autoresize class="h-full w-full" />
    <div v-else
      class="flex h-full items-center justify-center rounded-lg border border-dashed border-border bg-muted/30 px-4 text-center text-sm text-muted-foreground">
      No completed semester GPA data is available yet.
    </div>
  </div>
</template>
