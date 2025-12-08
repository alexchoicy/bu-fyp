<script setup lang="ts">
import { BookOpen, Building2, Hash } from "lucide-vue-next";
import type { paths } from "~/API/schema";

interface Props {
  courseInfo: paths["/api/courses/{id}"]["get"]["responses"]["200"]["content"]["application/json"];
}

defineProps<Props>();
</script>

<template>
  <Card class="border-border/50 bg-muted/30">
    <CardContent>
      <div class="flex items-center gap-2 mb-4">
        <Badge variant="outline" class="text-xs font-medium">
          Course Information
        </Badge>
        <span class="text-xs text-muted-foreground">Read-only</span>
      </div>
      <div class="grid gap-4 md:grid-cols-3">
        <div class="flex items-start gap-3">
          <div class="rounded-md bg-primary/10 p-2">
            <BookOpen class="h-4 w-4 text-primary" />
          </div>
          <div>
            <p class="text-xs text-muted-foreground font-medium">Course Name</p>
            <p class="text-sm font-semibold">{{ courseInfo.name }}</p>
          </div>
        </div>
        <div class="flex items-start gap-3">
          <div class="rounded-md bg-primary/10 p-2">
            <Hash class="h-4 w-4 text-primary" />
          </div>
          <div>
            <p class="text-xs text-muted-foreground font-medium">Course Code</p>
            <p class="text-sm font-semibold font-mono">
              {{ courseInfo.codeTag }} {{ courseInfo.courseNumber }}
            </p>
          </div>
        </div>
        <div class="flex items-start gap-3">
          <div class="rounded-md bg-primary/10 p-2">
            <Building2 class="h-4 w-4 text-primary" />
          </div>
          <div>
            <p class="text-xs text-muted-foreground font-medium">Departments</p>
            <p class="text-sm font-semibold">
              {{ courseInfo.departments?.join(", ") || "N/A" }}
            </p>
          </div>
        </div>
      </div>
    </CardContent>
  </Card>
</template>
