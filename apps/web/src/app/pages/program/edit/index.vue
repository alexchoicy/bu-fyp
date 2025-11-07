<script setup lang="ts">
  import type { Programme } from "@fyp/api/program/types";
  // I DUNNO WHAT I WAS DOING HERE BUT IT WORKS COOL

  const data: Ref<Programme> = ref({
    name: "new Programme",
    categories: [],
  });

  function createCategory() {
    data.value.categories.push({
      name: "New Category",
      ruleTree: {
        type: "rule",
        operator: "and",
        children: [],
      },
    });
  }

  async function createProgram() {
    await useNuxtApp().$backend("program", {
      method: "POST",
      body: data.value,
    });
  }
</script>

<template>
  <div>
    <Button @click="createProgram">Create</Button>
    <Button @click="createCategory">Add</Button>
    <Input v-model="data.name" placeholder="Programme Name" />
    <div v-for="(category, index) in data.categories" :key="index">
      <Card>
        <CardHeader>
          <Input v-model="category.name" placeholder="Category Name" />
        </CardHeader>
        <CardContent>
          <div>Rule Builder for {{ category.name }}</div>
          <ProgramRuleBuilder :node="category.ruleTree" :depth="0" />
        </CardContent>
      </Card>
    </div>
  </div>
  <pre>{{ data }}</pre>
</template>
