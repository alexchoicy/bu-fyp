<script setup lang="ts">

const open = defineModel<boolean>('open', { default: false });

interface BlockTimeItem {
    id: string;
    startTime: number;
    endTime: number;
}

type BlockTimes = Record<number, BlockTimeItem[]>;

const props = defineProps<{
    blockTimes: BlockTimes;
}>();

const emit = defineEmits<{
    addBlockTime: [day: number, items: BlockTimeItem[]];
    deleteBlockTime: [day: number, itemId: string];
}>();

const selectedDay = ref<number>();
const selectedStartTime = ref<number>();
const selectedEndTime = ref<number>();

const handleAddBlockTime = () => {
    if (selectedDay.value !== undefined && selectedStartTime.value !== undefined && selectedEndTime.value !== undefined) {
        const day = selectedDay.value;
        const newItem: BlockTimeItem = {
            id: crypto.randomUUID(),
            startTime: selectedStartTime.value,
            endTime: selectedEndTime.value,
        };

        if (props.blockTimes[day]) {
            props.blockTimes[day].push(newItem);
        } else {
            emit('addBlockTime', day, [newItem]);
        }

        selectedDay.value = undefined;
        selectedStartTime.value = undefined;
        selectedEndTime.value = undefined;
        open.value = false;
    }
};

const handleDeleteBlockTime = (day: number, itemId: string) => {
    emit('deleteBlockTime', day, itemId);
};

const days = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

</script>

<template>
    <Dialog v-model:open="open">
        <DialogContent>
            <DialogHeader>
                <DialogTitle>TimeTable Block Time</DialogTitle>
                <DialogDescription>
                    Block specific times on your timetable to block out those slots when generating timetables.
                </DialogDescription>
            </DialogHeader>
            <div class="space-y-4 py-4">
                <Select v-model="selectedDay">
                    <SelectTrigger class="w-full">
                        <SelectValue placeholder="Select Day" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem v-for="(day, index) in days" :key="index" :value="index">
                            {{ day }}
                        </SelectItem>
                    </SelectContent>
                </Select>
                <Select v-model="selectedStartTime">
                    <SelectTrigger class="w-full">
                        <SelectValue placeholder="Select Start Time" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem v-for="hour in 15" :key="hour + 7" :value="hour + 8">
                            {{ (hour + 8).toString().padStart(2, '0') }}:30
                        </SelectItem>
                    </SelectContent>
                </Select>
                <Select v-model="selectedEndTime">
                    <SelectTrigger class="w-full">
                        <SelectValue placeholder="Select End Time" />
                    </SelectTrigger>
                    <SelectContent>
                        <SelectItem v-for="hour in 15" :key="hour + 8" :value="hour + 9">
                            {{ (hour + 9).toString().padStart(2, '0') }}:30
                        </SelectItem>
                    </SelectContent>
                </Select>
            </div>
            <div>
                <Button class="w-full" @click="handleAddBlockTime">Add Block Time</Button>
            </div>
            <div class="mt-4 space-y-3">
                <div v-for="(items, day) in blockTimes" :key="day">
                    <h3 class="text-sm font-medium mb-2">{{ days[Number(day)] }}</h3>
                    <div class="space-y-2">
                        <div v-for="item in items" :key="item.id"
                            class="flex items-center justify-between p-2 bg-muted rounded">
                            <span class="text-sm">{{ item.startTime }}:30 - {{ item.endTime }}:30</span>
                            <Button variant="ghost" size="sm"
                                @click="handleDeleteBlockTime(Number(day), item.id)">Delete</Button>
                        </div>
                    </div>
                </div>
            </div>
        </DialogContent>
    </Dialog>
</template>