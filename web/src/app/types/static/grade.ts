export type Grade =
    | "A"
    | "AMinus"
    | "BPlus"
    | "B"
    | "BMinus"
    | "CPlus"
    | "C"
    | "CMinus"
    | "D"
    | "E"
    | "F"
    | "DT"
    | "I"
    | "S"
    | "U"
    | "W"
    | "IP"
    | "NA";
export const GradeDisplayNames: Record<Grade, string> = {
    A: "A",
    AMinus: "A-",
    BPlus: "B+",
    B: "B",
    BMinus: "B-",
    CPlus: "C+",
    C: "C",
    CMinus: "C-",
    D: "D",
    E: "Conditional Pass",
    F: "F",
    DT: "Distinction",
    I: "Incomplete",
    S: "Satisfactory",
    U: "Unsatisfactory",
    W: "Withdrawn",
    IP: "In Progress",
    NA: "Planned / Not Assigned",
};

export const StudentCourseStatuses = ["Enrolled", "Completed", "Dropped", "Planned", "Withdrawn", "Failed", "Exemption"] as const;
export type StudentCourseStatuses = (typeof StudentCourseStatuses)[number];