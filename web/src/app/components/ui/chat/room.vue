<script setup lang="ts">
import { ArrowUp } from "lucide-vue-next";

enum Roles {
  Assistance,
  User,
}

const input = defineModel<string>();

const messages = [
  {
    role: Roles.User,
    message: "TEST",
    id: "1",
  },
  {
    role: Roles.Assistance,
    message: "TEST",
    id: "2",
  },
  {
    role: Roles.User,
    message: "TEST",
    id: "3",
  },
  {
    role: Roles.Assistance,
    message: "TEST",
    id: "4",
  },
  {
    role: Roles.Assistance,
    message: "TEST",
    id: "4",
  },
  {
    role: Roles.Assistance,
    message: "TEST",
    id: "4",
  },
];
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex-1 overflow-y-auto p-4">
      <div v-for="message in messages" :key="message.id" class="">
        <div v-if="message.role === Roles.User" class="mb-2 flex justify-end">
          <div class="inline-block bg-secondary p-2 rounded-lg">
            {{ message.message }}
          </div>
        </div>
        <div
          v-if="message.role === Roles.Assistance"
          class="pb-20 justify-start flex"
        >
          {{ message.message }}
        </div>
      </div>
    </div>
    <div class="sticky bottom-0 z-20 flex bg-background">
      <div class="m-2 flex w-full">
        <InputGroup>
          <InputGroupTextarea
            v-model="input"
            placeholder="Ask Academic questions"
          />
          <InputGroupAddon align="block-end" class="justify-between">
            <div />
            <div>
              <InputGroupButton
                variant="default"
                class="rounded-full"
                size="icon-sm"
                :disabled="input && input?.length < 0"
              >
                <ArrowUp class="size-4" />
                <span class="sr-only">Send</span>
              </InputGroupButton>
            </div>
          </InputGroupAddon>
        </InputGroup>
      </div>
    </div>
  </div>
</template>
