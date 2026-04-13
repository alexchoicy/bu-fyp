<script setup lang="ts">
import { ArrowUp, LoaderCircle, Plus } from "lucide-vue-next";
import type { components } from "~/API/schema";
import { base64UrlEncodeString } from "~/lib/base64utils";

type Message = components["schemas"]["MessageResponseDto"]
type MessageRelatedData = components["schemas"]["MessageRelatedDataResponseDto"]
type TimetableGenerationRequest = components["schemas"]["TimetableGenerationRequestDto"]
type EncodedGenerationState = {
  generationRequest?: TimetableGenerationRequest;
  currentStep?: "form" | "results";
}

type ChatMessage = Message & {
  nextSuggestions?: string[];
  timetable_tool?: TimetableGenerationRequest[];
}

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
const messages = ref<ChatMessage[]>([]);
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

const clearNextSuggestions = () => {
  messages.value = messages.value.map((message) => {
    if (message.role !== Roles.Assistant || !message.nextSuggestions?.length) {
      return message;
    }

    return {
      ...message,
      nextSuggestions: [],
    };
  });
}

const updateMessage = (messageId: string, updater: (message: ChatMessage) => ChatMessage) => {
  const index = messages.value.findIndex((message) => message.id === messageId);

  if (index === -1) {
    return;
  }

  const currentMessage = messages.value[index];

  if (!currentMessage) {
    return;
  }

  messages.value[index] = updater(currentMessage);
}

const fetchRelatedData = async (messageId: string) => {
  try {
    const related = await useNuxtApp().$api<MessageRelatedData>(
      `chat/${thisChat.value}/result/${messageId}/related`,
      {
        method: "GET",
      }
    );

    updateMessage(messageId, (message) => ({
      ...message,
      nextSuggestions: related.nextSuggestions ?? [],
      timetable_tool: related.timetable_tool ?? [],
    }));
  } catch (error) {
    console.error("Error fetching related message data:", error);
  }
}

const formatToolPayload = (payload: TimetableGenerationRequest) => {
  return JSON.stringify(payload, null, 2);
}

const openTimetableGenerator = (payload: TimetableGenerationRequest) => {
  const encodedState = base64UrlEncodeString(JSON.stringify({
    generationRequest: payload,
    currentStep: "results",
  } satisfies EncodedGenerationState));

  navigateTo({
    path: "/timetable/generation",
    query: {
      tg: encodedState,
    },
  }, {
    open: {
      target: "_blank",
    },
  });
}

const canSend = computed(() => {
  const value = input.value?.trim() ?? "";
  return value.length > 0 && lastMessageId.value === "" && !isSending.value;
});

const canStartNewRoom = computed(() => {
  return !isSending.value && lastMessageId.value === "";
});

const startNewRoom = async () => {
  if (!canStartNewRoom.value) {
    return;
  }

  isNewChat.value = true;
  messages.value = [];
  thisChat.value = "";
  lastMessageId.value = "";
  input.value = "";

  await nextTick();
  messageViewport.value?.scrollTo({
    top: 0,
    behavior: "auto",
  });
}

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
      const completedMessageId = lastMessageId.value;

      updateMessage(completedMessageId, (message) => ({
        ...message,
        ...result,
      }));

      await fetchRelatedData(completedMessageId);
      lastMessageId.value = "";
      scrollToBottom();
    } else if (result.status === MessageStatus.Failed) {
      updateMessage(lastMessageId.value, (message) => ({
        ...message,
        ...result,
        content:
          result.content && result.content.trim().length > 0
            ? result.content
            : "Sorry, I couldn't generate a response. Please try again.",
      }));

      lastMessageId.value = "";
    } else {
      // Still processing, will check again later
      console.log("Message still processing, will check again later.");
    }
  } catch (error) {
    console.error("Error fetching message result:", error);
  }
};

const sendMessage = async (rawMessage: string) => {
  const message = rawMessage.trim() ?? "";

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
  clearNextSuggestions();

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
      nextSuggestions: [],
      timetable_tool: [],
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
    updateMessage(lastMessageId.value, (message) => ({
      ...message,
      content:
        "Sorry, I couldn't send your message. Please try again.",
      status: MessageStatus.Failed,
    }));

    if (lastMessageId.value === "") {
      messages.value.push({
        role: Roles.Assistant,
        content:
          "Sorry, I couldn't send your message. Please try again.",
        status: MessageStatus.Failed,
        id: crypto.randomUUID(),
        nextSuggestions: [],
        timetable_tool: [],
      });
    }

    scrollToBottom("smooth");
  } finally {
    isSending.value = false;
  }

};

const onSend = async () => {
  await sendMessage(input.value ?? "");
}
</script>

<template>
  <div class="flex flex-col h-full">
    <div class="flex justify-end px-4 py-3">
      <Button variant="outline" size="sm" :disabled="!canStartNewRoom" @click="startNewRoom">
        <Plus class="size-4" />
        <span>New room</span>
      </Button>
    </div>
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
              <Accordion v-if="message.timetable_tool?.length" type="single" collapsible class="mb-3 w-full space-y-2">
                <AccordionItem v-for="(toolPayload, index) in message.timetable_tool" :key="`${message.id}-tool-${index}`"
                  :value="`${message.id}-tool-${index}`">
                  <AccordionTrigger>
                    Timetable Generation v{{ index + 1 }}
                  </AccordionTrigger>
                  <AccordionContent>
                    <div class="space-y-3 rounded-md border bg-muted/30 p-3">
                      <Button variant="outline" size="sm" @click="openTimetableGenerator(toolPayload)">
                        Open timetable generator
                      </Button>
                      <pre class="overflow-x-auto rounded-md bg-background p-3 text-xs">{{ formatToolPayload(toolPayload) }}</pre>
                    </div>
                  </AccordionContent>
                </AccordionItem>
              </Accordion>
              <UiChatMessageContent :content="message.content" />
              <div v-if="message.nextSuggestions?.length" class="mt-4 flex flex-wrap gap-2">
                <Button v-for="suggestion in message.nextSuggestions" :key="`${message.id}-${suggestion}`" variant="outline"
                  size="sm" :disabled="isSending || lastMessageId !== ''" @click="sendMessage(suggestion)">
                  {{ suggestion }}
                </Button>
              </div>
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
