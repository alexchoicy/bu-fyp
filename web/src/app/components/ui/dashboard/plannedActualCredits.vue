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

type SuggestedScheduleResponseDto = components['schemas']['SuggestedScheduleResponseDto']
type UserCourseDto = components['schemas']['UserCourseDto']
type CurrentFactsResponseDto = components['schemas']['CurrentFactsResponseDto']

type TermCreditsPoint = {
  key: string
  label: string
  plannedCredits: number
  completedCredits: number
  enrolledCredits: number
}

use([SVGRenderer, BarChart, GridComponent, TooltipComponent, LegendComponent])

const { data: schedule } = useAPI<SuggestedScheduleResponseDto>('me/suggested-schedule')
const { data: studentCourses } = useAPI<UserCourseDto[]>('me/courses')
const { data: facts } = useAPI<CurrentFactsResponseDto>('/facts')

const toNumber = (value: number | string | null | undefined) => {
  if (typeof value === 'number') {
    return Number.isFinite(value) ? value : 0
  }

  const parsed = Number(value)
  return Number.isFinite(parsed) ? parsed : 0
}

const termCredits = computed<TermCreditsPoint[]>(() => {
  const plannedByTerm = new Map<string, { label: string, plannedCredits: number }>()
  const actualByTerm = new Map<string, { label: string, completedCredits: number, enrolledCredits: number }>()

  const currentAcademicYear = toNumber(facts.value?.currentAcademicYear)
  const currentStudyYear = toNumber(schedule.value?.currentStudyYear)

  for (const year of schedule.value?.years ?? []) {
    const studyYear = toNumber(year.studyYear)
    const derivedAcademicYear = currentAcademicYear > 0 && currentStudyYear > 0 && studyYear > 0
      ? currentAcademicYear - (currentStudyYear - studyYear)
      : 0

    for (const term of year.terms ?? []) {
      const termId = toNumber(term.termId)
      const termName = term.termName?.trim() || `Term ${termId || 0}`
      const plannedCredits = (term.items ?? []).reduce((sum, item) => {
        const credit = item.isFreeElective || item.isCoreElective ? item.credits : item.courseCredit
        return sum + toNumber(credit)
      }, 0)

      const keyYear = derivedAcademicYear > 0 ? derivedAcademicYear : studyYear
      const key = `${keyYear}-${termId}`
      const label = derivedAcademicYear > 0
        ? `${derivedAcademicYear}-${derivedAcademicYear + 1} ${termName}`
        : `Study Year ${studyYear} ${termName}`

      const existing = plannedByTerm.get(key)
      plannedByTerm.set(key, {
        label,
        plannedCredits: (existing?.plannedCredits ?? 0) + plannedCredits,
      })
    }
  }

  for (const course of studentCourses.value ?? []) {
    const year = toNumber(course.academicYear)
    const termId = toNumber(course.termNumber)
    const credit = Math.max(0, toNumber(course.credit))
    const status = course.status ?? ''

    if (year <= 0 || termId <= 0) {
      continue
    }

    if (!['Completed', 'Exemption', 'Enrolled'].includes(status)) {
      continue
    }

    const key = `${year}-${termId}`
    const existing = actualByTerm.get(key) ?? {
      label: `${year}-${year + 1} ${course.term?.trim() || `Term ${termId}`}`,
      completedCredits: 0,
      enrolledCredits: 0,
    }

    if (status === 'Enrolled') {
      existing.enrolledCredits += credit
    }
    else {
      existing.completedCredits += credit
    }

    actualByTerm.set(key, existing)
  }

  const allKeys = new Set<string>([...plannedByTerm.keys(), ...actualByTerm.keys()])

  return [...allKeys]
    .map((key) => {
      const planned = plannedByTerm.get(key)
      const actual = actualByTerm.get(key)

      return {
        key,
        label: planned?.label || actual?.label || key,
        plannedCredits: Number((planned?.plannedCredits ?? 0).toFixed(2)),
        completedCredits: Number((actual?.completedCredits ?? 0).toFixed(2)),
        enrolledCredits: Number((actual?.enrolledCredits ?? 0).toFixed(2)),
      }
    })
    .sort((left, right) => {
      const [leftYear, leftTerm] = left.key.split('-').map(Number)
      const [rightYear, rightTerm] = right.key.split('-').map(Number)

      if (leftYear !== rightYear) {
        return leftYear - rightYear
      }

      return leftTerm - rightTerm
    })
})

const hasData = computed(() => termCredits.value.length > 0)

const chartOptions = computed<EChartsOption>(() => ({
  color: ['#2563eb', '#16a34a', '#f59e0b'],
  grid: {
    top: 20,
    right: 20,
    bottom: termCredits.value.length > 4 ? 80 : 48,
    left: 48,
    containLabel: true,
  },
  legend: {
    top: 0,
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
      const row = termCredits.value[points[0]?.dataIndex ?? -1]

      if (!row) {
        return ''
      }

      const actualTotal = row.completedCredits + row.enrolledCredits
      const gap = Number((row.plannedCredits - actualTotal).toFixed(2))

      return [
        `<div class="space-y-1">`,
        `<div class="font-medium">${row.label}</div>`,
        `<div>Planned credits: ${row.plannedCredits}</div>`,
        `<div>Completed credits: ${row.completedCredits}</div>`,
        `<div>Enrolled credits: ${row.enrolledCredits}</div>`,
        `<div>Gap (planned - actual): ${gap}</div>`,
        `</div>`,
      ].join('')
    },
  },
  xAxis: {
    type: 'category',
    data: termCredits.value.map(item => item.label),
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
      rotate: termCredits.value.length > 4 ? 22 : 0,
      margin: 14,
    },
  },
  yAxis: {
    type: 'value',
    min: 0,
    axisLabel: {
      color: 'var(--muted-foreground)',
    },
    splitLine: {
      lineStyle: {
        color: 'rgba(148, 163, 184, 0.2)',
      },
    },
  },
  series: [
    {
      name: 'Planned',
      type: 'bar',
      barMaxWidth: 20,
      data: termCredits.value.map(item => item.plannedCredits),
      itemStyle: {
        borderRadius: [6, 6, 0, 0],
      },
    },
    {
      name: 'Completed',
      type: 'bar',
      barMaxWidth: 20,
      data: termCredits.value.map(item => item.completedCredits),
      itemStyle: {
        borderRadius: [6, 6, 0, 0],
      },
    },
    {
      name: 'Enrolled',
      type: 'bar',
      barMaxWidth: 20,
      data: termCredits.value.map(item => item.enrolledCredits),
      itemStyle: {
        borderRadius: [6, 6, 0, 0],
      },
    },
  ],
}))
</script>

<template>
  <div class="h-72 w-full">
    <VChart v-if="hasData" :option="chartOptions" autoresize class="h-full w-full" />
    <div
      v-else
      class="flex h-full items-center justify-center rounded-lg border border-dashed border-border bg-muted/30 px-4 text-center text-sm text-muted-foreground"
    >
      No planned or actual term credit data is available yet.
    </div>
  </div>
</template>
