<script setup lang="ts">
import { useDropZone } from "@vueuse/core";
import type { components, paths } from "~/API/schema";
import { FileText, Upload, Loader2 } from "lucide-vue-next";

const uploadedFile = defineModel<File | null>("uploadedFile");
const isUploading = defineModel<boolean>("isUploading");

const emit = defineEmits<{
  parsed: [data: components["schemas"]["PdfParseResponseDto"]];
}>();

function handleFileUpload(file: File) {
  if (
    file &&
    file.type === "application/pdf" &&
    file.size <= 10 * 1024 * 1024
  ) {
    uploadedFile.value = file;
  } else {
    alert("Please upload a valid PDF file (max 10MB).");
  }
}

const dropZoneRef = ref<HTMLDivElement>();

useDropZone(dropZoneRef, {
  onDrop: (files) => {
    if (files && files.length > 0) {
      const file = files[0];
      if (file) {
        handleFileUpload(file);
      }
    }
  },
});

async function handleParse() {
  if (!uploadedFile.value) return;
  isUploading.value = true;

  const formData = new FormData();
  formData.append("file", uploadedFile.value);
  const parsedData = await useNuxtApp().$api<
    paths["/api/courses/create/parsePDF"]["post"]["responses"]["200"]["content"]["application/json"]
  >("courses/create/parsePDF", {
    method: "POST",
    body: formData,
  });
  isUploading.value = false;

  console.log("Parsed Data:", parsedData);

  // Emit the parsed data to parent component
  if (parsedData) {
    emit("parsed", parsedData);
  }
}
</script>

<template>
  <Card>
    <CardHeader>
      <CardTitle>Upload Course Outline</CardTitle>
      <CardDescription>
        Upload a PDF course outline to automatically extract course information
      </CardDescription>
    </CardHeader>
    <CardContent class="space-y-4">
      <div
        ref="dropZoneRef"
        class="border-2 border-dashed border-border rounded-lg p-8 text-center hover:border-primary/50 transition-colors"
      >
        <input
          id="file-upload"
          type="file"
          accept=".pdf"
          class="hidden"
          @change="
              (e: Event) => {
                const files = (e.target as HTMLInputElement).files;
                if (files && files[0]) handleFileUpload(files[0]);
              }
            "
        />
        <label for="file-upload" class="cursor-pointer">
          <Upload class="h-12 w-12 mx-auto text-muted-foreground mb-4" />
          <p class="text-foreground font-medium">
            {{
              uploadedFile
                ? uploadedFile.name
                : "Click to upload or drag and drop"
            }}
          </p>
          <p class="text-sm text-muted-foreground mt-1">
            PDF files only (max 10MB)
          </p>
        </label>
      </div>

      <div
        v-if="uploadedFile"
        class="flex items-center justify-between bg-muted/50 rounded-lg p-4"
      >
        <div class="flex items-center gap-3">
          <FileText class="h-8 w-8 text-primary" />
          <div>
            <p class="font-medium text-foreground">
              {{ uploadedFile.name }}
            </p>
            <p class="text-sm text-muted-foreground">
              {{ (uploadedFile.size / 1024).toFixed(1) }} KB
            </p>
          </div>
        </div>
        <Button :disabled="isUploading" @click="handleParse">
          <Loader2 v-if="isUploading" class="h-4 w-4 mr-2 animate-spin" />
          <FileText v-else class="h-4 w-4 mr-2" />
          {{ isUploading ? "Parsing..." : "Parse Document" }}
        </Button>
      </div>
    </CardContent>
  </Card>
</template>
