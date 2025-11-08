import { CourseSchema } from "../course/types.js";
import { z } from "zod";

export type CourseSelectionMode = "one-of" | "all-of" | "min-credit";

export const RuleNodeSchema: z.ZodType<{
  type: "group" | "rule";
  operator?: "and" | "any";
  groupID?: string;
  courseSelectionMode?: CourseSelectionMode;
  children?: RuleNode[];
}> = z.lazy(() =>
  z.discriminatedUnion("type", [
    z.object({
      type: z.literal("group"),
      groupID: z.string(),
      courseSelectionMode: z
        .enum(["one-of", "all-of", "min-credit"])
        .optional(),
    }),
    z.object({
      type: z.literal("rule"),
      operator: z.enum(["and", "any"]).optional(),
      children: z.array(RuleNodeSchema).optional(),
    }),
  ])
);

export type RuleNode = z.infer<typeof RuleNodeSchema>;

export const CategorySchema = z.object({
  id: z.string().optional(),
  name: z.string(),
  ruleTree: RuleNodeSchema,
  min_credit: z.number().min(0),
  priority: z.number().int(),
  notes: z.string().optional(),
});
export type Category = z.infer<typeof CategorySchema>;

export const ProgrammeSchema = z.object({
  name: z.string(),
  categories: z.array(CategorySchema),
});
export type Programme = z.infer<typeof ProgrammeSchema>;

export const GroupSchema = z.object({
  id: z.string(),
  name: z.string(),
});
export type Group = z.infer<typeof GroupSchema>;

export const ProgrammeCheckCategorySchema = CategorySchema.extend({
  completed: z.boolean(),
});

export type ProgrammeCheckCategory = z.infer<
  typeof ProgrammeCheckCategorySchema
>;

export const ProgrammeCheckSchema = z.object({
  programmeId: z.string(),
  completedCourses: z.array(CourseSchema),
  name: z.string(),
  categories: z.array(ProgrammeCheckCategorySchema),
});

export type ProgrammeCheck = z.infer<typeof ProgrammeCheckSchema>;
