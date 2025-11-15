import { CourseSectionWeekdays, TermCodeSchema } from "../static/types.js";
import { Group, GroupSchema } from "../program/types.js";
import { z } from "zod";

export const AcademicYearSchema = z
  .string()
  .regex(/^[0-9]{4}-[0-9]{4}$/u, "Year must be in YYYY-YYYY format")
  .refine((value) => {
    const [start, end] = value.split("-").map(Number);
    return Number.isFinite(start) && Number.isFinite(end) && end - start === 1;
  }, "Academic year must span two consecutive years");

export type AcademicYear = z.infer<typeof AcademicYearSchema>;

export const CourseCodeSchema = z.object({
  id: z.string(),
  tag: z.string(),
  name: z.string(),
});
export type CourseCode = z.infer<typeof CourseCodeSchema>;

export const CourseSchema: z.ZodType<{
  id?: string;
  name: string;
  credit: number;
  description?: string;
  code?: CourseCode;
  courseNumber: number;
  is_active: boolean;
  prerequisites: Course[];
  antiRequisites: Course[];
  groups: Group[];
  sections: CourseSection[];
}> = z.lazy(() =>
  z.object({
    id: z.string().optional(),
    name: z.string(),
    description: z.string().optional(),
    credit: z.number(),
    code: CourseCodeSchema.optional(),
    courseNumber: z.number(),
    is_active: z.boolean(),
    prerequisites: z.array(CourseSchema),
    antiRequisites: z.array(CourseSchema),
    groups: z.array(GroupSchema),
    sections: z.array(CourseSectionSchema),
  })
);

export type Course = z.infer<typeof CourseSchema>;

export type CourseSectionType = "LEC" | "TUT" | "LAB";

export type CourseSectionWeekday =
  | "MON"
  | "TUE"
  | "WED"
  | "THU"
  | "FRI"
  | "SAT"
  | "SUN";

export const CourseMeetingSchema = z.object({
  id: z.string().optional(),
  day: z.enum(["MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN"]),
  startTime: z.string(),
  endTime: z.string(),
  location: z.string(),
  sectionType: z.enum(["LEC", "TUT", "LAB"]),
});

export type CourseMeeting = z.infer<typeof CourseMeetingSchema>;

export const CourseSectionSchema = z.object({
  id: z.string().optional(),
  capacity: z.number(),
  meetings: z.array(CourseMeetingSchema),
  term: TermCodeSchema,
  year: AcademicYearSchema,
});

export type CourseSection = z.infer<typeof CourseSectionSchema>;

export const CreateCourseSectionSchema = z.object({
  term: TermCodeSchema,
  year: AcademicYearSchema,
  position: z.number().int().min(0),
  meetings: z
    .array(CourseMeetingSchema.omit({ id: true }))
    .min(1, "At least one meeting is required"),
});

export type CreateCourseSection = z.infer<typeof CreateCourseSectionSchema>;
