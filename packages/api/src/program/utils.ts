import { Course, CourseCode } from "course/types.js";
import {
  Category,
  CourseSelectionMode,
  ProgrammeCheckCategory,
  RuleNode,
} from "./types.js";

export function getAllGroupsFromCategory(data: RuleNode): string[] {
  const groups: string[] = [];

  function traverse(node: RuleNode) {
    if (node.type === "group" && node.groupID) {
      groups.push(node.groupID);
    }
    if (node.children) {
      for (const child of node.children) {
        traverse(child);
      }
    }
  }

  traverse(data);
  return groups;
}

export function checkGroupCompletion(
  groupID: string,
  selectionMode: CourseSelectionMode,
  min_credit: number, // only for min-credit mode
  completedCourses: Map<string, Course>, //courseID -> course
  groupMap: Map<string, { course: Course[]; tag: CourseCode[] }> //groupID -> courses or accept tags in that group
): boolean {
  const groupData = groupMap.get(groupID);

  if (!groupData) {
    return false;
  }

  const { course: groupCourses, tag: groupTags } = groupData;

  if (selectionMode === "all-of") {
    // all of shouldn't care about tags I think
    const checkArray: boolean[] = [];

    for (const course of groupCourses) {
      const hasCourse = completedCourses.has(course.id!);
      if (hasCourse) {
        completedCourses.delete(course.id!);
      }
      checkArray.push(hasCourse);
    }

    return checkArray.every((check) => check === true);
  } else if (selectionMode === "one-of") {
    for (const course of completedCourses.values()) {
      // check if course is in group courses
      const inGroupCourses = groupCourses.some(
        (groupCourse) => groupCourse.id === course.id
      );
      if (inGroupCourses) {
        completedCourses.delete(course.id!);
        return true;
      }

      // check if course code tag is in group tags
      if (course.code && groupTags.some((tag) => tag.id === course.code?.id)) {
        completedCourses.delete(course.id!);
        return true;
      }
    }

    return false;
  } else if (selectionMode === "min-credit") {
    let totalCredits = 0;

    for (const course of completedCourses.values()) {
      // check if course is in group courses
      const inGroupCourses = groupCourses.some(
        (groupCourse) => groupCourse.id === course.id
      );
      if (inGroupCourses) {
        completedCourses.delete(course.id!);

        totalCredits += course.credit || 0;
        continue;
      }

      // check if course code tag is in group tags
      if (course.code && groupTags.some((tag) => tag.id === course.code?.id)) {
        completedCourses.delete(course.id!);
        totalCredits += course.credit || 0;
      }
    }

    return totalCredits >= min_credit;
  }

  return false;
}

// WHY I AM SO SMART
export function checkCategoryCompletion(
  category: Category,
  completedCourses: Map<string, Course>, //courseID -> course
  groupMap: Map<string, { course: Course[]; tag: CourseCode[] }> //groupID -> courses or accept tags in that group
): ProgrammeCheckCategory {
  function traverseRuleNode(node: RuleNode): boolean {
    const checks: boolean[] = [];

    if (node.children) {
      for (const child of node.children) {
        if (child.type === "rule") {
          const nodeCheck = traverseRuleNode(child);
          console.log("after traversing child node:", child, "got:", nodeCheck);
          checks.push(nodeCheck);
        } else {
          // group node
          const groupCheck = checkGroupCompletion(
            child.groupID!,
            child.courseSelectionMode || "one-of",
            category.min_credit,
            completedCourses,
            groupMap
          );
          console.log("Group node:", child, "has result:", groupCheck);
          checks.push(groupCheck);
        }
      }
    }

    if (checks.length > 0) {
      if (node.operator === "and") {
        const allTrue = checks.every((check) => check === true);
        console.log("Node:", node, "with operator AND has result:", allTrue);
        return allTrue;
      } else if (node.operator === "any") {
        const anyTrue = checks.some((check) => check === true);
        console.log("Node:", node, "with operator ANY has result:", anyTrue);
        return anyTrue;
      }
    }

    return false;
  }

  const completed = traverseRuleNode(category.ruleTree);

  return {
    ...category,
    completed,
  };
}
