<script
  setup
  lang="ts"
  generic="T extends { id: string | number; name: string }"
>
import { ChevronsUpDown, Search, X, Check } from "lucide-vue-next";
import {
  Combobox,
  ComboboxAnchor,
  ComboboxTrigger,
  ComboboxList,
  ComboboxInput,
  ComboboxEmpty,
  ComboboxGroup,
  ComboboxItem,
  ComboboxItemIndicator,
} from "~/components/shadcn/combobox";
import { Button } from "~/components/shadcn/button";
import { Badge } from "~/components/shadcn/badge";
import { Label } from "~/components/shadcn/label";

interface Props {
  label: string;
  placeholder?: string;
  emptyMessage?: string;
  options: T[];
  required?: boolean;
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: "Select items...",
  emptyMessage: "No items found.",
  required: false,
});

const modelValue = defineModel<T[]>({
  required: true,
});

const removeItem = (item: T) => {
  modelValue.value = modelValue.value.filter(
    (selected) => selected.id !== item.id
  );
};
</script>

<template>
  <div class="space-y-2">
    <Label
      >{{ label }}
      <span v-if="required" class="text-destructive">*</span></Label
    >
    <Combobox v-model="modelValue" multiple by="id">
      <ComboboxAnchor as-child>
        <ComboboxTrigger as-child>
          <Button variant="outline" class="justify-between w-full">
            <span class="text-wrap">
              {{
                modelValue.length > 0
                  ? `${modelValue.length} item(s) selected`
                  : props.placeholder
              }}
            </span>
            <ChevronsUpDown class="ml-2 h-4 w-4 shrink-0 opacity-50" />
          </Button>
        </ComboboxTrigger>
      </ComboboxAnchor>
      <ComboboxList class="w-full">
        <div class="relative w-full max-w-sm items-center">
          <ComboboxInput
            class="focus-visible:ring-0 border-0 border-b rounded-none h-10"
            :placeholder="placeholder"
          />
          <span
            class="absolute start-0 inset-y-0 flex items-center justify-center px-3"
          >
            <Search class="size-4 text-muted-foreground" />
          </span>
        </div>
        <ComboboxEmpty>{{ emptyMessage }}</ComboboxEmpty>
        <ComboboxGroup>
          <ComboboxItem v-for="item in options" :key="item.id" :value="item">
            {{ item.name }}
            <ComboboxItemIndicator>
              <Check class="size-4" />
            </ComboboxItemIndicator>
          </ComboboxItem>
        </ComboboxGroup>
      </ComboboxList>
    </Combobox>
    <div v-if="modelValue.length > 0" class="flex flex-wrap gap-1 mb-2">
      <Badge
        v-for="item in modelValue"
        :key="item.id"
        variant="secondary"
        class="text-xs"
      >
        {{ item.name }}
        <button
          type="button"
          class="ml-1 hover:text-destructive"
          @click="removeItem(item)"
        >
          <X class="h-3 w-3" />
        </button>
      </Badge>
    </div>
  </div>
</template>
