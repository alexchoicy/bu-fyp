<script setup lang="ts">
import { ArrowUp, LoaderCircle } from "lucide-vue-next";
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
const isSending = ref(false);
const messageViewport = ref<HTMLElement | null>(null)

// This is for fetching message results later
const lastMessageId = ref<string>("");

const scrollToBottom = async (behavior: ScrollBehavior = "smooth") => {
  await nextTick()

  if (!messageViewport.value) {
    return
  }

  messageViewport.value.scrollTo({
    top: messageViewport.value.scrollHeight,
    behavior,
  })
}

const messagePollingInterval = 1000;

const canSend = computed(() => {
  const value = input.value?.trim() ?? "";
  return value.length > 0 && lastMessageId.value === "" && !isSending.value;
});

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
      const index = messages.value.findIndex(
        (msg) => msg.id === lastMessageId.value
      );
      if (index !== -1) {
        messages.value[index] = result;
      }

      lastMessageId.value = "";
      scrollToBottom();
    } else if (result.status === MessageStatus.Failed) {
      const index = messages.value.findIndex(
        (msg) => msg.id === lastMessageId.value
      );
      if (index !== -1) {
        messages.value[index] = {
          ...result,
          content:
            result.content && result.content.trim().length > 0
              ? result.content
              : "Sorry, I couldn't generate a response. Please try again.",
        };
      }
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
  const message = input.value?.trim() ?? "";

  if (message.length === 0) {
    return;
  }

  if (lastMessageId.value !== "") {
    return;
  }

  if (isSending.value) {
    return;
  }

  isSending.value = true;

  messages.value.push({
    role: Roles.User,
    content: message,
    status: MessageStatus.Complete,
    id: crypto.randomUUID(),
  });

  try {
    if (isNewChat.value) {
      const chatRoomId = await useNuxtApp().$api<
        components["schemas"]["CreateRoomResponseDto"]
      >("chat", {
        method: "POST",
      });

      thisChat.value = chatRoomId.roomId!;
      isNewChat.value = false;
    }

    const newMessageId = await useNuxtApp().$api<
      components["schemas"]["SendMessageResponseDto"]
    >(`chat/${thisChat.value}`, {
      method: "POST",
      body: {
        message,
      },
    });

    input.value = "";
    lastMessageId.value = newMessageId.generatedId!;

    messages.value.push({
      role: Roles.Assistant,
      content: "",
      status: MessageStatus.Pending,
      id: lastMessageId.value,
    });


    setTimeout(function poll() {
      messagePoller().then(() => {
        if (lastMessageId.value !== "") {
          setTimeout(poll, messagePollingInterval);
        }
      });
    }, messagePollingInterval);
  } catch (error) {
    console.error("Failed to send message:", error);
    const index = messages.value.findIndex(
      (msg) => msg.id === lastMessageId.value
    );
    if (index !== -1) {
      messages.value[index] = {
        ...messages.value[index],
        content:
          "Sorry, I couldn't send your message. Please try again.",
        status: MessageStatus.Failed,
      };
    }
    scrollToBottom("smooth");
  } finally {
    isSending.value = false;
  }

};
</script>

<template>
  <div class="flex flex-col h-full">
    <div ref="messageViewport" class="flex-1 overflow-y-auto p-4">
      <div v-for="message in messages" :key="message.id" class="">
        <div v-if="message.role === Roles.User" class="mb-2 flex justify-end">
          <div class="inline-block bg-secondary p-2 rounded-lg">
            <UiChatMessageContent :content="message.content" />
          </div>
        </div>
        <div v-if="message.role === Roles.Assistant" class="pb-20 justify-start flex">
          <template v-if="message.status === MessageStatus.Pending">
            <div class="space-y-3 py-1">
              <div class="flex items-center gap-2 text-sm font-medium text-muted-foreground">
                <LoaderCircle class="size-4 animate-spin" />
                Thinking through your request...
              </div>
              <div class="space-y-2">
                <Skeleton class="h-4 w-5/6" />
                <Skeleton class="h-4 w-4/6" />
                <Skeleton class="h-4 w-3/6" />
              </div>
            </div>
          </template>
          <template v-else>
            <div class="inline-block p-2 rounded-lg">
              <UiChatMessageContent :content="message.content" />
            </div>
          </template>
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
              <InputGroupButton variant="default" class="rounded-full" size="icon-sm" :disabled="!canSend"
                @click="onSend">
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
