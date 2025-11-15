<script setup lang="ts">
  import type { ProgrammeListItem } from "@fyp/api/program/types";
  import type { NuxtError } from "#app";
  import { toast } from "vue-sonner";

  const user = useAuthUser();

  const { data: programmes, pending: programmesPending } = await useAPI<ProgrammeListItem[]>("program");

  const {
    data: userProgrammes,
    pending: userProgrammesPending,
    refresh: refreshUserProgrammes,
  } = await useAPI<ProgrammeListItem[]>(() => `users/${user.value?.uid}/programmes`);

  const selectedProgramme = ref<string>("");
  const isSubmitting = ref(false);

  const isLoading = computed(() => programmesPending.value || userProgrammesPending.value);

  const currentProgrammes = computed(() => userProgrammes.value ?? []);

  const availableProgrammes = computed(() => {
    const assignedIds = new Set(currentProgrammes.value.map((p) => p.id));
    return (programmes.value ?? []).filter((programme) => {
      return !assignedIds.has(programme.id);
    });
  });

  function getErrorMessage(error: unknown) {
    const typedError = error as NuxtError | { statusMessage?: string };
    if (typedError?.statusMessage) {
      return typedError.statusMessage;
    }
    if (typedError && "message" in typedError && typedError.message) {
      return typedError.message as string;
    }
    return "Failed to add programme. Please try again.";
  }

  async function handleAddProgramme() {
    if (!selectedProgramme.value || !user.value) return;
    isSubmitting.value = true;
    try {
      await useNuxtApp().$backend<ProgrammeListItem>(`users/${user.value.uid}/programmes`, {
        method: "POST",
        body: { programmeId: selectedProgramme.value },
      });

      await refreshUserProgrammes();
      toast.success("Programme added to your profile.");
      selectedProgramme.value = "";
    } catch (error) {
      toast.error(getErrorMessage(error));
    } finally {
      isSubmitting.value = false;
    }
  }
</script>

<template>
  <div class="mx-auto flex max-w-3xl flex-col gap-6">
    <div class="space-y-2">
      <h1 class="text-3xl font-bold tracking-tight">Settings</h1>
      <p class="text-sm text-muted-foreground">Manage the academic programmes associated with your account.</p>
    </div>

    <Card>
      <CardHeader>
        <CardTitle>Current Programmes</CardTitle>
        <CardDescription>Programmes already linked to your account.</CardDescription>
      </CardHeader>
      <CardContent>
        <div v-if="isLoading" class="space-y-2">
          <Skeleton class="h-10 w-full" />
          <Skeleton class="h-10 w-3/4" />
        </div>
        <div v-else-if="currentProgrammes.length" class="flex flex-wrap gap-2">
          <Badge v-for="programme in currentProgrammes" :key="programme.id" variant="secondary">
            <span class="font-medium">{{ programme.name }}</span>
            <span class="ml-2 text-xs text-muted-foreground">v{{ programme.version }}</span>
          </Badge>
        </div>
        <p v-else class="text-sm text-muted-foreground">You haven't added any programmes yet.</p>
      </CardContent>
    </Card>

    <Card>
      <CardHeader>
        <CardTitle>Add a Programme</CardTitle>
        <CardDescription>Select a programme from the list to associate it with your profile.</CardDescription>
      </CardHeader>
      <CardContent class="space-y-4">
        <div class="space-y-2">
          <Label for="programme-select">Programme</Label>
          <Select id="programme-select" v-model="selectedProgramme" :disabled="availableProgrammes.length === 0">
            <SelectTrigger class="w-full md:w-80">
              <SelectValue placeholder="Select a programme" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem v-for="programme in availableProgrammes" :key="programme.id" :value="programme.id">
                {{ programme.name }} (v{{ programme.version }})
              </SelectItem>
            </SelectContent>
          </Select>
          <p v-if="availableProgrammes.length === 0" class="text-sm text-muted-foreground">
            You've already added all available programmes.
          </p>
        </div>
      </CardContent>
      <CardFooter>
        <Button class="w-full md:w-auto" :disabled="!selectedProgramme || isSubmitting" @click="handleAddProgramme">
          <span v-if="isSubmitting">Adding...</span>
          <span v-else>Add Programme</span>
        </Button>
      </CardFooter>
    </Card>
  </div>
</template>
