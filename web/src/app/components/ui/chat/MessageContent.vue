<script setup lang="ts">
import { renderChatMarkdown } from "@/lib/chatMarkdown"

const props = defineProps<{
    content?: string
}>()

const renderedContent = computed(() => renderChatMarkdown(props.content ?? ""))
</script>

<template>
    <div class="chat-markdown" v-html="renderedContent" />
</template>