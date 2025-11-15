<script setup lang="ts">
  import type { CourseSectionType, CourseSectionWeekday } from "@fyp/api/course/types";

  type TimeOption = {
    value: string;
    label: string;
  };

  defineProps({
    modelValue: {
      type: Object as () => {
        day: CourseSectionWeekday;
        sectionType: CourseSectionType;
        startTime: string;
        endTime: string;
        location: string;
      },
      required: true,
    },
  });

  const startTimeOptions = computed<TimeOption[]>(() => {
    const options: TimeOption[] = [];
    for (let hour = 8; hour <= 21; hour += 1) {
      const hourString = hour.toString().padStart(2, "0");
      const label = `${hourString}:30`;
      options.push({ value: label, label });
    }
    return options;
  });

  const endTimeOptions = computed<TimeOption[]>(() => {
    const options: TimeOption[] = [];
    for (let hour = 9; hour <= 22; hour += 1) {
      const hourString = hour.toString().padStart(2, "0");
      const label = `${hourString}:20`;
      options.push({ value: label, label });
    }
    return options;
  });
</script>

<template>
  <div class="relative rounded-lg border border-border p-4 space-y-4">
    <div class="grid grid-cols-4 gap-4">
      <div class="space-y-2">
        <Label>Day</Label>
        <Select v-model="modelValue.day">
          <SelectTrigger class="w-full">
            <SelectValue placeholder="Select day" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="MON">Monday</SelectItem>
            <SelectItem value="TUE">Tuesday</SelectItem>
            <SelectItem value="WED">Wednesday</SelectItem>
            <SelectItem value="THU">Thursday</SelectItem>
            <SelectItem value="FRI">Friday</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div class="space-y-2">
        <Label>Section Type</Label>
        <Select v-model="modelValue.sectionType">
          <SelectTrigger class="w-full">
            <SelectValue placeholder="Select type" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="LEC">Lecture</SelectItem>
            <SelectItem value="TUT">Tutorial</SelectItem>
            <SelectItem value="LAB">Lab</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div class="space-y-2">
        <Label>Start Time</Label>
        <Select v-model="modelValue.startTime">
          <SelectTrigger class="w-full">
            <SelectValue placeholder="Select start time" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="option in startTimeOptions" :key="option.value" :value="option.value">
              {{ option.label }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>

      <div class="space-y-2">
        <Label>End Time</Label>
        <Select v-model="modelValue.endTime">
          <SelectTrigger class="w-full">
            <SelectValue placeholder="Select end time" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem v-for="option in endTimeOptions" :key="option.value" :value="option.value">
              {{ option.label }}
            </SelectItem>
          </SelectContent>
        </Select>
      </div>
    </div>

    <div class="space-y-2 mt-4">
      <Label>Location</Label>
      <Input v-model="modelValue.location" placeholder="Enter location..." />
    </div>
  </div>
</template>
