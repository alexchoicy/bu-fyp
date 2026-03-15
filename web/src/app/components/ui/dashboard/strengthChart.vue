<script setup lang="ts">
import { computed } from 'vue'
import { BarChart } from 'echarts/charts'
import {
  GridComponent,
  TooltipComponent,
} from 'echarts/components'
import { use } from 'echarts/core'
import type { EChartsOption } from 'echarts'
import { SVGRenderer } from 'echarts/renderers'
import type { components } from '~/API/schema'
import VChart from 'vue-echarts'

type UserCourse = components['schemas']['UserCourseDto']
type Grade = components['schemas']['Grade']

type StrengthPoint = {
  subject: string
  averageGpa: number
  totalCredits: number
  courseCount: number
}

const gradePoints: Partial<Record<Exclude<Grade, null>, number>> = {
  A: 4,
  AMinus: 3.67,
  BPlus: 3.33,
  B: 3,
  BMinus: 2.67,
  CPlus: 2.33,
  C: 2,
  CMinus: 1.67,
  D: 1,
  E: 0,
  F: 0,
}

const chartColors = ['#0f766e', '#0284c7', '#ca8a04', '#9333ea', '#dc2626', '#4f46e5']

use([SVGRenderer, BarChart, GridComponent, TooltipComponent])

const props = defineProps<{
  courses: UserCourse[]
}>()

const toNumber = (value: number | string | null | undefined) => {
  if (typeof value === 'number') {
    return Number.isFinite(value) ? value : 0
  }

  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : 0
}

const strengthData = computed<StrengthPoint[]>(() => {
  const grouped = new Map<string, { gradePoints: number; credits: number; courses: number }>()

  for (const course of props.courses) {
    const grade = course.grade
    if (!grade) {
      continue
    }

    const gradePoint = gradePoints[grade]
    if (gradePoint === undefined) {
      continue
    }

    const credit = toNumber(course.credit)
    if (credit <= 0) {
      continue
    }

    const subject = course.codeTag?.trim() || 'Other'
    const current = grouped.get(subject) ?? { gradePoints: 0, credits: 0, courses: 0 }

    current.gradePoints += gradePoint * credit
    current.credits += credit
    current.courses += 1

    grouped.set(subject, current)
  }

  return [...grouped.entries()]
    .map(([subject, values]) => ({
      subject,
      averageGpa: Number((values.gradePoints / values.credits).toFixed(2)),
      totalCredits: values.credits,
      courseCount: values.courses,
    }))
    .sort((left, right) => right.averageGpa - left.averageGpa)
    .slice(0, 8)
})

const hasData = computed(() => strengthData.value.length > 0)

const chartOptions = computed<EChartsOption>(() => ({
  grid: {
    top: 8,
    right: 24,
    bottom: 8,
    left: 44,
    containLabel: true,
  },
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
        data: StrengthPoint
        color: string
      }
      const data = point.data

      return [
        `<div class="space-y-1">`,
        `<div class="font-medium">${data.subject}</div>`,
        `<div><span style="display:inline-block;margin-right:6px;height:10px;width:10px;border-radius:9999px;background:${point.color}"></span>Weighted GPA: ${data.averageGpa.toFixed(2)}</div>`,
        `<div>Completed credits: ${data.totalCredits}</div>`,
        `<div>Courses counted: ${data.courseCount}</div>`,
        `</div>`,
      ].join('')
    },
  },
  xAxis: {
    type: 'value',
    min: 0,
    max: 4,
    interval: 1,
    axisLabel: {
      color: 'var(--muted-foreground)',
      formatter: (value: number) => value.toFixed(2),
    },
    splitLine: {
      lineStyle: {
        color: 'rgba(148, 163, 184, 0.2)',
      },
    },
  },
  yAxis: {
    type: 'category',
    data: strengthData.value.map(item => item.subject),
    axisTick: {
      show: false,
    },
    axisLine: {
      show: false,
    },
    axisLabel: {
      color: 'var(--muted-foreground)',
    },
  },
  series: [
    {
      type: 'bar',
      barWidth: 18,
      data: strengthData.value.map((item, index) => ({
        ...item,
        value: item.averageGpa,
        itemStyle: {
          borderRadius: [0, 8, 8, 0],
          color: chartColors[index % chartColors.length],
        },
      })),
      label: {
        show: true,
        position: 'right',
        color: 'var(--foreground)',
        formatter: ({ value }: { value: number }) => value.toFixed(2),
      },
    },
  ],
}))
</script>

<template>
  <div class="h-72 w-full">
    <VChart
      v-if="hasData"
      :option="chartOptions"
      autoresize
      class="h-full w-full"
    />
    <div
      v-else
      class="flex h-full items-center justify-center rounded-lg border border-dashed border-border bg-muted/30 px-4 text-center text-sm text-muted-foreground"
    >
      No GPA-eligible course results are available to compare yet.
    </div>
  </div>
</template>
