<script setup lang="ts">
import { BookMarked } from "lucide-vue-next";
import { generateAcademicYearDict } from "~/lib/facts/utils";
import { AvailableTermLabels } from "~/types/static/facts";
import type { components } from "~/API/schema";

const yearList = generateAcademicYearDict(2018, new Date().getFullYear() + 5);

const versionData = defineModel<
  components["schemas"]["CreateCourseVersionDto"]
>({
  required: true,
});
</script>

<template>
  <Card>
    <CardHeader class="pb-4">
      <div class="flex items-center gap-2">
        <BookMarked class="h-5 w-5 text-primary" />
        <div>
          <CardTitle class="text-base font-semibold"
            >Version Metadata</CardTitle
          >
        </div>
      </div>
    </CardHeader>
    <CardContent class="space-y-4">
      <div class="grid grid-rows-2 gap-4">
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label>From Academic Year</Label>
            <Select v-model="versionData.fromYear">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select Academic Year" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem
                  v-for="(year, index) in yearList"
                  :key="index"
                  :value="index"
                  >{{ year }}</SelectItem
                >
              </SelectContent>
            </Select>
          </div>
          <div class="space-y-2">
            <Label>From Term</Label>
            <Select v-model="versionData.fromTermId">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select Term" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem
                  v-for="(term, index) in AvailableTermLabels"
                  :key="index"
                  :value="index"
                  >{{ term }}</SelectItem
                >
              </SelectContent>
            </Select>
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label>To Academic Year</Label>
            <Select v-model="versionData.toYear">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select Academic Year" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem :value="null">Present</SelectItem>
                <SelectItem
                  v-for="(year, index) in yearList"
                  :key="index"
                  :value="index"
                  >{{ year }}</SelectItem
                >
              </SelectContent>
            </Select>
          </div>
          <div class="space-y-2">
            <Label>To Term</Label>
            <Select v-model="versionData.toTermId">
              <SelectTrigger class="w-full">
                <SelectValue placeholder="Select Term" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem :value="null">Present</SelectItem>
                <SelectItem
                  v-for="(term, index) in AvailableTermLabels"
                  :key="index"
                  :value="index"
                  >{{ term }}</SelectItem
                >
              </SelectContent>
            </Select>
          </div>
        </div>
      </div>
    </CardContent>
  </Card>
</template>
