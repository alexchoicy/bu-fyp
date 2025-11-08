<script setup lang="ts">
  import type { Course, CourseCode } from "@fyp/api/course/types";
  import type { Group } from "@fyp/api/program/types";
  import { Check, ChevronsUpDown, Search } from "lucide-vue-next";

  const courseData = ref<Course>({
    name: "",
    credit: 0,
    is_active: true,
    groups: [],
    courseNumber: 1234,
    code: undefined,
    prerequisites: [],
    antiRequisites: [],
  } as Course);

  const { data: tags } = await useAPI<CourseCode[]>("tags");

  const createCourseID = computed(() => {
    if (courseData.value.code && courseData.value.courseNumber) {
      return `${courseData.value.code.tag}${courseData.value.courseNumber}`;
    }
    return "";
  });

  const { data: availableGroup } = useAPI<Group[]>("groups");

  async function createCourse() {
    await useNuxtApp().$backend("courses", {
      method: "POST",
      body: courseData.value,
    });
  }
</script>

<template>
  <div class="space-y-6">
    <Card>
      <CardHeader>
        <CardTitle>Course Information</CardTitle>
      </CardHeader>
      <CardContent>
        <div>
          <div class="grid grid-cols-1 md:grid-cols-12 gap-4">
            <div class="md:col-span-3 space-y-2">
              <Label>Course Tag</Label>
              <Combobox v-model="courseData.code" by="id">
                <ComboboxAnchor as-child>
                  <ComboboxTrigger as-child>
                    <Button variant="outline" class="justify-between w-full">
                      <span class="text-wrap">
                        {{ courseData.code ? courseData.code.tag + " - " + courseData.code.name : "Select tag..." }}
                      </span>
                      <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
                    </Button>
                  </ComboboxTrigger>
                </ComboboxAnchor>
                <ComboboxList class="w-full">
                  <div class="relative w-full max-w-sm items-center">
                    <ComboboxInput
                      class="focus-visible:ring-0 border-0 border-b rounded-none h-10"
                      placeholder="Input Tag..." />
                    <span class="absolute start-0 inset-y-0 flex items-center justify-center px-3">
                      <Search class="size-4 text-muted-foreground" />
                    </span>
                  </div>
                  <ComboboxEmpty>No tag found.</ComboboxEmpty>
                  <ComboboxGroup>
                    <ComboboxItem v-for="tag in tags" :key="tag.id" :value="tag">
                      {{ tag.tag }} - {{ tag.name }}
                      <ComboboxItemIndicator>
                        <Check class="size-4" />
                      </ComboboxItemIndicator>
                    </ComboboxItem>
                  </ComboboxGroup>
                </ComboboxList>
              </Combobox>
            </div>
            <div class="md:col-span-3 space-y-2">
              <Label>Course Number</Label>
              <Input
                v-model="courseData.courseNumber"
                type="number"
                placeholder="4 Number"
                maxlength="4"
                minlength="4" />
            </div>
            <div class="md:col-span-4 space-y-2">
              <div class="grid grid-cols-2">
                <div class="space-y-2">
                  <Label>Course ID</Label>
                  <div
                    v-if="courseData.code && courseData.courseNumber"
                    class="px-3 py-1 flex items-center border border-primary/20 rounded-lg">
                    <p class="font-mono font-bold text-primary">{{ createCourseID }}</p>
                  </div>
                </div>
                <div class="space-y-2">
                  <Label>Credits</Label>
                  <Input v-model="courseData.credit" type="number" placeholder="Credits" />
                </div>
              </div>
            </div>
            <div class="md:col-span-2 space-y-2">
              <Label>Status</Label>
              <div class="flex items-center gap-2 h-10">
                <Switch id="active-toggle" v-model="courseData.is_active" />
                <span class="text-sm font-medium">{{ courseData.is_active ? "Active" : "Inactive" }}</span>
              </div>
            </div>
          </div>
        </div>
        <div class="md:col-span-3 space-y-2">
          <Label>Name</Label>
          <Input v-model="courseData.name" type="text" placeholder="Course Name" />
        </div>
      </CardContent>
    </Card>

    <Card>
      <CardHeader>
        <CardTitle>Course Groups</CardTitle>
      </CardHeader>
      <CardContent>
        <div class="space-y-2">
          <div class="flex flex-wrap gap-2 mb-4">
            <Badge
              v-for="group in courseData.groups"
              :key="group.id"
              variant="secondary"
              class="text-sm py-1 hover:cursor-pointer">
              {{ group.name }}
            </Badge>
          </div>
        </div>
        <Combobox v-model="courseData.groups" multiple by="id">
          <ComboboxAnchor as-child>
            <ComboboxTrigger as-child>
              <Button variant="outline" class="justify-between w-full">
                <span class="text-wrap">Select Group...</span>
                <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
              </Button>
            </ComboboxTrigger>
          </ComboboxAnchor>
          <ComboboxList class="w-full">
            <ComboboxEmpty>No group found.</ComboboxEmpty>
            <ComboboxGroup>
              <ComboboxItem v-for="group in availableGroup" :key="group.id" :value="group">
                {{ group.name }}
                <ComboboxItemIndicator>
                  <Check class="size-4" />
                </ComboboxItemIndicator>
              </ComboboxItem>
            </ComboboxGroup>
          </ComboboxList>
        </Combobox>
      </CardContent>
    </Card>

    <div>
      <Button @click="createCourse">Create Course</Button>
    </div>
  </div>
  <pre>
    {{ courseData }}
  </pre>
</template>
