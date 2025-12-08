<script setup lang="ts">
import { GraduationCap, Plus, Trash2 } from "lucide-vue-next";
import type { components } from "~/API/schema";

const versionData = defineModel<
  components["schemas"]["CreateCourseVersionDto"]
>({
  required: true,
});

const addCILO = () => {
  if (!versionData.value.cilOs) {
    versionData.value.cilOs = [];
  }
  const newCILO = {
    code: `CILO${versionData.value.cilOs.length + 1}`,
    description: "",
  };
  versionData.value.cilOs.push(newCILO);
};
</script>

<template>
  <Card>
    <CardHeader class="pb-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2">
          <GraduationCap class="h-5 w-5 text-primary" />
          <div>
            <CardTitle class="text-base font-semibold"
              >Course Intended Learning Outcomes (CILOs)</CardTitle
            >
            <CardDescription class="text-sm"
              >Define measurable learning outcomes for students</CardDescription
            >
          </div>
        </div>
        <Button variant="outline" size="sm" @click="addCILO">
          <Plus class="h-4 w-4 mr-1" />
          Add CILO
        </Button>
      </div>
    </CardHeader>
    <CardContent class="space-y-4">
      <div
        v-if="!versionData.cilOs || versionData.cilOs.length === 0"
        class="text-center py-8 text-muted-foreground border-2 border-dashed rounded-lg"
      >
        <GraduationCap class="h-8 w-8 mx-auto mb-2 opacity-50" />
        <p class="text-sm">No CILOs added yet</p>
        <Button variant="link" size="sm" @click="addCILO">
          Add your first CILO
        </Button>
      </div>
      <template v-else>
        <div v-for="(cilo, index) in versionData.cilOs" :key="cilo.code">
          <div class="flex-1 space-y-1.5">
            <div class="flex justify-between">
              <Label class="text-xs font-medium"
                >CILO {{ index + 1 }} Description</Label
              >
              <Button
                v-if="versionData.cilOs.length === index + 1"
                variant="ghost"
                size="sm"
                class="text-red-500 hover:bg-red-500/10"
                @click="versionData.cilOs.splice(index, 1)"
              >
                <Trash2 class="h-4 w-4" />
              </Button>
            </div>
            <Textarea
              v-model="cilo.description"
              placeholder="Describe the learning outcome..."
              rows="{2}"
              class="resize-none"
            />
          </div>
        </div>
      </template>
    </CardContent>
  </Card>
</template>
