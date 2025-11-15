import {
  GradeSchema,
  StudentCourseStatusSchema,
  TermCodeSchema,
} from "../static/types.js";
import { AcademicYearSchema, CourseSchema } from "../course/types.js";
import { z } from "zod";
export const StudentCourseRequestSchema = z.object({
  courseID: z.string(),
  status: StudentCourseStatusSchema,
  grade: GradeSchema,
  term: TermCodeSchema,
  sectionID: z.string(),
  year: AcademicYearSchema,
});

export type StudentCourseRequest = z.infer<typeof StudentCourseRequestSchema>;

export const StudentCourse = z.object({
  id: z.string(),
  course: CourseSchema,
  studentCourseStatus: StudentCourseStatusSchema,
  grade: GradeSchema,
  term: TermCodeSchema,
  year: AcademicYearSchema,
});

export type StudentCourse = z.infer<typeof StudentCourse>;
