<script setup lang="ts">
import { ArrowUp } from "lucide-vue-next";
import type { components } from "~/API/schema";

type Message = components["schemas"]["MessageResponseDto"]

enum Roles {
  User,
  Assistant,
}

enum MessageStatus {
  Pending,
  Complete,
  Failed,
}

const input = defineModel<string>();

//TODO right now i don't do any history things first, each time open a new chat
const isNewChat = ref(true);
const messages = ref<Message[]>([]);
const thisChat = ref<string>("");

// This is for fetching message results later
const lastMessageId = ref<string>("");


const messagePollingInterval = 1000;

const messagePoller = async () => {
  if (lastMessageId.value === "") return;

  try {
    const result = await useNuxtApp().$api<Message>(
      `chat/${thisChat.value}/result/${lastMessageId.value}`,
      {
        method: "GET",
      }
    );

    if (result.status === MessageStatus.Complete) {
      messages.value.push(result);
      lastMessageId.value = "";
    } else {
      // Still processing, will check again later
      console.log("Message still processing, will check again later.");
    }
  } catch (error) {
    console.error("Error fetching message result:", error);
  }
};

const onSend = async () => {
  if (lastMessageId.value !== "") {
    console.log("Fetching result for message ID:", lastMessageId.value);
    return;
  }

  messages.value.push({
    role: Roles.User,
    content: input.value!,
    id: crypto.randomUUID(),
  });

  if (isNewChat.value) {
    const chatRoomId = await useNuxtApp().$api<
      components["schemas"]["CreateRoomResponseDto"]
    >("chat", {
      method: "POST"
    })

    thisChat.value = chatRoomId.roomId!;
    isNewChat.value = false;
    console.log("Created new chat room with ID:", chatRoomId);
  }

  const newMessageId = await useNuxtApp().$api<
    components["schemas"]["SendMessageResponseDto"]
  >(`chat/${thisChat.value}`, {
    method: "POST",
    body: {
      message: input.value!,
    },
  });
  // Um... any better way to do this?
  // put this here so i don't need to find from the messages array
  input.value = "";

  lastMessageId.value = newMessageId.generatedId!;

  console.log("Sent message, received message ID:", newMessageId);

  setTimeout(function poll() {
    messagePoller().then(() => {
      if (lastMessageId.value !== "") {
        setTimeout(poll, messagePollingInterval);
      }
    });
  }, messagePollingInterval);

};
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex-1 overflow-y-auto p-4">
      <div v-for="message in messages" :key="message.id" class="">
        <div v-if="message.role === Roles.User" class="mb-2 flex justify-end">
          <div class="inline-block bg-secondary p-2 rounded-lg">
            {{ message.content }}
          </div>
        </div>
        <div v-if="message.role === Roles.Assistance" class="pb-20 justify-start flex">
          {{ message.content }}
        </div>
      </div>
    </div>
    <div class="sticky bottom-0 z-20 flex bg-background">
      <div class="m-2 flex w-full">
        <InputGroup>
          <InputGroupTextarea v-model="input" placeholder="Ask Academic questions" />
          <InputGroupAddon align="block-end" class="justify-between">
            <div />
            <div>
              <InputGroupButton variant="default" class="rounded-full" size="icon-sm"
                :disabled="!input || input.length === 0 || lastMessageId === ''" @click="onSend">
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
