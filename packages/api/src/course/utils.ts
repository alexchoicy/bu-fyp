export function generateYearOptions() {
  const currentYear = new Date().getFullYear();
  const startYear = currentYear - 5;
  const endYear = currentYear + 2;

  const years: number[] = [];

  for (let y = startYear; y <= endYear; y++) {
    years.push(y);
  }

  return years;
}
