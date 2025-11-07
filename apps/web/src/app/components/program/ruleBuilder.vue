<script setup lang="ts">
  import type { Group, RuleNode } from "@fyp/api/program/types";

  const props = defineProps({
    node: {
      type: Object as () => RuleNode,
      required: true,
    },
    depth: {
      type: Number,
      required: true,
    },
  });

  function addRule() {
    if (props.node.children) {
      props.node.children.push({
        type: "rule",
        operator: "and",
        children: [],
      });
    }
  }

  function addGroup() {
    if (props.node.children) {
      props.node.children.push({
        type: "group",
        courseSelectionMode: "all-of",
        children: [],
      });
    }
  }

  const { data: availableGroup } = useAPI<Group[]>("groups");
</script>

<template>
  <div v-if="node.type === 'rule'">
    <Card>
      <CardHeader>
        <Select v-model="node.operator">
          <SelectTrigger>
            <SelectValue placeholder="Select operator" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="and">AND</SelectItem>
            <SelectItem value="any">ANY OF</SelectItem>
          </SelectContent>
        </Select>
      </CardHeader>
      <CardContent>
        <div v-for="(child, index) in node.children" :key="index" :style="{ marginLeft: `${depth * 20}px` }">
          <ProgramRuleBuilder :node="child" :depth="depth + 1" />
        </div>
      </CardContent>
      <CardFooter>
        <Button
          v-if="
            (node.children && node.children[0] && node.children[0].type != 'group') ||
            !node.children ||
            node.children.length === 0
          "
          variant="outline"
          @click="addRule">
          Add Rule
        </Button>
        <Button
          v-if="
            (node.children && node.children[0] && node.children[0].type === 'group') ||
            !node.children ||
            node.children.length === 0
          "
          variant="outline"
          @click="addGroup">
          Add Group
        </Button>
      </CardFooter>
    </Card>
  </div>
  <div v-else-if="node.type === 'group'">
    <Card>
      <CardContent>
        <Select v-model="node.courseSelectionMode">
          <SelectTrigger>
            <SelectValue placeholder="Select course selection mode" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all-of">ALL OF</SelectItem>
            <SelectItem value="one-of">ONE OF</SelectItem>
          </SelectContent>
        </Select>

        <div class="w-full">
          <Select v-model="node.groupID">
            <SelectTrigger class="w-full">
              <SelectValue placeholder="Select group" class="w-full" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem v-for="group in availableGroup" :key="group.id" :value="group.id">
                {{ group.name }}
              </SelectItem>
            </SelectContent>
          </Select>
        </div>
      </CardContent>
    </Card>
  </div>
</template>
