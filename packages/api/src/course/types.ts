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
