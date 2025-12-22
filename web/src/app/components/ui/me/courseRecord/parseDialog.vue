<script setup lang="ts">
import type { components } from '~/API/schema';
import { AvailableTerm } from '~/types/static/facts';
import type { Grade, StudentCourseStatuses } from '~/types/static/grade';

const props = defineProps<{
    courses: components["schemas"]["SimpleCourseDto"][];
}>();

const records = ref<components["schemas"]["CreateStudentCourseDto"][]>([]);

const mainItems = defineModel<components["schemas"]["CreateStudentCourseDto"][]>("records");

const open = defineModel<boolean>('open', { default: false });
const input = ref<string>("");

export interface TermGroup {
    term: AvailableTerm;
    year: number;
}

const TermRegex = /(\d{4})-\d{4} (Semester \d)/;
const CourseCodeRegex = /\b([A-Za-z]{4})\s?(\d{4})\b/;


function parseCourseRecords() {
    const parser = new DOMParser();
    const doc = parser.parseFromString(input.value, "text/html");
    const table = doc.querySelector("table.section.course");
    if (!table) {
        throw new Error('No <table class="section course"> found in HTML.');
    }
    const rows = table.querySelectorAll("tr");
    const currentParseTermGroup = ref<TermGroup | null>(null);

    for (const row of rows) {
        if (row.classList.contains("termH")) {
            const cell = row.querySelector("th");
            if (cell === null || !cell.textContent) {
                continue
            }
            const match = cell.textContent?.match(TermRegex);
            if (match) {
                let switchTerm: AvailableTerm;
                switch (match[2]) {
                    case "Semester 1":
                        switchTerm = AvailableTerm.Semester1
                        break;
                    case "Semester 2":
                        switchTerm = AvailableTerm.Semester2;
                        break;
                    default:
                        switchTerm = AvailableTerm.SummerTerm;
                }

                currentParseTermGroup.value = {
                    year: parseInt(match[1]!),
                    term: switchTerm,
                };
            }
        } else if (row.classList.contains("C")) {
            const scCell = row.querySelector("td.sc");
            const gradeCell = row.querySelector("td.grade");
            if (scCell === null || gradeCell === null || !scCell.textContent || !gradeCell.textContent) {
                continue;
            }

            const courseCodeMatch = scCell.textContent.match(CourseCodeRegex);
            if (!courseCodeMatch || !currentParseTermGroup.value) {
                continue;
            }
            let status: StudentCourseStatuses;
            let grade: Grade | null = null;

            //there is better way to do this but later
            switch (gradeCell.textContent.trim()) {
                case "A":
                    grade = 'A';
                    status = 'Completed';
                    break;
                case "A-":
                    grade = 'AMinus';
                    status = 'Completed';
                    break;
                case "B+":
                    grade = 'BPlus';
                    status = 'Completed';
                    break;
                case "B":
                    grade = 'B';
                    status = 'Completed';
                    break;
                case "B-":
                    grade = 'BMinus';
                    status = 'Completed';
                    break;
                case "C+":
                    grade = 'CPlus';
                    status = 'Completed';
                    break;
                case "C":
                    grade = 'C';
                    status = 'Completed';
                    break;
                case "D":
                    grade = 'D';
                    status = 'Completed';
                    break;
                case "F":
                    grade = 'F';
                    status = 'Completed';
                    break;
                case "S":
                    grade = "S"
                    status = 'Completed';
                    break;
                case "U":
                    grade = "U"
                    status = 'Failed';
                    break;
                case "**":
                    status = "Exemption";
                    break;
                case "W":
                    status = "Withdrawn";
                    break;
                default:
                    status = "Enrolled";
            }

            const course = props.courses.find(c => c.codeTag === courseCodeMatch[1] && c.courseNumber === courseCodeMatch[2]);

            if (course) {
                records.value.push({
                    courseId: course.id,
                    termId: currentParseTermGroup.value.term,
                    grade: gradeCell.textContent.trim() === "**" ? null : grade,
                    academicYear: currentParseTermGroup.value.year,
                    status: status,
                });
            }
        }
    }
    mainItems.value = records.value;
    open.value = false;
}



</script>

<template>
    <Dialog v-model:open="open">
        <DialogContent>
            <DialogHeader>
                <DialogTitle>Parse Course Records</DialogTitle>
            </DialogHeader>
            <div class="space-y-4 py-4">
                <div class="space-y-2">
                    <Label>Course Records</Label>
                    <Textarea v-model="input" class="h-[200px] text-sm overflow-auto field-sizing-fixed!" wrap="off" />
                </div>
            </div>
            <DialogFooter>
                <Button @click="parseCourseRecords()">Parse</Button>
            </DialogFooter>
        </DialogContent>
    </Dialog>
</template>
