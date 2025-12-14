<script setup lang="ts">

function openCourse(id: number | string) {
    navigateTo(`/admin/courses/${id}`);
}
</script>

<template>
    <UiCourseList :on-open="openCourse" />
</template>