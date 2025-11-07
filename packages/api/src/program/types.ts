import { z } from "zod";

export const RuleNodeSchema: z.ZodType<{
  type: "group" | "rule";
  operator?: "and" | "any";
  groupID?: string;
  courseSelectionMode?: "one-of" | "all-of";
  children?: RuleNode[];
}> = z.lazy(() =>
  z.discriminatedUnion("type", [
    z.object({
      type: z.literal("group"),
      groupID: z.string(),
      courseSelectionMode: z.enum(["one-of", "all-of"]).optional(),
    }),
    z.object({
      type: z.literal("rule"),
      operator: z.enum(["and", "any"]).optional(),
      children: z.array(RuleNodeSchema).optional(),
    }),
  ])
);

// --- Inferred TypeScript type (automatically matches your definition) ---
export type RuleNode = z.infer<typeof RuleNodeSchema>;

// --- Then the rest follow naturally ---
export const CategorySchema = z.object({
  name: z.string(),
  ruleTree: RuleNodeSchema,
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
