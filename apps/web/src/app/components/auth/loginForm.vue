<script setup lang="ts">
  import type { NuxtError } from "#app";
  import { toTypedSchema } from "@vee-validate/zod";
  import { useForm } from "vee-validate";
  import { LoginRequestSchema } from "@fyp/api/auth/types";

  const formSchema = toTypedSchema(LoginRequestSchema);

  const form = useForm({
    validationSchema: formSchema,
  });

  const accountType = ref<"user" | "admin">("user");

  const errorMessage = ref<NuxtError | null>(null);

  const onFormSubmit = form.handleSubmit(async (values) => {
    try {
      await useNuxtApp().$backend(`auth/${accountType.value}/login`, {
        method: "POST",
        body: {
          username: values.username,
          password: values.password,
        },
      });
      await navigateTo("/");
    } catch (error: unknown) {
      errorMessage.value = error as NuxtError;
    }
  });
</script>

<template>
  <Card class="w-full max-w-sm">
    <form @submit="onFormSubmit">
      <CardHeader>
        <CardTitle class="text-2xl">Login</CardTitle>
        <CardDescription></CardDescription>
      </CardHeader>
      <CardContent class="grid gap-4 p-6">
        <Tabs v-model="accountType" class="w-full">
          <TabsList class="grid w-full grid-cols-2">
            <TabsTrigger value="user">User</TabsTrigger>
            <TabsTrigger value="admin">Admin</TabsTrigger>
          </TabsList>
        </Tabs>
        <FormField v-slot="{ componentField }" name="username">
          <FormItem>
            <div class="grid gap-2">
              <FormLabel>Username</FormLabel>
              <FormControl>
                <Input type="text" placeholder="Enter your username" v-bind="componentField" required />
              </FormControl>
            </div>
            <FormMessage class="mt-1" />
          </FormItem>
        </FormField>
        <FormField v-slot="{ componentField }" name="password">
          <FormItem>
            <div class="grid gap-2">
              <FormLabel>Password</FormLabel>
              <FormControl>
                <Input type="password" v-bind="componentField" required />
              </FormControl>
            </div>
            <FormMessage class="mt-1" />
          </FormItem>
        </FormField>
        <CardFooter v-if="errorMessage" class="">
          <p class="text-destructive text-sm mb-2">
            {{ errorMessage.statusMessage || "An unknown error occurred" }}
          </p>
        </CardFooter>
      </CardContent>
      <CardFooter class="flex flex-col gap-2 p-6 pt-0">
        <Button class="w-full">Login</Button>
      </CardFooter>
    </form>
  </Card>
</template>
