<script setup lang="ts">
import { ref } from "vue";
import type { components } from "~/API/schema";
import { Check, ChevronsUpDown, X, Search } from "lucide-vue-next";

const uploadedFile = ref<File | null>(null);
const isUploading = ref(false);

//tell me do i need to use useAsyncData, i think don't
const { data: codes } =
  useAPI<components["schemas"]["CodeResponseDto"][]>("facts/codes");
const { data: departments } =
  useAPI<components["schemas"]["DepartmentResponseDto"][]>("facts/departments");
const { data: courseGroups } =
  useAPI<components["schemas"]["CourseGroupResponseDto"][]>("facts/groups");
const { data: courseList } =
  useAPI<components["schemas"]["SimpleCourseDto"][]>("courses/simple");

const courseVersionData = ref<components["schemas"]["CreateCourseVersionDto"]>({
  description: "",
  aimAndObjectives: "",
  courseContent: "",
  cilOs: [],
  tlAs: [],
  assessments: [],
  mediumOfInstructionIds: [],
  preRequisiteCourseVersionIds: [],
  antiRequisiteCourseVersionIds: [],
  fromYear: "",
  fromTermId: "",
  toYear: null,
  toTermId: null,
});

const createCourseData = ref<components["schemas"]["CreateCourseDto"]>({
  name: "",
  courseNumber: "",
  codeId: "",
  description: "",
  departmentIds: [],
  groupIds: [],
  isActive: true,
});

const handleParsedData = (
  parsedData: components["schemas"]["PdfParseResponseDto"]
) => {
  if (parsedData && courseVersionData.value && createCourseData.value) {
    courseVersionData.value = {
      ...courseVersionData.value,
      cilOs: parsedData.parsedSections?.cilOs || [],
      tlAs: parsedData.parsedSections?.tlAs || [],
      assessments: parsedData.parsedSections?.assessmentMethods || [],
      aimAndObjectives: parsedData.parsedSections?.aimsObjectives || "",
      courseContent: parsedData.parsedSections?.courseContent || "",
    };

    createCourseData.value.name = parsedData.parsedSections?.courseTitle || "";

    createCourseData.value.courseNumber =
      parsedData.parsedSections?.courseCodeNumber || "";

    const creditRaw = parsedData.parsedSections?.noOfUnits || "0";
    const creditMatch = creditRaw.match(/(\d+(\.\d+)?)/);
    createCourseData.value.credit = creditMatch && creditMatch[0]
      ? parseInt(creditMatch[0], 10)
      : 0;

    createCourseData.value.codeId = codes.value?.find((code) =>
      parsedData.parsedSections?.courseCode
        ? code.tag === parsedData.parsedSections.courseCode
        : false
    )?.id as string;

    const offeringDeptRaw = parsedData.parsedSections?.offeringDepartment || "";
    const deptNames = offeringDeptRaw
      .split("\n")
      .map((d) => d.trim())
      .filter(Boolean);
    createCourseData.value.departmentIds = deptNames
      .map((name) => departments.value?.find((dept) => dept.name === name)?.id)
      .filter((id): id is string | number => id !== undefined);

    selectedDepartments.value =
      departments.value?.filter((dept) =>
        createCourseData.value.departmentIds?.includes(dept.id!)
      ) || [];

    const preRequest = parsedData.parsedSections?.prerequisites || [];
    courseVersionData.value.preRequisiteCourseVersionIds =
      preRequest
        .map((preReqName) => {
          return courseList.value?.find((course) => {
            return (
              course.codeTag === preReqName.courseCode &&
              course.courseNumber === preReqName.courseCodeNumber
            );
          })?.mostRecentVersion?.id;
        })
        .filter((id): id is number => id !== undefined) || [];
  }
};

const selectedCode = computed(() => {
  if (!createCourseData.value.codeId || !codes.value) return null;
  return codes.value.find((c) => c.id === createCourseData.value.codeId);
});

const selectedDepartments = ref<
  components["schemas"]["DepartmentResponseDto"][]
>([]);

const selectedGroups = ref<components["schemas"]["CourseGroupResponseDto"][]>(
  []
);

watch(
  selectedGroups,
  (newGroups) => {
    createCourseData.value.groupIds =
      newGroups.map((group) => group.id!) || [];
  },
  { deep: true }
);

watch(
  selectedDepartments,
  (newDepts) => {
    createCourseData.value.departmentIds =
      newDepts.map((dept) => dept.id!) || [];
  },
  { deep: true }
);

function removeDepartment(
  dept: components["schemas"]["DepartmentResponseDto"]
) {
  selectedDepartments.value = selectedDepartments.value.filter(
    (d) => d.id !== dept.id
  );
}

function removeGroup(group: components["schemas"]["CourseGroupResponseDto"]) {
  selectedGroups.value = selectedGroups.value.filter((g) => g.id !== group.id);
}

const isSubmitting = ref(false);

async function submitForm() {
  if (
    !createCourseData.value.name ||
    !createCourseData.value.courseNumber ||
    !createCourseData.value.codeId
  ) {
    alert("Please fill in all required fields");
    return;
  }
  isSubmitting.value = true;
  const $api = useNuxtApp().$api;
  let newID: number | undefined;
  try {
    const response = await $api<{ id: number }>("courses", {
      method: "POST",
      body: createCourseData.value,
    });
    newID = response.id;
  } catch (error) {
    console.error("Failed to create course:", error);
    alert("Failed to create course");
  }

  if (!newID) {
    return;
  }

  try {
    await $api<{ id: number }>(`courses/${newID}/versions`, {
      method: "POST",
      body: courseVersionData.value,
    });
  } catch (error) {
    console.error("Failed to create course version:", error);
    alert("Failed to create course version");
  } finally {
    isSubmitting.value = false;
  }

  navigateTo('/admin/courses');
}
</script>

<template>
  <div class="container mx-auto py-8 max-w-4xl space-y-2">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold">Create New Course</h1>
      <p class="text-muted-foreground">Add a new course to the system</p>
    </div>
    <UiCoursePdfParse v-model:uploaded-file="uploadedFile" v-model:is-uploading="isUploading"
      v-model:course-version-data="courseVersionData" @parsed="handleParsedData" />
    <Card>
      <CardHeader>
        <CardTitle>Basic Information</CardTitle>
        <CardDescription>Enter the course details manually</CardDescription>
      </CardHeader>
      <CardContent class="space-y-4">
        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label for="name">Course Name *</Label>
            <Input id="name" v-model="createCourseData.name"
              placeholder="e.g., Introduction to Artificial Intelligence" />
          </div>
          <div class="space-y-2">
            <Label for="courseNumber">Course Number *</Label>
            <Input id="courseNumber" v-model="createCourseData.courseNumber" placeholder="e.g., 3001" />
          </div>
        </div>

        <div class="grid grid-cols-2 gap-4">
          <div class="space-y-2">
            <Label>Course Code *</Label>
            <Select v-model="createCourseData.codeId">
              <SelectTrigger>
                <SelectValue placeholder="Select a code" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem v-for="code in codes" :key="code.id" :value="code.id!">
                  <span class="font-mono font-medium">{{ code.tag }}</span>
                  <span class="text-muted-foreground ml-2">- {{ code.name }}</span>
                </SelectItem>
              </SelectContent>
            </Select>
            <p v-if="selectedCode" class="text-xs text-muted-foreground">
              Full code: {{
                selectedCode.tag
              }}{{ createCourseData.courseNumber }}
            </p>
          </div>
          <div>
            <NumberField id="credit" v-model="createCourseData.credit" :default-value="3" :min="0" :max="4">
              <Label for="credit">Credit *</Label>
              <NumberFieldContent>
                <NumberFieldDecrement />
                <NumberFieldInput />
                <NumberFieldIncrement />
              </NumberFieldContent>
            </NumberField>
          </div>
        </div>

        <div class="space-y-2">
          <Label>Departments *</Label>
          <Combobox v-model="selectedDepartments" multiple by="id">
            <ComboboxAnchor as-child>
              <ComboboxTrigger as-child>
                <Button variant="outline" class="justify-between w-full">
                  <span class="text-wrap">
                    {{
                      selectedDepartments.length > 0
                        ? `${selectedDepartments.length} department(s) selected`
                        : "Select departments..."
                    }}
                  </span>
                  <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
              </ComboboxTrigger>
            </ComboboxAnchor>
            <ComboboxList class="w-full">
              <div class="relative w-full max-w-sm items-center">
                <ComboboxInput class="focus-visible:ring-0 border-0 border-b rounded-none h-10"
                  placeholder="Select framework..." />
                <span class="absolute start-0 inset-y-0 flex items-center justify-center px-3">
                  <Search class="size-4 text-muted-foreground" />
                </span>
              </div>
              <ComboboxEmpty>No department found.</ComboboxEmpty>
              <ComboboxGroup>
                <ComboboxItem v-for="dept in departments" :key="dept.id" :value="dept">
                  {{ dept.name }}
                  <ComboboxItemIndicator>
                    <Check class="size-4" />
                  </ComboboxItemIndicator>
                </ComboboxItem>
              </ComboboxGroup>
            </ComboboxList>
          </Combobox>
          <div v-if="selectedDepartments.length > 0" class="flex flex-wrap gap-1 mb-2">
            <Badge v-for="dept in selectedDepartments" :key="dept.id" variant="secondary" class="text-xs">
              {{ dept.name }}
              <button type="button" class="ml-1 hover:text-destructive" @click="removeDepartment(dept)">
                <X class="h-3 w-3" />
              </button>
            </Badge>
          </div>
        </div>

        <div class="space-y-2">
          <Label>Course Groups</Label>
          <Combobox v-model="selectedGroups" multiple by="id">
            <ComboboxAnchor as-child>
              <ComboboxTrigger as-child>
                <Button variant="outline" class="justify-between w-full">
                  <span class="text-wrap">
                    {{
                      selectedGroups.length > 0
                        ? `${selectedGroups.length} group(s) selected`
                        : "Select course groups..."
                    }}
                  </span>
                  <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
                </Button>
              </ComboboxTrigger>
            </ComboboxAnchor>
            <ComboboxList class="w-full">
              <div class="relative w-full max-w-sm items-center">
                <ComboboxInput class="focus-visible:ring-0 border-0 border-b rounded-none h-10"
                  placeholder="Select framework..." />
                <span class="absolute start-0 inset-y-0 flex items-center justify-center px-3">
                  <Search class="size-4 text-muted-foreground" />
                </span>
              </div>
              <ComboboxEmpty>No group found.</ComboboxEmpty>
              <ComboboxGroup>
                <ComboboxItem v-for="group in courseGroups" :key="group.id" :value="group">
                  {{ group.name }}
                  <ComboboxItemIndicator>
                    <Check class="size-4" />
                  </ComboboxItemIndicator>
                </ComboboxItem>
              </ComboboxGroup>
            </ComboboxList>
          </Combobox>
          <div v-if="selectedGroups.length > 0" class="flex flex-wrap gap-1 mb-2">
            <Badge v-for="group in selectedGroups" :key="group.id" variant="secondary" class="text-xs">
              {{ group.name }}
              <button type="button" class="ml-1 hover:text-destructive" @click="removeGroup(group)">
                <X class="h-3 w-3" />
              </button>
            </Badge>
          </div>
        </div>

        <div class="space-y-2">
          <Label for="description">Description</Label>
          <Textarea id="description" v-model="createCourseData.description!" />
        </div>

        <div class="flex items-center justify-between rounded-lg border border-border p-4">
          <div class="space-y-0.5">
            <Label for="isActive">Active Course</Label>
            <p class="text-sm text-muted-foreground">
              Active courses are visible to students in the catalog
            </p>
          </div>
          <Switch id="isActive" v-model="createCourseData.isActive" />
        </div>
      </CardContent>
    </Card>

    <UiCourseVersionsCourseSelection v-model="courseVersionData.preRequisiteCourseVersionIds!"
      :course-list="courseList || []" title="PreRequest"
      description="Select prerequisite courses for this course version." />

    <UiCourseVersionsEdit v-model="courseVersionData" />

    <div class="flex justify-end">
      <Button :disabled="isSubmitting || isUploading" @click="submitForm">{{
        isSubmitting
          ? "Submitting..."
          : isUploading
            ? "Uploading..."
            : "Create Course"
      }}
      </Button>
    </div>
  </div>
</template>
