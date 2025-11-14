import { CourseSectionType } from "../course/types.js";
import z from "zod";

export type StudentCourseStatus =
  | "enrolled"
  | "completed"
  | "dropped"
  | "planned"
  | "withdrawn"
  | "failed";

export type Grade =
  | "A+"
  | "A"
  | "A-"
  | "B+"
  | "B"
  | "B-"
  | "C+"
  | "C"
  | "C-"
  | "D+"
  | "D"
  | "D-"
  | "F"
  | "P"
  | "E"
  | "W"
  | "NA";

export type TermCode = "SEM1" | "SEM2" | "SUMMER" | "TRI1" | "TRI2" | "TRI3";

export const StudentCourseStatusSchema = z.enum([
  "enrolled",
  "completed",
  "dropped",
  "planned",
  "withdrawn",
  "failed",
]) as z.ZodType<StudentCourseStatus>;

export const GradeSchema = z.enum([
  "A+",
  "A",
  "A-",
  "B+",
  "B",
  "B-",
  "C+",
  "C",
  "C-",
  "D+",
  "D",
  "D-",
  "F",
  "P",
  "E",
  "W",
  "NA",
]) as z.ZodType<Grade>;

export const TermCodeSchema = z.enum([
  "SEM1",
  "SEM2",
  "SUMMER",
  "TRI1",
  "TRI2",
  "TRI3",
]) as z.ZodType<TermCode>;

export const TermMap = {
  SEM1: { label: "Semester 1" },
  SEM2: { label: "Semester 2" },
  SUMMER: { label: "Summer" },

  TRI1: { label: "Trimester 1" },
  TRI2: { label: "Trimester 2" },
  TRI3: { label: "Trimester 3" },
} satisfies Record<TermCode, { label: string }>;

export const GradeLabels: Record<Grade, string> = {
  "A+": "A+ (Excellent)",
  A: "A",
  "A-": "A-",
  "B+": "B+",
  B: "B",
  "B-": "B-",
  "C+": "C+",
  C: "C",
  "C-": "C-",
  "D+": "D+",
  D: "D",
  "D-": "D-",
  F: "F (Fail)",
  P: "P (Pass)",
  E: "E (Enrolled)",
  W: "W (Withdrawn)",
  NA: "NA (Planned / Not Assigned)",
};

// what are these lol, ya i change back to enum one day, i dunno why i heard someone say type is better
export const CourseSectionTypes: Record<CourseSectionType, string> = {
  LEC: "Lecture",
  TUT: "Tutorial",
  LAB: "Laboratory",
};

export const CourseSectionWeekdays: Record<string, string> = {
  MON: "Monday",
  TUE: "Tuesday",
  WED: "Wednesday",
  THU: "Thursday",
  FRI: "Friday",
  SAT: "Saturday",
  SUN: "Sunday",
};
