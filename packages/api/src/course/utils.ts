const academicYearPattern = /^[0-9]{4}-[0-9]{4}$/u;
const singleYearPattern = /^[0-9]{4}$/u;

export function formatAcademicYear(startYear: number): string {
  return `${startYear}-${startYear + 1}`;
}

export function normalizeAcademicYear(input: string): string {
  if (academicYearPattern.test(input)) {
    return input;
  }

  if (singleYearPattern.test(input)) {
    return formatAcademicYear(Number(input));
  }

  return input;
}

export function generateYearOptions(afterYears = 2, beforeYears = 5): string[] {
  const currentYear = new Date().getFullYear();
  const startYear = currentYear - beforeYears;
  const endYear = currentYear + afterYears;

  const years: string[] = [];

  for (let y = startYear; y <= endYear; y++) {
    years.push(formatAcademicYear(y));
  }

  return years;
}
