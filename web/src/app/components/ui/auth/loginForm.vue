<script setup lang="ts">
import { useForm } from "@tanstack/vue-form";
import { z } from "zod";
const formSchema = z.object({
  username: z.string().min(1, "Username is required"),
  password: z.string().min(1, "Password is required"),
});
const { login, loading, isAuthenticated } = useAuth();
const wrong = ref(false);
watch(
  () => isAuthenticated.value,
  (authed) => {
    if (authed) navigateTo("/");
  },
  { immediate: true }
);
const form = useForm({
  defaultValues: {
    username: "",
    password: "",
  },
  validators: {
    onSubmit: formSchema,
  },
  onSubmit: async ({ value }) => {
    const result = await login(value);
    if (!result) {
      wrong.value = true;
    } else {
      wrong.value = false;
    }
  },
});

function isInvalid(field: any) {
  return field.state.meta.isTouched && !field.state.meta.isValid;
}
</script>

<template>
  <div class="flex flex-col gap-6">
    <Card>
      <CardHeader>
        <CardTitle>Login to your account</CardTitle>
        <CardDescription>
          Enter your username below to login to your account
        </CardDescription>
      </CardHeader>
      <CardContent>
        <form @submit.prevent="form.handleSubmit">
          <FieldGroup>
            <form.Field name="username">
              <template #default="{ field }">
                <Field>
                  <FieldLabel for="username"> Username </FieldLabel>
                  <Input
                    :id="field.name"
                    :name="field.name"
                    :model-value="field.state.value"
                    :aria-invalid="isInvalid(field)"
                    autocomplete="off"
                    @blur="field.handleBlur"
                    @input="field.handleChange($event.target.value)"
                  />
                </Field>
              </template>
            </form.Field>
            <form.Field name="password">
              <template #default="{ field }">
                <Field>
                  <FieldLabel for="password"> Password </FieldLabel>
                  <Input
                    :id="field.name"
                    :name="field.name"
                    type="password"
                    :model-value="field.state.value"
                    :aria-invalid="isInvalid(field)"
                    autocomplete="off"
                    @blur="field.handleBlur"
                    @input="field.handleChange($event.target.value)"
                  />
                </Field>
              </template>
            </form.Field>
            <Field>
              <Button type="submit" :disabled="loading">{{
                loading ? "Logging in..." : "Login"
              }}</Button>
            </Field>
            <Field v-if="wrong">
              <span class="text-red-500 text-sm"
                >Wrong username or password.</span
              >
            </Field>
          </FieldGroup>
        </form>
      </CardContent>
    </Card>
  </div>
</template>
