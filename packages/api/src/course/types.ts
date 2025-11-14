import { TermCodeSchema } from "../static/types.js";
import { Group, GroupSchema } from "../program/types.js";
import { z } from "zod";

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
  day: z.string(),
  startTime: z.string(),
  endTime: z.string(),
  location: z.string(),
  sectionType: z.enum(["LEC", "TUT", "LAB"]),
});

export type CourseMeeting = z.infer<typeof CourseMeetingSchema>;

export const CourseSectionSchema = z.object({
  id: z.string().optional(),
  name: z.string(),
  capacity: z.number(),
  meetings: z.array(CourseMeetingSchema),
  term: TermCodeSchema,
  year: z.string(),
});

export type CourseSection = z.infer<typeof CourseSectionSchema>;
