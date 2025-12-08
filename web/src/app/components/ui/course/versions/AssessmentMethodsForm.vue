<script setup lang="ts">
import {
  ClipboardList,
  Plus,
  Trash2,
  AlertCircle,
  CheckCircle,
} from "lucide-vue-next";
import type { components } from "~/API/schema";

const versionData = defineModel<
  components["schemas"]["CreateCourseVersionDto"]
>({
  required: true,
});

type Assessment = components["schemas"]["CreateAssessmentDto"];

// Assessment categories mapping (matches backend enum)
const AssessmentCategory = {
  1: "Examination",
  2: "Assignment",
  3: "Project",
  4: "Group Project",
  5: "Solo Project",
  6: "Participation",
  7: "Presentation",
  99: "Other",
} as const;

const totalWeighting = computed(() => {
  if (!versionData.value.assessments) return 0;
  return versionData.value.assessments.reduce((sum, item) => {
    const weight =
      typeof item.weighting === "string"
        ? parseFloat(item.weighting)
        : item.weighting;
    return sum + (weight || 0);
  }, 0);
});

const isValid = computed(() => Math.abs(totalWeighting.value - 100) < 0.01);

const addAssessment = () => {
  if (!versionData.value.assessments) {
    versionData.value.assessments = [];
  }
  const newAssessment: Assessment = {
    name: "",
    weighting: 0,
    category: 1, // Default to Examination
    description: "",
  };
  versionData.value.assessments.push(newAssessment);
};

const removeAssessment = (index: number) => {
  if (versionData.value.assessments) {
    versionData.value.assessments.splice(index, 1);
  }
};
</script>

<template>
  <Card>
    <CardHeader class="pb-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2">
          <ClipboardList class="h-5 w-5 text-primary" />
          <div>
            <CardTitle class="text-base font-semibold"
              >Assessment Methods</CardTitle
            >
            <CardDescription class="text-sm"
              >Define assessment components and their
              weightings</CardDescription
            >
          </div>
        </div>
        <Button variant="outline" size="sm" @click="addAssessment">
          <Plus class="h-4 w-4 mr-1" />
          Add Assessment
        </Button>
      </div>
    </CardHeader>
    <CardContent class="space-y-4">
      <div
        v-if="!versionData.assessments || versionData.assessments.length === 0"
        class="text-center py-8 text-muted-foreground border-2 border-dashed rounded-lg"
      >
        <ClipboardList class="h-8 w-8 mx-auto mb-2 opacity-50" />
        <p class="text-sm">No assessment methods added yet</p>
        <Button variant="link" size="sm" @click="addAssessment">
          Add your first assessment method
        </Button>
      </div>
      <template v-else>
        <div class="space-y-3">
          <div
            v-for="(assessment, index) in versionData.assessments"
            :key="index"
            class="relative p-4 border rounded-lg bg-muted/30 hover:bg-muted/50 transition-colors"
          >
            <div class="flex items-start gap-3">
              <div
                class="flex items-center justify-center w-8 h-8 rounded-full bg-primary/10 text-primary text-sm font-medium shrink-0"
              >
                {{ index + 1 }}
              </div>

              <div class="flex-1 space-y-3">
                <div class="grid gap-3 md:grid-cols-[1fr,100px,150px]">
                  <div class="space-y-1.5">
                    <Label class="text-xs">Method Name</Label>
                    <Input
                      v-model="assessment.name"
                      placeholder="e.g. Final Examination"
                    />
                  </div>
                  <div class="space-y-1.5">
                    <Label class="text-xs">Weight (%)</Label>
                    <Input
                      v-model.number="assessment.weighting"
                      type="number"
                      min="0"
                      max="100"
                      step="0.1"
                      placeholder="0"
                    />
                  </div>
                  <div class="space-y-1.5">
                    <Label class="text-xs">Category</Label>
                    <Select
                      :model-value="String(assessment.category ?? 1)"
                      @update:model-value="
                        (v) => (assessment.category = Number(v))
                      "
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select category" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem
                          v-for="(label, value) in AssessmentCategory"
                          :key="value"
                          :value="String(value)"
                        >
                          {{ label }}
                        </SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div class="space-y-1.5">
                  <Label class="text-xs">Description (Optional)</Label>
                  <Textarea
                    v-model="assessment.description"
                    :rows="2"
                    class="resize-none"
                    placeholder="Additional details about this assessment..."
                  />
                </div>
              </div>

              <Button
                variant="ghost"
                size="icon"
                class="h-8 w-8 text-muted-foreground hover:text-destructive shrink-0"
                @click="removeAssessment(index)"
              >
                <Trash2 class="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>

        <!-- Total Weighting Summary -->
        <div
          :class="[
            'flex items-center justify-between p-4 rounded-lg border-2',
            isValid
              ? 'border-emerald-500/30 bg-emerald-500/5'
              : 'border-amber-500/30 bg-amber-500/5',
          ]"
        >
          <div class="flex items-center gap-2">
            <CheckCircle v-if="isValid" class="h-5 w-5 text-emerald-500" />
            <AlertCircle v-else class="h-5 w-5 text-amber-500" />
            <span class="text-sm font-medium">Total Weighting</span>
          </div>
          <div class="flex items-center gap-3">
            <Badge
              :variant="isValid ? 'default' : 'outline'"
              :class="
                isValid
                  ? 'bg-emerald-500 hover:bg-emerald-500'
                  : 'border-amber-500 text-amber-600'
              "
            >
              {{ totalWeighting.toFixed(1) }}%
            </Badge>
            <span v-if="!isValid" class="text-xs text-amber-600"
              >Must equal 100%</span
            >
          </div>
        </div>
      </template>
    </CardContent>
  </Card>
</template>
