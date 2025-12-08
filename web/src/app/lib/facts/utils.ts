export function generateAcademicYearDict(
  from: number,
  to: number
): Record<number, string> {
  const yearDict: Record<number, string> = {};
  for (let year = from; year < to; year++) {
    yearDict[year] = `${year}-${year + 1}`;
  }
  return yearDict;
}
