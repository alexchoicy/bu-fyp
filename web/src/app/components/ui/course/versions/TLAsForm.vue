<script setup lang="ts">
import { Users, Plus, Trash2 } from "lucide-vue-next";
import type { components } from "~/API/schema";

const versionData = defineModel<
  components["schemas"]["CreateCourseVersionDto"]
>({
  required: true,
});

const addTLA = () => {
  if (!versionData.value.tlAs) {
    versionData.value.tlAs = [];
  }
  const newTLA = {
    code: [],
    description: "",
  };
  versionData.value.tlAs.push(newTLA);
};

const removeTLA = (index: number) => {
  if (versionData.value.tlAs) {
    versionData.value.tlAs.splice(index, 1);
  }
};

const addCiloAlignment = (index: number, code: string) => {
  if (versionData.value.tlAs && versionData.value.tlAs[index]) {
    const tla = versionData.value.tlAs[index];
    if (!tla.code.includes(code)) {
      tla.code.push(code);
    }
  }
};

const removeCiloAlignment = (index: number, code: string) => {
  if (versionData.value.tlAs && versionData.value.tlAs[index]) {
    const tla = versionData.value.tlAs[index];
    tla.code = tla.code.filter((c) => c !== code);
  }
};
</script>

<template>
  <Card>
    <CardHeader class="pb-4">
      <div class="flex items-center justify-between">
        <div class="flex items-center gap-2">
          <Users class="h-5 w-5 text-primary" />
          <div>
            <CardTitle class="text-base font-semibold"
              >Teaching & Learning Activities (TLAs)</CardTitle
            >
            <CardDescription class="text-sm"
              >Define activities and their alignment with CILOs</CardDescription
            >
          </div>
        </div>
        <Button variant="outline" size="sm" @click="addTLA">
          <Plus class="h-4 w-4 mr-1" />
          Add TLA
        </Button>
      </div>
    </CardHeader>
    <CardContent class="space-y-4">
      <div
        v-if="!versionData.tlAs || versionData.tlAs.length === 0"
        class="text-center py-8 text-muted-foreground border-2 border-dashed rounded-lg"
      >
        <Users class="h-8 w-8 mx-auto mb-2 opacity-50" />
        <p class="text-sm">No TLAs added yet</p>
        <Button variant="link" size="sm" @click="addTLA">
          Add your first TLA
        </Button>
      </div>
      <template v-else>
        <div
          v-for="(tla, index) in versionData.tlAs"
          :key="index"
          class="relative flex gap-3 p-4 border rounded-lg bg-muted/30 hover:bg-muted/50 transition-colors"
        >
          <div
            class="flex items-center justify-center w-8 h-8 rounded-full bg-primary/10 text-primary text-sm font-medium shrink-0"
          >
            {{ index + 1 }}
          </div>

          <div class="flex-1 space-y-3">
            <div class="space-y-1.5">
              <Label class="text-xs">Activity Description</Label>
              <Textarea
                v-model="tla.description"
                placeholder="e.g. Lecture, Case Study, Student Presentation"
              />
            </div>

            <div class="space-y-1.5">
              <Label class="text-xs">CILO Alignment</Label>
              <div class="flex flex-wrap items-center gap-2">
                <div class="flex gap-1">
                  <Button
                    v-for="num in ['1', '2', '3', '4', '5', '6']"
                    :key="num"
                    variant="ghost"
                    size="sm"
                    :class="`h-7 w-7 p-0 text-xs ${
                      tla.code.includes(num)
                        ? 'bg-primary text-primary-foreground'
                        : 'hover:bg-primary/10'
                    }`"
                    @click="
                      tla.code.includes(num)
                        ? removeCiloAlignment(index, num)
                        : addCiloAlignment(index, num)
                    "
                  >
                    {{ num }}
                  </Button>
                </div>
              </div>
            </div>
          </div>

          <Button
            variant="ghost"
            size="icon"
            class="h-8 w-8 text-muted-foreground hover:text-destructive shrink-0"
            @click="removeTLA(index)"
          >
            <Trash2 class="h-4 w-4" />
          </Button>
        </div>
      </template>
    </CardContent>
  </Card>
</template>
