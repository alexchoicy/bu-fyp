<script setup lang="ts">
  import type { Programme } from "@fyp/api/program/types";

  definePageMeta({
    allowedRoles: ["admin"],
  });
  // I DUNNO WHAT I WAS DOING HERE BUT IT WORKS COOL

  const data: Ref<Programme> = ref({
    name: "new Programme",
    categories: [],
  });

  function createCategory() {
    data.value.categories.push({
      name: "New Category",
      min_credit: 0,
      priority: 0,
      ruleTree: {
        type: "rule",
        operator: "and",
        children: [],
      },
    });
  }

  function createFreeElectiveCategory() {
    data.value.categories.push({
      name: "Free Electives",
      min_credit: 0,
      priority: data.value.categories.length,
      ruleTree: {
        type: "group",
        children: [],
        groupID: "0",
        courseSelectionMode: "min-credit",
      },
    });
  }

  async function createProgram() {
    const sendSort = data.value.categories.sort((a, b) => a.priority - b.priority);

    await useNuxtApp().$backend("program", {
      method: "POST",
      body: {
        ...data.value,
        categories: sendSort,
      },
    });

    await useRouter().push("/");
  }
</script>

<template>
  <div>
    <Button @click="createProgram">Create</Button>
    <Button @click="createCategory">Add</Button>
    <Button @click="createFreeElectiveCategory">Add Free elective Category</Button>
    <Input v-model="data.name" placeholder="Programme Name" />
    <div v-for="(category, index) in data.categories" :key="index">
      <Card>
        <CardHeader>
          <Input v-model="category.name" placeholder="Category Name" />
        </CardHeader>
        <CardContent>
          <div>Rule Builder for {{ category.name }}</div>
          <ProgramRuleBuilder :category="category" :node="category.ruleTree" :depth="0" />
        </CardContent>
      </Card>
    </div>
  </div>
  <pre>{{ data }}</pre>
</template>
